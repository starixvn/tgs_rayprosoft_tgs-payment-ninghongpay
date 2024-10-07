namespace Tgs.Payment.NinghongPay.Client;
public record NinghongPayWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo);