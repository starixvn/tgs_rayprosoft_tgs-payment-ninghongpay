namespace Tgs.Payment.NinghongPay;
public record DepositOrder(
    decimal Amount,
    Guid FormId,    
    int Method,
    string? RealName);