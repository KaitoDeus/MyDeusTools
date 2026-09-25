using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.Services
{
    public class TextUtilityService : ITextUtilityService
    {
        // 1. JSON Operations
        public string FormatJson(string json, int indentSpaces = 2)
        {
            if (string.IsNullOrWhiteSpace(json)) return string.Empty;

            using var doc = JsonDocument.Parse(json);
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            return JsonSerializer.Serialize(doc.RootElement, options);
        }

        public string MinifyJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return string.Empty;

            using var doc = JsonDocument.Parse(json);
            var options = new JsonSerializerOptions
            {
                WriteIndented = false
            };
            return JsonSerializer.Serialize(doc.RootElement, options);
        }

        public (bool IsValid, string? ErrorMessage) ValidateJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return (false, "Chuỗi JSON trống.");

            try
            {
                using var doc = JsonDocument.Parse(json);
                return (true, null);
            }
            catch (JsonException ex)
            {
                return (false, $"Lỗi cú pháp tại dòng {ex.LineNumber}, vị trí {ex.BytePositionInLine}: {ex.Message}");
            }
        }

        // 2. Base64 Operations
        public string TextToBase64(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }

        public string Base64ToText(string base64)
        {
            if (string.IsNullOrWhiteSpace(base64)) return string.Empty;
            byte[] bytes = Convert.FromBase64String(base64.Trim());
            return Encoding.UTF8.GetString(bytes);
        }

        public string FileToBase64(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Không tìm thấy tập tin.", filePath);

            byte[] bytes = File.ReadAllBytes(filePath);
            return Convert.ToBase64String(bytes);
        }

        public void Base64ToFile(string base64, string destinationFilePath)
        {
            if (string.IsNullOrWhiteSpace(base64))
                throw new ArgumentException("Chuỗi Base64 không hợp lệ.", nameof(base64));

            byte[] bytes = Convert.FromBase64String(base64.Trim());
            string? dir = Path.GetDirectoryName(destinationFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllBytes(destinationFilePath, bytes);
        }

        // 3. URL & HTML Encoding
        public string UrlEncode(string text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : Uri.EscapeDataString(text);
        }

        public string UrlDecode(string text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : Uri.UnescapeDataString(text.Replace("+", " "));
        }

        public string HtmlEncode(string text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : WebUtility.HtmlEncode(text);
        }

        public string HtmlDecode(string text)
        {
            return string.IsNullOrEmpty(text) ? string.Empty : WebUtility.HtmlDecode(text);
        }

        // 4. Hash Generators
        public string ComputeMd5(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(text));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public string ComputeSha1(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            byte[] hash = SHA1.HashData(Encoding.UTF8.GetBytes(text));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public string ComputeSha256(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(text));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public string ComputeSha512(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            byte[] hash = SHA512.HashData(Encoding.UTF8.GetBytes(text));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public string ComputeFileHash(string filePath, string algorithm = "SHA256")
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Không tìm thấy tập tin.", filePath);

            using var stream = File.OpenRead(filePath);
            byte[] hash = algorithm.ToUpperInvariant() switch
            {
                "MD5" => MD5.HashData(stream),
                "SHA1" => SHA1.HashData(stream),
                "SHA512" => SHA512.HashData(stream),
                _ => SHA256.HashData(stream)
            };
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        // 5. Case Converters & Text Analysis
        public string ToCamelCase(string text)
        {
            var words = ExtractWords(text);
            if (words.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            sb.Append(words[0].ToLowerInvariant());
            for (int i = 1; i < words.Count; i++)
            {
                string w = words[i];
                if (w.Length > 0)
                {
                    sb.Append(char.ToUpperInvariant(w[0])).Append(w.Substring(1).ToLowerInvariant());
                }
            }
            return sb.ToString();
        }

        public string ToPascalCase(string text)
        {
            var words = ExtractWords(text);
            if (words.Count == 0) return string.Empty;

            var sb = new StringBuilder();
            foreach (var w in words)
            {
                if (w.Length > 0)
                {
                    sb.Append(char.ToUpperInvariant(w[0])).Append(w.Substring(1).ToLowerInvariant());
                }
            }
            return sb.ToString();
        }

        public string ToSnakeCase(string text)
        {
            var words = ExtractWords(text);
            return string.Join("_", words.Select(w => w.ToLowerInvariant()));
        }

        public string ToKebabCase(string text)
        {
            var words = ExtractWords(text);
            return string.Join("-", words.Select(w => w.ToLowerInvariant()));
        }

        public string ToUpperCase(string text)
        {
            return text.ToUpperInvariant();
        }

        public string ToLowerCase(string text)
        {
            return text.ToLowerInvariant();
        }

        public string ToTitleCase(string text)
        {
            var words = ExtractWords(text);
            return string.Join(" ", words.Select(w => w.Length > 0 ? char.ToUpperInvariant(w[0]) + w.Substring(1).ToLowerInvariant() : string.Empty));
        }

        public TextStatistics AnalyzeText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new TextStatistics(0, 0, 0, 0, 0);
            }

            int chars = text.Length;
            int charsNoSpaces = text.Count(c => !char.IsWhiteSpace(c));
            int words = text.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
            int lines = text.Split('\n').Length;
            int byteCount = Encoding.UTF8.GetByteCount(text);

            return new TextStatistics(chars, charsNoSpaces, words, lines, byteCount);
        }

        private static List<string> ExtractWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return new List<string>();

            var matches = Regex.Matches(text, @"[A-Z]?[a-z0-9]+|[A-Z0-9]+(?=[A-Z][a-z0-9]|\b|[^a-zA-Z0-9]|$)");
            var words = new List<string>();
            foreach (Match m in matches)
            {
                if (!string.IsNullOrWhiteSpace(m.Value))
                {
                    words.Add(m.Value);
                }
            }

            // Fallback if regex did not capture any (e.g. non-latin letters)
            if (words.Count == 0)
            {
                words = text.Split(new[] { ' ', '_', '-', '.', '/', '\\', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            return words;
        }
    }
}
