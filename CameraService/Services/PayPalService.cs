﻿using PayPal.Core;
using PayPal.v1.Payments;
using System.Net;
using CameraService.Services.IRepositoryServices;
using Microsoft.Extensions.Configuration;
using CameraCore.Models;
using System.Net.WebSockets;
using static System.Net.WebRequestMethods;
using Microsoft.AspNetCore.Http;

namespace CameraService.Services
{
    public class PayPalService : IPayPalService
    {
        private readonly IConfiguration _configuration;
        public PayPalService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public class PayPalPayment
        {
            public string url;
            public string statusCode;
            public string errorCode;
            public string Message;
        }

        public async Task<PayPalPayment> CreatePaymentUrl(PaymentInformationModel model)
        {
            var paypalOrderId = model.OrderId;
            var returnUrl = _configuration["PaymentCallBack:ReturnUrl"];
            var payment = new PayPal.v1.Payments.Payment()
            {
                Intent = "sale",
                Transactions = new List<Transaction>()
                {
                    new Transaction()
                    {
                        Amount = new Amount()
                        {
                            Total = model.Price.ToString(),
                            Currency = "USD",
                            Details = new AmountDetails
                            {
                                Tax = "0",
                                Shipping = "0",
                                Subtotal = model.Price.ToString()
                            }
                        },
                        ItemList = new ItemList()
                        {
                            Items = new List<Item>()
                            {
                                new Item()
                                {
                                    Name = " | Order: " + model.OrderDetails[0].Camera.Name,
                                    Currency = "USD",
                                    Price = model.Price.ToString(),
                                    Quantity = 1.ToString(),
                                    Sku = "sku",
                                    Tax = "0",
                                    Url = "https://www.code-mega.com" // Url detail of Item
                                }

                            }
                        },
                        Description = $"Invoice #{model.Message}",
                        InvoiceNumber = paypalOrderId.ToString()
                    }
                },
                RedirectUrls = new RedirectUrls()
                {
                    ReturnUrl =
                       $"{returnUrl}?payment_method=PayPal&success=1&order_id={paypalOrderId}",
                    CancelUrl =
                        $"{returnUrl}?payment_method=PayPal&success=0&order_id={paypalOrderId}"
                },
                Payer = new Payer()
                {
                    PaymentMethod = "paypal"
                }
            };

            return await ExecutePaymentAsync(payment);
        }

        public async Task<PayPalPayment> ExecutePaymentAsync(Payment payment)
        {
            var envSandbox = new SandboxEnvironment(_configuration["Paypal:ClientId"], _configuration["Paypal:SecretKey"]);
            var client = new PayPalHttpClient(envSandbox);

            var request = new PaymentCreateRequest();
            request.RequestBody(payment);

            var paymentUrl = "";
            var response = await client.Execute(request);
            var statusCode = response.StatusCode;

            if (statusCode is not (HttpStatusCode.Accepted or HttpStatusCode.OK or HttpStatusCode.Created))
            {
                var errorResponse = new PayPalPayment
                {
                    url = null,
                    statusCode = ((int)statusCode).ToString(),
                    errorCode = null,
                    Message = "Invalid status code: " + statusCode.ToString()
                };

                return errorResponse;
            }
            var result = response.Result<Payment>();
            using var links = result.Links.GetEnumerator();

            
            while (links.MoveNext())
            {
                var lnk = links.Current;
                if (lnk == null) continue;
                if (!lnk.Rel.ToLower().Trim().Equals("approval_url")) continue;
                paymentUrl = lnk.Href;
            }
            var reponsePayPal = new PayPalPayment
            {
                url = paymentUrl,
                statusCode = ((int)statusCode).ToString(),
                errorCode = null,
                Message = "Success"
            };

            return reponsePayPal;
        }

    }
}
