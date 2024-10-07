using System.Text.Json.Serialization;

namespace Tgs.Payment.NinghongPay;
public record BrandPaymentMerchantInfo(
    [property: JsonPropertyName("merchantCode")] string MerchantCode,
    [property: JsonPropertyName("secretKey")] string SecretKey);