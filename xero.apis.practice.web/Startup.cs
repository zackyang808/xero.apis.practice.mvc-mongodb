using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Threading.Tasks;
using xero.apis.practice.common.Contracts;
using xero.apis.practice.common.Models;
using xero.apis.practice.common.MongoDB;
using xero.apis.practice.common.Services;
using xero.apis.practice.web.Extensions;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;

namespace xero.apis.practice.web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddHttpClient();

            services.TryAddSingleton(new XeroConfiguration
            {
                ClientId = Configuration["Xero:ClientId"],
                ClientSecret = Configuration["Xero:ClientSecret"]
            });

            services.TryAddSingleton<IXeroClient, XeroClient>();
            services.TryAddSingleton<IAccountingApi, AccountingApi>();
            services.TryAddTransient<ITokenService, TokenService>();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "XeroSignIn";
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "XeroIdentity";

                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = async context =>
                    {
                        var tokenService = context.HttpContext.RequestServices.GetService<ITokenService>();
                        var token = await tokenService.GetAccessTokenAsync(context.Principal.XeroUserId());

                        if (token == null)
                        {
                            context.RejectPrincipal();
                        }
                    }
                };
            })
            .AddOpenIdConnect("XeroSignIn", options =>
            {
                options.Authority = "https://identity.xero.com";

                options.ClientId = Configuration["Xero:ClientId"];
                options.ClientSecret = Configuration["Xero:ClientSecret"];

                options.ResponseType = "code";

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");

                options.CallbackPath = "/signin-oidc";

                options.Events = new OpenIdConnectEvents
                {
                    OnTokenValidated = OnTokenValidated()
                };
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMongoDB(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static Func<TokenValidatedContext, Task> OnTokenValidated()
        {
            return context =>
            {
                var tokenService = context.HttpContext.RequestServices.GetService<ITokenService>();

                var token = new XeroToken
                {
                    XeroUserId = context.Principal.XeroUserId(),
                    AccessToken = context.TokenEndpointResponse.AccessToken,
                    RefreshToken = context.TokenEndpointResponse.RefreshToken,
                    ExpiresAtUtc = DateTime.UtcNow.AddSeconds(Convert.ToDouble(context.TokenEndpointResponse.ExpiresIn))
                };

                tokenService.SetToken(token);

                return Task.CompletedTask;
            };
        }
    }
}
