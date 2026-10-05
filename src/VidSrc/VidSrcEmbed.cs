using System.Text;

namespace VidSrc;

/// <summary>
/// Builds VidSrc iframe embed URLs for movies and TV episodes.
/// <para>
/// The <c>id</c> arguments accept either an IMDB id (<c>tt1300854</c>) or a
/// numeric TMDB id (<c>1399</c>) — VidSrc detects the format.
/// </para>
/// </summary>
public sealed class VidSrcEmbed
{
    private readonly string _baseUrl;

    /// <summary>Creates a builder rooted at the given VidSrc base URL.</summary>
    public VidSrcEmbed(string baseUrl = VidSrcClientOptions.DefaultBaseUrl)
    {
        _baseUrl = baseUrl.TrimEnd('/');
    }

    /// <summary>Embed URL for a movie, e.g. <c>/embed/movie/tt1300854</c>.</summary>
    public string Movie(string id, EmbedOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var sb = new StringBuilder(_baseUrl).Append("/embed/movie/").Append(Uri.EscapeDataString(id));
        options?.AppendQueryParameters(sb);
        return sb.ToString();
    }

    /// <summary>
    /// Embed URL for a TV show. Omit <paramref name="season"/>/<paramref name="episode"/>
    /// for the player's season/episode picker.
    /// </summary>
    public string Tv(string id, int? season = null, int? episode = null, EmbedOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        if (season is null != episode is null)
            throw new ArgumentException("Season and episode must either both be set or both be omitted.", nameof(episode));

        var sb = new StringBuilder(_baseUrl).Append("/embed/tv/").Append(Uri.EscapeDataString(id));
        if (season is { } s && episode is { } e)
            sb.Append('/').Append(s).Append('/').Append(e);
        options?.AppendQueryParameters(sb);
        return sb.ToString();
    }
}
