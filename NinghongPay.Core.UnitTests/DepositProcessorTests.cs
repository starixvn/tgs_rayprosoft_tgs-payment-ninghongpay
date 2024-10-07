using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.Clients;
using Tgs.Payment.NinghongPay;
using Tgs.Payment.NinghongPay.Client;
using Tgs.Payment.NinghongPay.MessageQueue;
using Tgs.Payment.Protos;

namespace NinghongPay.Core.UnitTests;

public class DepositProcessorTests
{
    [Fact]
    public async Task 儲值要求處理()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");

        var fakePayClient = new NinghongPayClient(
            httpClient,
            new NinghongPayHasher(NullLogger<NinghongPayHasher>.Instance),
            NullLogger<NinghongPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var options = new NinghongPayOptions
        {
            PaymentApiUrl = new Uri("https://localhost"),
            PaymentServiceBaseUrl = new Uri("http://localhost:5541")
        };
        var formId = Guid.Parse("337dbbc7-d701-426e-b1d4-baf045ad7c21");
        var merchantCode = "merchantCode";
        var secretKey = "secretKey";

        var sut = new DepositProcessor(
            fakePayClient,            
            NullLogger<DepositProcessor>.Instance);

        var applySettings = new NinghongPayDepositSettings(
            merchantCode,
            secretKey,
            formId,
            3,
            "刘洋");

        var request = new DepositRequest
        {
            Amount = "10000",
            ApplyInfo = JsonSerializer.Serialize(applySettings, NinghongPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/api/v1/login")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "token": "OgGzajrkOUMgt2pGEgBikpu7ljCgILEgxrYQQKwc92cbc0aa"
                }
                """));

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/api/v1/payin")
            .Respond(HttpStatusCode.Created, new StringContent("""
                {
                    "success": true,
                    "payin": {
                        "user_id": "Pay_4315",
                        "merchant_id": 7,
                        "txid": "txin_5R8YlnYa",
                        "mer_tx": "337dbbc7-d701-426e-b1d4-baf045ad7c21",
                        "method": "online",
                        "method_id": 3,
                        "amount": "10000.00",
                        "commission": "0.00",
                        "currency": "rmb",
                        "rate": "4.60",
                        "rate_type": "fixed",
                        "status": "1",
                        "remark": "https:\/\/pay.colorwin.com\/pay\/alipay\/7FF78958-C064-42E2-B99D-58803C95FFBE",
                        "created_at": "2024-10-04T06:46:58.000000Z",
                        "updated_at": "2024-10-04T06:46:59.000000Z"
                    }
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "test",
            request,
            null);

        var applyInfo = JsonSerializer.Deserialize<NinghongPayDepositApplyInfo>(actual.Result, NinghongPayUtility.SerializerOptions);

        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
        Assert.Equal(
            new NinghongPayDepositResponse(
                10000m,
                applySettings.FormId,
                new Uri("https://pay.colorwin.com/pay/alipay/7FF78958-C064-42E2-B99D-58803C95FFBE"),
                "txin_5R8YlnYa"),
            applyInfo.Response);
    }
}