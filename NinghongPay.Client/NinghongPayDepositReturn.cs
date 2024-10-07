namespace Tgs.Payment.NinghongPay.Client;
public record NinghongPayDepositReturn
{
    public decimal OrderAmount { get; init; }
    public Guid FormId { get; init; }

    public string Status { get; init; } = default!;

    public string ResponseUrl { get; init; } = default!;

    public string Currency { get; init; } = default!;
    public decimal Rate { get; init; }

    public decimal Commission { get; init; }

    public string? ApprovedAt { get; init; }
    public string? RejectedAt { get; init; }
}