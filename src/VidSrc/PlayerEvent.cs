using System.Text.Json.Serialization;

namespace VidSrc.Models;

/// <summary>
/// Message the VidSrc player posts to the parent window (<c>window.postMessage</c>)
/// with <c>type: "PLAYER_EVENT"</c>. Useful for hosts that embed the iframe and want
/// to persist playback progress (see <see cref="PlayerEventData.ProgressSeconds"/>).
/// </summary>
public sealed record PlayerEvent
{
    /// <summary>Value of <see cref="Type"/> for player status messages.</summary>
    public const string MessageType = "PLAYER_EVENT";

    /// <summary>Message discriminator; equals <see cref="MessageType"/> for player events.</summary>
    [JsonPropertyName("type")] public string? Type { get; init; }

    /// <summary>Event payload.</summary>
    [JsonPropertyName("data")] public PlayerEventData? Data { get; init; }
}

/// <summary>Payload of a <see cref="PlayerEvent"/>.</summary>
public sealed record PlayerEventData
{
    /// <summary>Identifies the media being played.</summary>
    [JsonPropertyName("player_info")] public PlayerInfo? Info { get; init; }

    /// <summary>One of: playing, paused, completed, seeked.</summary>
    [JsonPropertyName("player_status")] public string? Status { get; init; }

    /// <summary>Current playback position in seconds.</summary>
    [JsonPropertyName("player_progress")] public double ProgressSeconds { get; init; }

    /// <summary>Total duration in seconds.</summary>
    [JsonPropertyName("player_duration")] public double DurationSeconds { get; init; }
}

/// <summary>Identifies the media a <see cref="PlayerEvent"/> refers to.</summary>
public sealed record PlayerInfo
{
    /// <summary>IMDB id, e.g. <c>tt0944947</c>.</summary>
    [JsonPropertyName("imdb")] public string? Imdb { get; init; }

    /// <summary>TMDB id, e.g. <c>1399</c>.</summary>
    [JsonPropertyName("tmdb"), JsonConverter(typeof(FlexibleInt32Converter))] public int? Tmdb { get; init; }

    /// <summary>"movie" or "tv".</summary>
    [JsonPropertyName("mediaType")] public string? MediaType { get; init; }

    /// <summary>Season number (TV only).</summary>
    [JsonPropertyName("season")] public int? Season { get; init; }

    /// <summary>Episode number (TV only).</summary>
    [JsonPropertyName("episode")] public int? Episode { get; init; }
}
