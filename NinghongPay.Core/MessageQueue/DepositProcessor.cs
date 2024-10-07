using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Payment.NinghongPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.NinghongPay.MessageQueue;

internal class DepositProcessor(
    NinghongPayClient ninghongPayClient,    
    ILogger<DepositProcessor> logger)
    : IMessageProcessor<DepositRequest, DepositResponse>
{    
    public async ValueTask<DepositResponse> HandleAsync(
        string subject,
        DepositRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("data applyInfo:{applyInfo}", data.ApplyInfo);

        var settings = JsonSerializer.Deserialize<NinghongPayDepositSettings>(
            data.ApplyInfo,
            NinghongPayUtility.SerializerOptions)!;

        var order = new DepositOrder(
            decimal.Parse(data.Amount),
            settings.FormId,
            settings.Method,
            settings.RealName);

        var token = await ninghongPayClient
            .AuthAsync(settings.MerchantCode, settings.SecretKey, cancellationToken)
            .ConfigureAwait(false);

        var depositResponse = await ninghongPayClient
            .DepositAsync(token, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new NinghongPayDepositApplyInfo(
            settings,
            new NinghongPayDepositResponse(
                depositResponse.Amount,
                depositResponse.FormId,
                depositResponse.RedirectUrl,
                depositResponse.ThirdPartyOrderNo));

        return new DepositResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, NinghongPayUtility.SerializerOptions)
        };
    }
}