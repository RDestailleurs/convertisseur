using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConvertisseurApp
{
    public class ResultEntry
    {
        public string Extractor { get; set; } = "";
        public string Site { get; set; } = "";
        public string Title { get; set; } = "";
        public string Uploader { get; set; } = "";
        public string Id { get; set; } = "";
        public string Url { get; set; } = "";
        public int Score { get; set; }
        public override string ToString() => $"{Extractor} | {Title} | {Uploader}";
    }

    public static class NameSearch
    {
        public enum ExtractorType { All, Video, Audio }

        public static List<string> GetExtractors(ExtractorType type)
        {
            string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extractors.json");
            if (!File.Exists(jsonPath))
                return new List<string>();

            var json = File.ReadAllText(jsonPath);
            using var doc = JsonDocument.Parse(json);
            var extractors = new List<string>();
            if (type == ExtractorType.All)
            {
                extractors.AddRange(doc.RootElement.GetProperty("video").EnumerateArray().Select(e => e.GetString()).Where(s => !string.IsNullOrEmpty(s))!);
                extractors.AddRange(doc.RootElement.GetProperty("audio").EnumerateArray().Select(e => e.GetString()).Where(s => !string.IsNullOrEmpty(s))!);
            }
            else if (type == ExtractorType.Video)
            {
                extractors.AddRange(doc.RootElement.GetProperty("video").EnumerateArray().Select(e => e.GetString()).Where(s => !string.IsNullOrEmpty(s))!);
            }
            else if (type == ExtractorType.Audio)
            {
                extractors.AddRange(doc.RootElement.GetProperty("audio").EnumerateArray().Select(e => e.GetString()).Where(s => !string.IsNullOrEmpty(s))!);
            }
            return extractors;
        }

        public static async Task<List<ResultEntry>> SearchByNameAsync(string query, ExtractorType type)
        {
            string ytDlpExe = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp.exe");
            string safeQuery = query.Contains('\"') ? query : $"\"{query}\"";
            string args = $"ytsearch5:{safeQuery} --print '%(extractor)s' --get-title --print '%(uploader)s' --get-id --no-warnings";
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = ytDlpExe;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();
            var results = new List<ResultEntry>();
            if (!string.IsNullOrWhiteSpace(error))
            {
                results.Add(new ResultEntry { Extractor = "yt-dlp", Site = "yt-dlp", Title = $"[ERREUR yt-dlp] {error}", Score = 0 });
            }
            if (!string.IsNullOrWhiteSpace(output))
            {
                var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i + 3 < lines.Length; )
                {
                    string extractor = lines[i];
                    string title = lines[i + 1];
                    string uploader = lines[i + 2];
                    string id = lines[i + 3];
                    string url = BuildPageUrl(extractor, id);
                    if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(url))
                    {
                        string site = extractor;
                        int score = FuzzyScore(title, query);
                        results.Add(new ResultEntry {
                            Extractor = extractor,
                            Site = site,
                            Title = title,
                            Uploader = uploader,
                            Id = id,
                            Url = url,
                            Score = score
                        });
                        i += 4;
                    }
                    else
                    {
                        i++;
                    }
                }
            }
            var sorted = results.OrderByDescending(r => r.Score).ThenBy(r => r.Title).ToList();
            return sorted;
        }

        // Construit l'URL de la page vidéo à partir de l'extractor et de l'id
        private static string BuildPageUrl(string extractor, string id)
        {
            if (string.IsNullOrWhiteSpace(extractor) || string.IsNullOrWhiteSpace(id))
                return null;
            switch (extractor.ToLower())
            {
                case "youtube":
                    return $"https://www.youtube.com/watch?v={id}";
                case "soundcloud":
                    return $"https://soundcloud.com/{id}";
                case "dailymotion":
                    return $"https://www.dailymotion.com/video/{id}";
                // Ajouter d'autres extracteurs si besoin
                default:
                    return null;
            }
        }

        // Déduit le site à partir de l'URL (domaine principal)
        private static string GetSiteFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                string host = uri.Host;
                if (host.StartsWith("www.")) host = host.Substring(4);
                int dot = host.IndexOf('.');
                if (dot > 0) host = host.Substring(0, dot);
                return host;
            }
            catch { return "?"; }
        }

        // Simple fuzzy score: nombre de caractères du query trouvés dans l'ordre dans le titre
        private static int FuzzyScore(string text, string pattern)
        {
            text = text.ToLowerInvariant();
            pattern = pattern.ToLowerInvariant();
            int score = 0, ti = 0;
            foreach (char pc in pattern)
            {
                while (ti < text.Length && text[ti] != pc) ti++;
                if (ti < text.Length)
                {
                    score++;
                    ti++;
                }
            }
            return score;
        }
    }
}