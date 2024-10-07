using Tgs.Payment.Clients;
using Tgs.Payment.NinghongPay.Client;

namespace Tgs.Payment.NinghongPay;
public record WithdrawForm(
      Guid Id,
      string? UserId,
      UserType UserType,
      Guid BrandId,
      Guid? BrandPaymentId,
      decimal Amount,
      PaymentFlowState Status,
      Guid BankInfoId,
      string? BankInfo,
      NinghongPayWithdrawApplyInfo? ApplyInfo,
      string? PaymentReturn = null);