using System.Text;
using FluentAssertions;
using uOrgHub.Shared.Services.Attachments;

namespace uOrgHub.Tests.Shared;

public class FileSignatureTests
{
    [Theory]
    [InlineData(".pdf", new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31 })]
    [InlineData(".png", new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })]
    [InlineData(".jpg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 })]
    [InlineData(".jpeg", new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 })]
    [InlineData(".gif", new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 })]
    public void Matches_accepts_genuine_headers(string extension, byte[] header)
        => FileSignature.Matches(extension, header).Should().BeTrue();

    [Fact]
    public void Matches_accepts_webp_riff_container()
        => FileSignature.Matches(".webp", Encoding.ASCII.GetBytes("RIFF\0\0\0\0WEBPVP8 ")).Should().BeTrue();

    [Theory]
    [InlineData(".pdf")]
    [InlineData(".png")]
    [InlineData(".jpg")]
    [InlineData(".gif")]
    [InlineData(".webp")]
    public void Matches_rejects_renamed_executable(string extension)
        => FileSignature.Matches(extension, "MZ\u0090\0\u0003\0\0\0"u8).Should().BeFalse();

    [Fact]
    public void Matches_rejects_truncated_file()
        => FileSignature.Matches(".pdf", "%P"u8).Should().BeFalse();

    [Theory]
    [InlineData(".docx")]
    [InlineData(".csv")]
    [InlineData(".txt")]
    public void Matches_passes_formats_without_checked_signature(string extension)
        => FileSignature.Matches(extension, "anything"u8).Should().BeTrue();
}
