using System.Text.Json.Serialization;

namespace Tgs.Payment.NinghongPay.ViewModels;

public class DepositPayload
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("txid")]
    public required string Txid { get; init; }

    [JsonPropertyName("mer_tx")]
    public required string MerTx { get; init; }

    [JsonPropertyName("amount")]
    public required string Amount { get; init; }

    [JsonPropertyName("currency")]
    public required string Currency { get; init; }

    [JsonPropertyName("rate")]
    public required string Rate { get; init; }

    [JsonPropertyName("commission")]
    public required string Commission { get; init; }

    [JsonPropertyName("remark")]
    public required string Remark { get; init; }

    [JsonPropertyName("approved_at")]
    public string? ApprovedAt { get; init; }

    [JsonPropertyName("rejected_at")]
    public string? RejectedAt { get; init; }
}
