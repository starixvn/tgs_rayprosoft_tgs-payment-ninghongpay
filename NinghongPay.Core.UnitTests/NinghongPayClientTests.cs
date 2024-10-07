using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Tgs.Payment.NinghongPay;

namespace NinghongPay.Core.UnitTests;

public class NinghongPayClientTests
{
    [Fact]
    public async Task Auth()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new NinghongPayHasher(NullLogger<NinghongPayHasher>.Instance);

        var sut = new NinghongPayClient(httpClient, signer, NullLogger<NinghongPayClient>.Instance);

        var merchantCode = "merchantCode";
        var secretKey = "secretKey";

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/api/v1/login")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {
                    "token": "OgGzajrkOUMgt2pGEgBikpu7ljCgILEgxrYQQKwc92cbc0aa"
                }
                """));

        var actual = await sut.AuthAsync(merchantCode, secretKey);

        Assert.Equal("OgGzajrkOUMgt2pGEgBikpu7ljCgILEgxrYQQKwc92cbc0aa", actual);
    }

    [Fact]
    public async Task 餘額查詢()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new NinghongPayHasher(NullLogger<NinghongPayHasher>.Instance);

        var sut = new NinghongPayClient(httpClient, signer, NullLogger<NinghongPayClient>.Instance);

        var merchantCode = "merchantCode";
        var token = "token";

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

        var actual = await sut.QueryBalanceAsync(merchantCode, token);

        Assert.Equal(0, actual);
    }

    [Fact]
    public async Task 代收()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new NinghongPayHasher(NullLogger<NinghongPayHasher>.Instance);

        var sut = new NinghongPayClient(httpClient, signer, NullLogger<NinghongPayClient>.Instance);

        var formId = Guid.Parse("337dbbc7-d701-426e-b1d4-baf045ad7c21");
        var token = "token";

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

        var order = new DepositOrder(
           10000,
           formId,
           3,
           "刘洋");

        var actual = await sut.DepositAsync(token, order);

        Assert.Equal("txin_5R8YlnYa", actual.ThirdPartyOrderNo);
        Assert.Equal(new Uri("https://pay.colorwin.com/pay/alipay/7FF78958-C064-42E2-B99D-58803C95FFBE"), actual.RedirectUrl);
    }

    [Fact]
    public async Task 代收查詢()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new NinghongPayHasher(NullLogger<NinghongPayHasher>.Instance);

        var sut = new NinghongPayClient(httpClient, signer, NullLogger<NinghongPayClient>.Instance);

        var formId = Guid.Parse("337dbbc7-d701-426e-b1d4-baf045ad7c21");
        var token = "token";

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/api/v1/payincheck")
            .Respond(HttpStatusCode.Created, new StringContent("""
                {
                    "success": true,
                    "payin": {
                        "user_id": "Pay_4315",
                        "merchant_code": "Pay_991837",
                        "txid": "txin_5R8YlnYa",
                        "mer_tx": "337dbbc7-d701-426e-b1d4-baf045ad7c21",
                        "method": "online",
                        "method_id": 3,
                        "amount": "10000.00",
                        "commission": "0.00",
                        "currency": "rmb",
                        "rate": "4.60",
                        "rate_type": "fixed",
                        "status": "Rejected",
                        "remark": "https:\/\/pay.colorwin.com\/pay\/alipay\/7FF78958-C064-42E2-B99D-58803C95FFBE",
                        "created_at": "2024-10-04T06:46:58.000000Z",
                        "updated_at": "2024-10-05T06:50:02.000000Z"
                    }
                }
                """));     

        var actual = await sut.QueryDepositFormAsync(token, Guid.Parse("337dbbc7-d701-426e-b1d4-baf045ad7c21"));

        Assert.Equal("Rejected", actual);        
    }

    [Fact]
    public async Task 代付()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new NinghongPayHasher(NullLogger<NinghongPayHasher>.Instance);

        var sut = new NinghongPayClient(httpClient, signer, NullLogger<NinghongPayClient>.Instance);

        var formId = Guid.Parse("08df278b-4e81-4bc1-8956-7205d6020a38");
        var token = "token";

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

        var order = new WithdrawOrder(
           formId,
           100000m,
           3,
           "Maybank",
           "123456789",
           "LIM LUI KIM");

        var actual = await sut.WithdrawAsync(token, order);

        Assert.Equal(100000m, actual.Amount);
        Assert.Equal(formId, actual.FormId);
        Assert.Equal("txout_SstNW2k1", actual.ThirdPartyOrderNo);
    }
}