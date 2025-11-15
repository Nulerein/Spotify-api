using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpotifyTopTracks
{
    class Program
    {
        // Token obtained from Spotify Developer Dashboard for testing purposes
        private const string token = "TOKEN_ID";

        static async Task<int> Main(string[] args)
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.spotify.com/")
            };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var topTracks = await GetTopTracksAsync(client);
                if (topTracks.Count == 0)
                {
                    Console.WriteLine("Нет треков или пустой ответ.");
                    return 0;
                }

                foreach (var t in topTracks)
                {
                    var artists = string.Join(", ", t.Artists.ConvertAll(a => a.Name));
                    Console.WriteLine($"{t.Name} by {artists}");
                }

                return 0;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"HTTP error: {e.Message}");
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
                return 2;
            }
        }

        private static async Task<List<Track>> GetTopTracksAsync(HttpClient client)
        {
            var resp = await client.GetAsync("v1/me/top/tracks?time_range=long_term&limit=5");
            if (!resp.IsSuccessStatusCode)
            {
                var content = await resp.Content.ReadAsStringAsync();
                Console.WriteLine($"Spotify API returned {(int)resp.StatusCode} {resp.ReasonPhrase}: {content}");
                return new List<Track>();
            }

            var json = await resp.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var root = JsonSerializer.Deserialize<TopTracksResponse>(json, options);
            return root?.Items ?? new List<Track>();
        }
    }

    public class TopTracksResponse
    {
        public List<Track> Items { get; set; }
    }

    public class Track
    {
        public string Name { get; set; }
        public List<Artist> Artists { get; set; }
    }

    public class Artist
    {
        public string Name { get; set; }
    }
}
