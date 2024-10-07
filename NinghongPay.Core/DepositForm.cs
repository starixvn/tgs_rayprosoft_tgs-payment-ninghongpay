using Tgs.Payment.Clients;
using Tgs.Payment.NinghongPay.Client;

namespace Tgs.Payment.NinghongPay;
public record DepositForm(
    Guid Id,
    string PlayerCode,
    Guid BrandId,
    Guid BrandPaymentId,
    decimal Amount,
    PaymentFlowState Status,
    NinghongPayDepositApplyInfo? ApplyInfo,
    string? PaymentReturn = null);