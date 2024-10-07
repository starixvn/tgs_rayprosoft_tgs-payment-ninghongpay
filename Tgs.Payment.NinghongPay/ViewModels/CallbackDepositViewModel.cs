using System.Text.Json.Serialization;

namespace Tgs.Payment.NinghongPay.ViewModels;

public class CallbackDepositViewModel
{
    [JsonPropertyName("responseurl")]
    public required string Responseurl { get; init; }

    [JsonPropertyName("payload")]
    public DepositPayload Payload { get; init; } = default!;
}
