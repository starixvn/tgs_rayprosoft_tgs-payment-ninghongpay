using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.NinghongPay.Client;
using Tgs.Payment.Clients;
using Xunit.Abstractions;

namespace Tgs.Payment.NinghongPay.IntegrationTests;

public class WithdrawTests(ITestOutputHelper testOutputHelper)
{    
    [Fact]
    public async Task 代付回調_訂單狀態失敗()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = NinghongPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("5fab3642-04a9-4c33-9731-50ab5b836a4e");
        var brandId = Guid.NewGuid();
        var merchantCode = "merchantCode";
        var secretKey = "secretKey";

        var settings = new NinghongPayWithdrawSettings(
            merchantCode,
            secretKey,
            formId,
            3,
            "Maybank",
            "123456789",
            "LIM LUI KIM");
        var withdrawResponse = new NinghongPayWithdrawResponse(formId, "txout_lBjMajF2");

        var applyInfo = new NinghongPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, NinghongPayUtility.SerializerOptions)
            });

        var request = """
            {
                "responseurl": "https:\/\/4d9hrtdf-7299.asse.devtunnels.ms\/withdraw\/callback",
                "payload": {
                    "status": "error",
                    "txid": "txout_lBjMajF2",
                    "mer_tx": "5fab3642-04a9-4c33-9731-50ab5b836a4e",
                    "amount": "100000.00",
                    "currency": "rmb",
                    "commission": "0.00",
                    "remark": null,
                    "rejected_at": "2024-10-07T05:28:36.234349Z"
                }
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("success", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .RejectWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 代付回調_訂單狀態成功()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = NinghongPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("897081a0-7438-43d2-bcdf-5c17db20aee0");
        var brandId = Guid.NewGuid();
        var merchantCode = "merchantCode";
        var secretKey = "secretKey";

        var settings = new NinghongPayWithdrawSettings(
            merchantCode,
            secretKey,
            formId,
            3,
            "Maybank",
            "123456789",
            "LIM LUI KIM");
        var withdrawResponse = new NinghongPayWithdrawResponse(formId, "txout_mm9gfkbp");

        var applyInfo = new NinghongPayWithdrawApplyInfo(settings, withdrawResponse);

        _ = fakePaymentServiceClient.GetWithdrawFormAsync(
                Arg.Is(formId),
                Arg.Any<CancellationToken>())
            .Returns(new WithdrawFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.Approved,
                ApplyInfo = JsonSerializer.Serialize(applyInfo, NinghongPayUtility.SerializerOptions)
            });

        var request = """
            {
                "responseurl": "https:\/\/4d9hrtdf-7299.asse.devtunnels.ms\/withdraw\/callback",
                "payload": {
                    "status": "success",
                    "txid": "txout_mm9gfkbp",
                    "mer_tx": "897081a0-7438-43d2-bcdf-5c17db20aee0",
                    "amount": "100.00",
                    "currency": "rmb",
                    "commission": "0.00",
                    "remark": null,
                    "approved_at": "2024-10-07 14:31:24"
                }
            }            
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var response = await httpClient.PostAsync("/withdraw/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        var paymentReturn = new NinghongPayWithdrawReturn()
        {
            FormId = formId,
            OrderAmount = decimal.Parse("100.00"),
           
        };

        Assert.Equal("success", actual);
        _ = fakePaymentServiceClient.Received(1)
            .SubmitWithdrawFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .PaidWithdrawAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }
}
