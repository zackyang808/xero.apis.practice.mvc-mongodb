using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xero.apis.practice.common.Contracts;
using xero.apis.practice.web.Extensions;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Model;

namespace xero.apis.practice.web.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly IXeroClient _xeroClient;
        private readonly IAccountingApi _accountingApi;

        public PaymentsController(ITokenService tokenService, IXeroClient xeroClient, IAccountingApi accountingApi)
        {
            _tokenService = tokenService;
            _xeroClient = xeroClient;
            _accountingApi = accountingApi;
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(FromFormAttribute formData)
        {
            var token = await _tokenService.GetAccessTokenAsync(User.XeroUserId());
            var connections = await _xeroClient.GetConnectionsAsync(token);
            var tenantId = connections[0].TenantId.ToString();

            Payment payment = new Payment()
            {
                Invoice = new Invoice()
                {
                    InvoiceNumber= "INV-0002"
                },
                Account = new Account()
                {
                    Code = "200"
                },
                Date = DateTime.Now,
                Amount = 3
            };

            await _accountingApi.CreatePaymentAsync(token.AccessToken, tenantId, payment);

            return RedirectToAction("Index", "Invoices");
        }
    }
}