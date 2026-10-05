namespace VidSrc;

/// <summary>Configuration for <see cref="VidSrcClient"/>.</summary>
public sealed record VidSrcClientOptions
{
    /// <summary>The public VidSrc origin.</summary>
    public const string DefaultBaseUrl = "https://vidsrc.sh";

    /// <summary>
    /// Root of the VidSrc deployment. Point this at your own domain if you front
    /// VidSrc via a Cloudflare CNAME to <c>vidsrc-ip.com</c>.
    /// </summary>
    public string BaseUrl { get; init; } = DefaultBaseUrl;
}
