using System.Text.Json;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.NinghongPay.MessageQueue;

internal class GetBalanceProcessor(NinghongPayClient ninghongPayClient)
    : IMessageProcessor<PaymentBalanceRequest, PaymentBalanceResponse>
{
    public async ValueTask<PaymentBalanceResponse> HandleAsync(
        string subject,
        PaymentBalanceRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        var settings = JsonSerializer.Deserialize<BrandPaymentMerchantInfo>(
            data.BrandPaymentSettings)!;

        var token = await ninghongPayClient.AuthAsync(settings.MerchantCode, settings.SecretKey, cancellationToken)
            .ConfigureAwait(false);

        var result = await ninghongPayClient.QueryBalanceAsync(
            token,
            settings.MerchantCode,
            cancellationToken).ConfigureAwait(false);

        return new PaymentBalanceResponse
        {
            Balance = result
        };
    }
}