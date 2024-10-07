using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Tgs.Payment.Clients;
using Tgs.Payment.NinghongPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.NinghongPay.MessageQueue;

internal class WithdrawProcessor(
    NinghongPayClient ninghongPayClient,
    IPaymentClient paymentClient)
    : IMessageProcessor<WithdrawRequest, WithdrawResponse>
{
    public async ValueTask<WithdrawResponse> HandleAsync(
        string subject,
        WithdrawRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        var settings = JsonSerializer.Deserialize<NinghongPayWithdrawSettings>(data.ApplyInfo, NinghongPayUtility.SerializerOptions)!;

        var order = new WithdrawOrder(
            settings.FormId,
            decimal.Parse(data.Amount),
            settings.Method,
            settings.BankName,
            settings.AccountNo,
            settings.AccountName);

        var token = await ninghongPayClient
            .AuthAsync(settings.MerchantCode, settings.SecretKey, cancellationToken)
            .ConfigureAwait(false);

        var withdrawResponse = await ninghongPayClient
            .WithdrawAsync(token, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new NinghongPayWithdrawApplyInfo(
            settings,
            new NinghongPayWithdrawResponse(withdrawResponse.FormId, withdrawResponse.ThirdPartyOrderNo));

        await paymentClient
            .SaveWithdrawFormThirdPartyOrderNoAsync(
                settings.FormId,
                withdrawResponse.ThirdPartyOrderNo,
                cancellationToken)
            .ConfigureAwait(false);

        return new WithdrawResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, NinghongPayUtility.SerializerOptions)
        };
    }
}