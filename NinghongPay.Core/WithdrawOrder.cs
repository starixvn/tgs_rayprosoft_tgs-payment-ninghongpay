namespace Tgs.Payment.NinghongPay;
public record WithdrawOrder(
    Guid FormId,
    decimal Amount,    
    int Method,
    string BankName,
    string AccountNo,
    string AccountName);