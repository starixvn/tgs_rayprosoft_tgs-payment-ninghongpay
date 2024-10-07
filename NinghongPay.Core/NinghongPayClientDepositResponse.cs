namespace Tgs.Payment.NinghongPay;
public record NinghongPayClientDepositResponse(
    decimal Amount,
    Guid FormId,
    string ThirdPartyOrderNo,
    Uri RedirectUrl);