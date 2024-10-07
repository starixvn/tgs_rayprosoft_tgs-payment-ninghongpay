using System.Text.Json.Serialization;

namespace Tgs.Payment.NinghongPay.ViewModels;

public class CallbackWithdrawViewModel
{
    [JsonPropertyName("responseurl")]
    public required string Responseurl { get; init; }

    [JsonPropertyName("payload")]
    public WithdrawPayload Payload { get; init; } = default!;
}