using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using xero.apis.practice.common.Contracts;
using xero.apis.practice.web.Extensions;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Model;

namespace xero.apis.practice.web.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly ITokenService _tokenService;
        private readonly IXeroClient _xeroClient;
        private readonly IAccountingApi _accountingApi;

        public InvoicesController(ITokenService tokenService, IXeroClient xeroClient, IAccountingApi accountingApi)
        {
            _tokenService = tokenService;
            _xeroClient = xeroClient;
            _accountingApi = accountingApi;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var token = await _tokenService.GetAccessTokenAsync(User.XeroUserId());
            var connections = await _xeroClient.GetConnectionsAsync(token);
            var tenantId = connections[0].TenantId.ToString();

            var invoices = await _accountingApi.GetInvoicesAsync(token.AccessToken, tenantId);
            return View(invoices._Invoices);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(IFormCollection formData)
        {
            var token = await _tokenService.GetAccessTokenAsync(User.XeroUserId());
            var connections = await _xeroClient.GetConnectionsAsync(token);
            var tenantId = connections[0].TenantId.ToString();

            Invoice invoice = new Invoice
            {
                InvoiceID = Guid.NewGuid(),
                Type = Invoice.TypeEnum.ACCREC,
                Contact = new Contact
                {
                    Name = formData["name"],
                    Addresses = new List<Address>()
                    {
                        new Address()
                        {
                            AddressType = Address.AddressTypeEnum.POBOX,
                            AddressLine1 = formData["addressline1"],
                            AddressLine2 = formData["addressline2"],
                            City=formData["city"],
                            Country="New Zealand",
                            PostalCode=formData["postalcode"]
                        }
                    }
                },
                DueDate = new DateTime(2020, 12, 31),
                LineItems = new List<LineItem>()
                {
                    new LineItem()
                    {
                        Description = formData["description"],
                        Quantity = double.Parse(formData["quantity"]),
                        UnitAmount = double.Parse(formData["unitAmount"]),
                        AccountCode = "200"
                    }
                },
                Status = Invoice.StatusEnum.AUTHORISED
            };

            var invoices = await _accountingApi.CreateInvoiceAsync(token.AccessToken, tenantId, invoice);

            return RedirectToAction("InvoiceView", new { invoiceId = invoices._Invoices[0].InvoiceID });
        }

        [Authorize]
        public async Task<IActionResult> InvoiceView([FromQuery] Guid invoiceId)
        {
            var token = await _tokenService.GetAccessTokenAsync(User.XeroUserId());
            var connections = await _xeroClient.GetConnectionsAsync(token);
            var tenantId = connections[0].TenantId.ToString();

            var invoices = await _accountingApi.GetInvoiceAsync(token.AccessToken, tenantId, invoiceId);
            return View(invoices._Invoices.FirstOrDefault());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> InvoiceView([FromQuery] Guid invoiceId, IFormCollection formData)
        {
            var token = await _tokenService.GetAccessTokenAsync(User.XeroUserId());
            var connections = await _xeroClient.GetConnectionsAsync(token);
            var tenantId = connections[0].TenantId.ToString();

            Payment payment = new Payment()
            {
                Invoice = new Invoice()
                {
                    InvoiceID = invoiceId
                },
                Account = new Account()
                {
                    Code = "200"
                },
                Date = DateTime.Now,
                Amount = double.Parse(formData["amount"])
            };

            await _accountingApi.CreatePaymentAsync(token.AccessToken, tenantId, payment);

            return RedirectToAction("Index");
        }

    }
}