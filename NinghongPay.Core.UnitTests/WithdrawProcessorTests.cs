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
public class WithdrawProcessorTests
{
    [Fact]
    public async Task 代付()
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

        var sut = new WithdrawProcessor(
            fakePayClient,            
            fakePaymentClient);

        var formId = Guid.Parse("08df278b-4e81-4bc1-8956-7205d6020a38");
        var merchantCode = "merchantCode";
        var secretKey = "secretKey";

        var applySettings = new NinghongPayWithdrawSettings(
            merchantCode,
            secretKey,
            formId,
            3,
            "Maybank",
            "123456789",
            "LIM LUI KIM");

        var request = new WithdrawRequest
        {
            Amount = "100000",
            ApplyInfo = JsonSerializer.Serialize(applySettings, NinghongPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/api/v1/login")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "token": "OgGzajrkOUMgt2pGEgBikpu7ljCgILEgxrYQQKwc92cbc0aa"
                }
                """));

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/api/v1/payout")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "success": true,
                    "payout": {
                        "user_id": "Pay_5694",
                        "merchant_id": 7,
                        "txid": "txout_SstNW2k1",
                        "mer_tx": "08df278b-4e81-4bc1-8956-7205d6020a38",
                        "method": "local",
                        "method_id": 3,
                        "amount": "100000",
                        "commission": "0.00",
                        "currency": "rmb",
                        "rate": "0.00",
                        "rate_type": "fixed",
                        "bankname": "Maybank",
                        "accname": "LIM LUI KIM",
                        "accno": "123456789",
                        "status": "1",
                        "remark": null,
                        "created_at": "2024-10-04T09:19:47.938661Z",
                        "updated_at": "2024-10-04T09:19:47.938667Z"
                    }
                }
                """));

        // Act
        var actual = await sut.HandleAsync(
            "test",
            request,
            null);

        var applyInfo = JsonSerializer.Deserialize<NinghongPayWithdrawApplyInfo>(actual.Result, NinghongPayUtility.SerializerOptions);

        // Assert
        _ = fakePaymentClient.Received(1)
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                Arg.Is(applySettings.FormId),
                Arg.Is("txout_SstNW2k1"));
        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
    }
}
