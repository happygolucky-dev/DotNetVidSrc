using System.Text.Json.Serialization;

namespace VidSrc.Models;

/// <summary>Quality labels reported by VidSrc (1080p, 720p, DVD, SD, CAM, …).</summary>
[JsonConverter(typeof(JsonStringEnumConverter<ContentQuality>))]
public enum ContentQuality
{
    /// <summary>1080p.</summary>
    [JsonStringEnumMemberName("1080p")] Hd1080,
    /// <summary>720p.</summary>
    [JsonStringEnumMemberName("720p")] Hd720,
    /// <summary>DVD rip.</summary>
    [JsonStringEnumMemberName("DVD")] Dvd,
    /// <summary>Standard definition (typical for episodes).</summary>
    [JsonStringEnumMemberName("SD")] Sd,
    /// <summary>Camera recording.</summary>
    [JsonStringEnumMemberName("CAM")] Cam,
    /// <summary>Unrecognized or absent quality label.</summary>
    Unknown,
}

/// <summary>Common fields shared by all <c>/info/*.json</c> responses.</summary>
public abstract record ContentInfo
{
    /// <summary>HTTP-style status mirrored in the payload (200 or 404).</summary>
    [JsonPropertyName("status_code")] public int StatusCode { get; init; }
    /// <summary>"movie" or "tv".</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }
    /// <summary>IMDB id, e.g. <c>tt0944947</c>.</summary>
    [JsonPropertyName("imdb_id")] public string? ImdbId { get; init; }
    /// <summary>TMDB id, e.g. <c>1399</c>. VidSrc returns it as a string; parsed for convenience.</summary>
    [JsonPropertyName("tmdb_id"), JsonConverter(typeof(FlexibleInt32Converter))] public int? TmdbId { get; init; }
    /// <summary>Display title.</summary>
    [JsonPropertyName("title")] public string? Title { get; init; }
    /// <summary>Best available quality.</summary>
    [JsonPropertyName("quality")] public ContentQuality Quality { get; init; }
    /// <summary>Embed URL keyed by IMDB id.</summary>
    [JsonPropertyName("embed_url")] public string? EmbedUrl { get; init; }
    /// <summary>Embed URL keyed by TMDB id.</summary>
    [JsonPropertyName("embed_url_tmdb")] public string? EmbedUrlTmdb { get; init; }
    /// <summary>Raw <c>time_added</c> value (<c>"yyyy-MM-dd HH:mm:ss"</c>, UTC).</summary>
    [JsonPropertyName("time_added")] public string? TimeAdded { get; init; }

    /// <summary><see cref="TimeAdded"/> parsed as UTC, or null when absent/unparseable.</summary>
    [JsonIgnore] public DateTimeOffset? TimeAddedUtc => VidSrcTime.Parse(TimeAdded);

    // Populated on 404 responses: {"status_code":404,"error":"not found","message":"..."}
    /// <summary>Error code on 404 responses ("not found").</summary>
    [JsonPropertyName("error")] public string? Error { get; init; }
    /// <summary>Human-readable error message on 404 responses.</summary>
    [JsonPropertyName("message")] public string? Message { get; init; }

    /// <summary>True when VidSrc has this title (HTTP 200 payload).</summary>
    [JsonIgnore] public bool IsAvailable => StatusCode == 200;
}

/// <summary>Response of <c>GET /info/movie/{id}.json</c>.</summary>
public sealed record MovieInfo : ContentInfo
{
    /// <summary>Release year.</summary>
    [JsonPropertyName("year")] public int? Year { get; init; }
    /// <summary>Poster image URL.</summary>
    [JsonPropertyName("poster")] public string? Poster { get; init; }
    /// <summary>IMDB rating (0–10).</summary>
    [JsonPropertyName("imdb_rating")] public double? ImdbRating { get; init; }
}

/// <summary>One season entry inside <see cref="TvShowInfo"/>.</summary>
public sealed record SeasonInfo
{
    /// <summary>Season number.</summary>
    [JsonPropertyName("season")] public int Season { get; init; }
    /// <summary>Number of available episodes.</summary>
    [JsonPropertyName("episodes")] public int Episodes { get; init; }
    /// <summary>First available episode number.</summary>
    [JsonPropertyName("first_episode")] public int FirstEpisode { get; init; }
    /// <summary>Last available episode number.</summary>
    [JsonPropertyName("last_episode")] public int LastEpisode { get; init; }
    /// <summary>Best available quality for this season.</summary>
    [JsonPropertyName("quality")] public ContentQuality Quality { get; init; }
}

