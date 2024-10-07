namespace Tgs.Payment.NinghongPay.Client;
public record NinghongPayDepositResponse(
    decimal Amount,
    Guid FormId,
    Uri RedirectUrl,
    string ThirdPartyOrderNo);