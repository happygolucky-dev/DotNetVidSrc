# DotNetVidSrc

A .NET 10 API wrapper for [VidSrc](https://vidsrc.sh/vidsrc/docs/) — no API key required.

```
dotnet add package DotNetVidSrc
```

*(Hosted on GitHub Packages — see "Installing" below for the one-time feed setup.)*

Covers the full public surface:

- **Embed URL builder** for movies and TV episodes (all documented query params)
- **Content info** endpoints (`/info/*.json`)
- **Latest content** endpoints (`/movies|tvshows|episodes/latest/page-N.json`), including auto-paginating streams
- **ID lists** (`/ids/*.txt`)
- **Player event model** for hosts embedding the iframe (`postMessage` `PLAYER_EVENT` payloads)

## Installing

The package is published to GitHub Packages, which requires a one-time authenticated
feed setup (any GitHub account + a PAT with the `read:packages` scope):

```bash
dotnet nuget add source https://nuget.pkg.github.com/happygolucky-dev/index.json \
  --name happygolucky --username YOUR_GITHUB_USERNAME --password YOUR_PAT

dotnet add package DotNetVidSrc
```

No GitHub account? Download the `.nupkg` from the
[latest release](https://github.com/happygolucky-dev/DotNetVidSrc/releases/latest)
and add its folder as a local feed.

## Usage

```csharp
using VidSrc;
using VidSrc.Models;

using var client = new VidSrcClient();

// ── Embed URLs ──
var movieUrl = client.Embed.Movie("tt1300854");
// https://vidsrc.sh/embed/movie/tt1300854

var episodeUrl = client.Embed.Tv("tt0944947", season: 1, episode: 1, new EmbedOptions
{
    AutoNext = true,
    StartAtSeconds = 300,
    SubtitleLanguages = ["en", "fr"],   // up to 3 ISO codes
});
// https://vidsrc.sh/embed/tv/tt0944947/1/1?autonext=1&startAt=300&ds_lang=en,fr

var pickerUrl = client.Embed.Tv("1399"); // season/episode picker

// ── Content info ──
MovieInfo? movie = await client.GetMovieInfoAsync("tt1300854");
if (movie is { IsAvailable: true })
    Console.WriteLine($"{movie.Title} ({movie.Year}) — {movie.Quality}");

TvShowInfo? show = await client.GetTvShowInfoAsync("1399");
EpisodeInfo? ep = await client.GetEpisodeInfoAsync("1399", 1, 1);

// Unavailable titles come back with StatusCode 404 and a Message instead of throwing:
MovieInfo? missing = await client.GetMovieInfoAsync("tt9999999999");
Console.WriteLine(missing?.Message); // "This title is not available."

// ── Latest content (50 items/page) ──
LatestResult<LatestMovieItem>? page1 = await client.GetLatestMoviesAsync(page: 1);

// Auto-paginating streams:
await foreach (LatestEpisodeItem item in client.GetAllLatestEpisodesAsync())
    Console.WriteLine($"{item.ShowTitle} S{item.Season}E{item.Episode}");

// ── ID lists (updated daily) ──
IReadOnlyList<string> movieIds = await client.GetMovieImdbIdsAsync();   // tt1234567, one per line
IReadOnlyList<string> epIds = await client.GetEpisodeTmdbIdsAsync();    // 1399_1x1
```

All `id` arguments accept either an IMDB id (`tt0944947`) or a numeric TMDB id (`1399`).

### Custom domains / DI

```csharp
// Point at your own Cloudflare-fronted domain:
var options = new VidSrcClientOptions { BaseUrl = "https://video.example.com" };
using var client = new VidSrcClient(options);

// Or bring your own HttpClient (e.g. from IHttpClientFactory):
var client = new VidSrcClient(httpClient);
```

### Resume playback (player events)

The embedded player posts `PLAYER_EVENT` messages to the parent window. `VidSrc.Models.PlayerEvent`
matches the payload shape, so Blazor/MAUI hosts can deserialize it, persist
`Data.ProgressSeconds`, and later restore with `EmbedOptions.StartAtSeconds`.

## Error handling

- Info endpoints return a model even on 404 (check `IsAvailable` / `StatusCode`).
- Other non-success statuses throw `VidSrcApiException` (carries the HTTP `StatusCode`).

## Notes

- VidSrc caches info/latest responses for ~5 minutes; ID lists update daily.
- Direct autoplay (`autoplay=1`) only works on custom domains.
- Custom subtitle files (`CustomSubtitleUrl`) must be served over HTTPS with CORS headers.
