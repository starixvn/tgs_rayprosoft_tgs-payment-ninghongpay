using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.Clients;
using Tgs.Payment.NinghongPay.Client;
using Xunit.Abstractions;

namespace Tgs.Payment.NinghongPay.IntegrationTests;

public class DepositTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));

    [Fact]
    public async Task 代收回調_訂單狀態成功()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = NinghongPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("4248c27c-7caa-4aef-9b5a-a5d63c2c9bd7");
        var brandId = Guid.NewGuid();
        var secretKey = "secretKey";

        var request = """
            {
                "responseurl": "https:\/\/4d9hrtdf-7299.asse.devtunnels.ms\/deposit\/callback",
                "payload": {
                    "status": "success",
                    "txid": "txin_Unm5MCw8",
                    "mer_tx": "4248c27c-7caa-4aef-9b5a-a5d63c2c9bd7",
                    "amount": "10000.00",
                    "currency": "rmb",
                    "rate": "4.60",
                    "commission": "0.00",
                    "remark": "https:\/\/pay.colorwin.com\/pay\/alipay\/53306B88-E5E6-41F6-99E4-E091B10B2730",
                    "approved_at": "2024-10-07 10:31:26"
                }
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var applyInfo = new NinghongPayDepositApplyInfo(
            new NinghongPayDepositSettings(
                "merchantCode",
                secretKey,
                formId,
                3,
                "\u5218\u6D0B"),
             new NinghongPayDepositResponse(
                10000.00m,
                formId,
                new Uri("https://pay.colorwin.com/pay/alipay/53306B88-E5E6-41F6-99E4-E091B10B2730"),
                "2e042362-41dd-4fad-95bb-e6cee2216251"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitPayment,
                Amount = 10000.00m,
                ThirdPartyOrderNo = "txin_Unm5MCw8",
                ApplyInfo = JsonSerializer.Serialize(applyInfo, NinghongPayUtility.SerializerOptions)
            });

        var response = await httpClient.PostAsync("/deposit/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("success", actual);     

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .ApproveDepositAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Is(10000.00m),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task 代收回調_訂單狀態失敗()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = NinghongPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(_testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("93920172-d09b-45ec-a066-7262b0686d54");
        var brandId = Guid.NewGuid();
        var secretKey = "secretKey";

        var request = """
            {
                "responseurl": "https:\/\/4d9hrtdf-7299.asse.devtunnels.ms\/deposit\/callback",
                "payload": {
                    "status": "error",
                    "txid": "txin_IycprlTB",
                    "mer_tx": "93920172-d09b-45ec-a066-7262b0686d54",
                    "amount": "10000.00",
                    "currency": "rmb",
                    "rate": "4.60",
                    "commission": "0.00",
                    "remark": "https:\/\/pay.colorwin.com\/pay\/alipay\/18DFF16A-648B-4351-B53A-943A6B8D8B55",
                    "rejected_at": "2024-10-07T03:34:15.809848Z"
                }
            }            
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var applyInfo = new NinghongPayDepositApplyInfo(
            new NinghongPayDepositSettings(
                "merchantCode",
                secretKey,
                formId,
                3,
                "\u5218\u6D0B"),
             new NinghongPayDepositResponse(
                10000.00m,
                formId,
                new Uri("https://pay.colorwin.com/pay/alipay/18DFF16A-648B-4351-B53A-943A6B8D8B55"),
                "txin_IycprlTB"));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitPayment,
                Amount = 10000.00m,
                ThirdPartyOrderNo = "txin_Unm5MCw8",
                ApplyInfo = JsonSerializer.Serialize(applyInfo, NinghongPayUtility.SerializerOptions)
            });

        var response = await httpClient.PostAsync("/deposit/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("success", actual);

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Any<string>(),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .RejectDepositAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Any<CancellationToken>());
    }
}