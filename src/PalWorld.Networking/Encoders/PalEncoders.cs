namespace PalWorld.Networking;

/// <summary>
/// Represents the various encoders included with the library
/// </summary>
public static class PalEncoders
{
    /// <summary>
    /// The default encoder used by the library
    /// </summary>
    public static IEncoderService Default { get; } = Utf8!;

    /// <summary>
    /// The UTF-8 encoder
    /// </summary>
    public static IEncoderService Utf8 { get; } = new Utf8EncoderService();

    /// <summary>
    /// The UTF-16 encoder
    /// </summary>
    public static IEncoderService Utf16 { get; } = new Utf16EncoderService();

    /// <summary>
    /// The UTF-32 encoder
    /// </summary>
    public static IEncoderService Utf32 { get; } = new Utf32EncoderService();

    /// <summary>
    /// The Base64 encoder
    /// </summary>
    public static IEncoderService Base64 { get; } = new Base64EncoderService();
}
