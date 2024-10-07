using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Tgs.Payment.NinghongPay;
using Tgs.Payment.NinghongPay.MessageQueue;
using Tgs.Payment.Protos;

namespace NinghongPay.Core.UnitTests;
public class GetBalanceProcessorTests
{
    [Fact]
    public async Task GetBalanceProcessor_取得商戶餘額()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");        

        var fakePayClient = new NinghongPayClient(
            httpClient,
            new NinghongPayHasher(NullLogger<NinghongPayHasher>.Instance),            
            NullLogger<NinghongPayClient>.Instance);

        var sut = new GetBalanceProcessor(fakePayClient);

        var request = new PaymentBalanceRequest
        {
            BrandPaymentSettings = """
                {
                    "merchantCode": "merchantCode",
                    "secretKey": "secretKey"
                }
                """
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/api/v1/login")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "token": "OgGzajrkOUMgt2pGEgBikpu7ljCgILEgxrYQQKwc92cbc0aa"
                }
                """));

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/api/v1/check-balance")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "success": true,
                    "balances": [
                        {
                            "currency": "RMB",
                            "amount": "0.00",
                            "freeze": "0.00"
                        }
                    ]
                }
                """));

        // Act
        var actual = await sut.HandleAsync("test", request, null);

        // Assert
        Assert.Equal(0M, actual.Balance.ToDecimal());
    }
}
