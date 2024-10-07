namespace Tgs.Payment.NinghongPay;
public record NinghongPayClientWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo,
    decimal Amount);