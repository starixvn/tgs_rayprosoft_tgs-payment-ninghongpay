namespace Tgs.Payment.NinghongPay.Client;
public record NinghongPayWithdrawApplyInfo(
    NinghongPayWithdrawSettings Settings,
    NinghongPayWithdrawResponse Response);