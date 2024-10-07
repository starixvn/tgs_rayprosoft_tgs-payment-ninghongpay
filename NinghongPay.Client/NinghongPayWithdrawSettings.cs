namespace Tgs.Payment.NinghongPay.Client;
public record NinghongPayWithdrawSettings(
    string MerchantCode,
    string SecretKey,
    Guid FormId,
    int Method,
    string BankName,
    string AccountNo,
    string AccountName);