using FluentAssertions;
using uOrgHub.Sms.Models.Enums;
using uOrgHub.Sms.Services;

namespace uOrgHub.Tests.Sms;

public class SmsSegmentCalculatorTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(160, 1)]
    [InlineData(161, 2)]
    [InlineData(306, 2)]
    [InlineData(307, 3)]
    public void Gsm_text_splits_at_160_then_153(int length, int segments)
    {
        var info = SmsSegmentCalculator.Calculate(new string('a', length));
        info.Encoding.Should().Be(SmsEncoding.Gsm7);
        info.Segments.Should().Be(segments);
    }

    [Theory]
    [InlineData(70, 1)]
    [InlineData(71, 2)]
    [InlineData(134, 2)]
    [InlineData(135, 3)]
    public void Unicode_text_splits_at_70_then_67(int length, int segments)
    {
        var info = SmsSegmentCalculator.Calculate(new string('অ', length));
        info.Encoding.Should().Be(SmsEncoding.Unicode);
        info.Segments.Should().Be(segments);
    }

    [Fact]
    public void Extension_characters_cost_two_septets()
    {
        var info = SmsSegmentCalculator.Calculate(new string('a', 158) + "€");
        info.Encoding.Should().Be(SmsEncoding.Gsm7);
        info.Units.Should().Be(160);
        info.Segments.Should().Be(1);
        SmsSegmentCalculator.Calculate(new string('a', 159) + "€").Segments.Should().Be(2);
    }

    /// <summary>An escape pair can't be split across parts, so a part ends one septet early.</summary>
    [Fact]
    public void Extension_character_is_not_split_across_parts()
    {
        // 152 'a' + '[' + 152 'a' = 306 septets. Plain division says ceil(306/153) = 2 parts, but
        // '[' (2 septets) doesn't fit after the first 152, so it opens part 2 and the tail spills
        // into a third.
        var text = new string('a', 152) + "[" + new string('a', 152);
        SmsSegmentCalculator.Calculate(text).Segments.Should().Be(3);
    }

    [Fact]
    public void One_non_gsm_character_makes_the_whole_message_unicode()
    {
        var info = SmsSegmentCalculator.Calculate(new string('a', 100) + "’");
        info.Encoding.Should().Be(SmsEncoding.Unicode);
        info.Segments.Should().Be(2);
    }

    [Fact]
    public void Surrogate_pairs_count_two_units_and_stay_together()
    {
        SmsSegmentCalculator.Calculate("😀").Units.Should().Be(2);
        // 66 units then an emoji (2) would straddle the 67-unit boundary: it moves to part 2.
        var text = new string('অ', 66) + "😀" + new string('অ', 66);
        SmsSegmentCalculator.Calculate(text).Segments.Should().Be(3);
    }
}

public class SmsPhoneNumberTests
{
    [Theory]
    [InlineData("01712345678", "8801712345678")]
    [InlineData("8801712345678", "8801712345678")]
    [InlineData("+8801712345678", "8801712345678")]
    [InlineData("008801712345678", "8801712345678")]
    [InlineData(" 017-1234 5678 ", "8801712345678")]
    [InlineData("+14155550123", "14155550123")]
    public void Valid_numbers_normalise_to_one_form(string input, string expected)
        => SmsPhoneNumber.Normalize(input).Should().Be(expected);

    [Theory]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("01212345678")]   // 012 is not a mobile operator prefix
    [InlineData("0171234567")]    // one digit short
    [InlineData("abc")]
    public void Invalid_numbers_are_rejected(string input)
        => SmsPhoneNumber.Normalize(input).Should().BeNull();

    [Fact]
    public void Lists_are_split_deduplicated_and_invalid_entries_reported()
    {
        var (valid, invalid) = SmsPhoneNumber.Parse(["01712345678, +8801712345678\n01812345678; nope"]);
        valid.Should().Equal("8801712345678", "8801812345678");
        invalid.Should().Equal("nope");
    }

    [Fact]
    public void Masking_hides_the_middle_digits()
        => SmsPhoneNumber.Mask("8801712345678").Should().Be("88017****5678");
}

public class SmsCredentialProtectorTests
{
    [Fact]
    public void Round_trips_and_does_not_store_plain_text()
    {
        var protector = new SmsCredentialProtector(new byte[32]);
        var cipher = protector.Protect(new Dictionary<string, string> { ["api_key"] = "super-secret-value" });

        cipher.Should().StartWith("v1:").And.NotContain("super-secret-value");
        protector.Unprotect(cipher).Should().ContainKey("api_key").WhoseValue.Should().Be("super-secret-value");
    }

    [Fact]
    public void A_different_key_cannot_read_it()
    {
        var cipher = new SmsCredentialProtector(new byte[32]).Protect(new Dictionary<string, string> { ["k"] = "v" });
        var other = new SmsCredentialProtector(Enumerable.Repeat((byte)7, 32).ToArray());

        var act = () => other.Unprotect(cipher);
        act.Should().Throw<System.Security.Cryptography.CryptographicException>();
    }

    [Fact]
    public void Each_encryption_uses_a_fresh_nonce()
    {
        var protector = new SmsCredentialProtector(new byte[32]);
        var secrets = new Dictionary<string, string> { ["k"] = "v" };
        protector.Protect(secrets).Should().NotBe(protector.Protect(secrets));
    }
}
