using System.Globalization;
using System.Text;

namespace VidSrc;

/// <summary>Optional query parameters accepted by all VidSrc embed URLs.</summary>
public sealed record EmbedOptions
{
    /// <summary>Autoplay without a click. Only honored on custom domains.</summary>
    public bool? Autoplay { get; init; }

    /// <summary>TV only: automatically play the next episode after a countdown.</summary>
    public bool? AutoNext { get; init; }

    /// <summary>Resume position in seconds (mapped to <c>startAt</c>).</summary>
    public double? StartAtSeconds { get; init; }

    /// <summary>
    /// Preferred subtitle languages (ISO codes, max 3). The first available is used,
    /// e.g. <c>["en", "fr", "de"]</c>.
    /// </summary>
    public IReadOnlyList<string>? SubtitleLanguages { get; init; }

    /// <summary>
    /// URL of an external .vtt/.srt subtitle file. Must be https:// and served with CORS headers.
    /// </summary>
    public string? CustomSubtitleUrl { get; init; }

    /// <summary>Display label for the custom subtitle track.</summary>
    public string? CustomSubtitleLabel { get; init; }

    /// <summary>Language code for the custom subtitle track.</summary>
    public string? CustomSubtitleLanguage { get; init; }

    internal void AppendQueryParameters(StringBuilder sb)
    {
        var separator = '?';
        void Add(string key, string value, bool escape = true)
        {
            sb.Append(separator).Append(key).Append('=');
            sb.Append(escape ? Uri.EscapeDataString(value) : value);
            separator = '&';
        }

        if (Autoplay is { } autoplay) Add("autoplay", autoplay ? "1" : "0");
        if (AutoNext is { } autoNext) Add("autonext", autoNext ? "1" : "0");
        if (StartAtSeconds is { } startAt)
            Add("startAt", startAt.ToString("0.###", CultureInfo.InvariantCulture));
        if (SubtitleLanguages is { Count: > 0 } langs)
            Add("ds_lang", string.Join(',', langs.Take(3)), escape: false); // commas are valid in query strings
        if (CustomSubtitleUrl is { } subUrl) Add("sub_url", subUrl);
        if (CustomSubtitleLabel is { } subLabel) Add("sub_label", subLabel);
        if (CustomSubtitleLanguage is { } subLang) Add("sub_lang", subLang);
    }
}