/// <summary>Response of <c>GET /info/tv/{id}.json</c>.</summary>
public sealed record TvShowInfo : ContentInfo
{
    /// <summary>Per-season availability.</summary>
    [JsonPropertyName("seasons")] public IReadOnlyList<SeasonInfo> Seasons { get; init; } = [];
    /// <summary>Total number of available episodes across all seasons.</summary>
    [JsonPropertyName("total_episodes")] public int TotalEpisodes { get; init; }
}

/// <summary>Response of <c>GET /info/tv/{id}/{season}/{episode}.json</c>.</summary>
public sealed record EpisodeInfo : ContentInfo
{
    /// <summary>Title of the parent show.</summary>
    [JsonPropertyName("show_title")] public string? ShowTitle { get; init; }
    /// <summary>Season number.</summary>
    [JsonPropertyName("season")] public int Season { get; init; }
    /// <summary>Episode number.</summary>
    [JsonPropertyName("episode")] public int Episode { get; init; }
}

/// <summary>Paginated envelope of the <c>/*/latest/page-N.json</c> endpoints.</summary>
/// <typeparam name="T">Item type.</typeparam>
public sealed record LatestResult<T>
{
    /// <summary>Items on this page (up to 50).</summary>
    [JsonPropertyName("result")] public IReadOnlyList<T> Result { get; init; } = [];
    /// <summary>Total number of pages.</summary>
    [JsonPropertyName("pages")] public int Pages { get; init; }
}

/// <summary>Common fields of items returned by the <c>latest</c> endpoints.</summary>
public abstract record LatestItem
{
    /// <summary>IMDB id.</summary>
    [JsonPropertyName("imdb_id")] public string? ImdbId { get; init; }
    /// <summary>TMDB id. VidSrc returns it as a string; parsed for convenience.</summary>
    [JsonPropertyName("tmdb_id"), JsonConverter(typeof(FlexibleInt32Converter))] public int? TmdbId { get; init; }
    /// <summary>Best available quality.</summary>
    [JsonPropertyName("quality")] public ContentQuality Quality { get; init; }
    /// <summary>Embed URL keyed by IMDB id.</summary>
    [JsonPropertyName("embed_url")] public string? EmbedUrl { get; init; }
    /// <summary>Embed URL keyed by TMDB id.</summary>
    [JsonPropertyName("embed_url_tmdb")] public string? EmbedUrlTmdb { get; init; }
    /// <summary>Raw <c>time_added</c> value (<c>"yyyy-MM-dd HH:mm:ss"</c>, UTC).</summary>
    [JsonPropertyName("time_added")] public string? TimeAdded { get; init; }

    /// <summary><see cref="TimeAdded"/> parsed as UTC, or null when absent/unparseable.</summary>
    [JsonIgnore] public DateTimeOffset? TimeAddedUtc => VidSrcTime.Parse(TimeAdded);
}

/// <summary>Item of <c>GET /movies/latest/page-N.json</c>.</summary>
public sealed record LatestMovieItem : LatestItem
{
    /// <summary>Movie title.</summary>
    [JsonPropertyName("title")] public string? Title { get; init; }
}

/// <summary>Item of <c>GET /tvshows/latest/page-N.json</c>.</summary>
public sealed record LatestTvShowItem : LatestItem
{
    /// <summary>Show title.</summary>
    [JsonPropertyName("title")] public string? Title { get; init; }
}

/// <summary>Item of <c>GET /episodes/latest/page-N.json</c>.</summary>
public sealed record LatestEpisodeItem : LatestItem
{
    /// <summary>Title of the parent show.</summary>
    [JsonPropertyName("show_title")] public string? ShowTitle { get; init; }
    /// <summary>Season number.</summary>
    [JsonPropertyName("season")] public int Season { get; init; }
    /// <summary>Episode number.</summary>
    [JsonPropertyName("episode")] public int Episode { get; init; }
}
