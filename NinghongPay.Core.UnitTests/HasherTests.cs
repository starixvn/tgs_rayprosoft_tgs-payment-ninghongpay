using Microsoft.Extensions.Logging.Abstractions;
using Tgs.Payment.NinghongPay;

namespace NinghongPay.Core.UnitTests;

public class HasherTests
{
    [Fact]
    public void Auth()
    {
        var merchant_code = "merchant_code";
        var secret = "secret";

        var sut = new NinghongPayHasher(NullLogger<NinghongPayHasher>.Instance);

        var actual = sut.ComputeHash($"{merchant_code}{secret}");

        var expected = "a7a6a5a0a78b462dccfc0241787d9cce";

        Assert.Equal(expected, actual);
    }
}