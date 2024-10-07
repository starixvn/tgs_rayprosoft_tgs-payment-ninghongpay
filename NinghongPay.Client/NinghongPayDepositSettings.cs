namespace Tgs.Payment.NinghongPay.Client;
public record NinghongPayDepositSettings(
    string MerchantCode,
    string SecretKey,
    Guid FormId,
    int Method,
    string RealName);