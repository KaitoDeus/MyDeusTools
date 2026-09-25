using System.Threading.Tasks;

namespace MyDeusTools.App.Services.Impl
{
    public record TextStatistics(int Characters, int CharactersNoSpaces, int Words, int Lines, int ByteCount);

    public interface ITextUtilityService
    {
        // JSON Operations
        string FormatJson(string json, int indentSpaces = 2);
        string MinifyJson(string json);
        (bool IsValid, string? ErrorMessage) ValidateJson(string json);

        // Base64 Operations
        string TextToBase64(string text);
        string Base64ToText(string base64);
        string FileToBase64(string filePath);
        void Base64ToFile(string base64, string destinationFilePath);

        // URL & HTML Encoding
        string UrlEncode(string text);
        string UrlDecode(string text);
        string HtmlEncode(string text);
        string HtmlDecode(string text);

        // Hash Generators
        string ComputeMd5(string text);
        string ComputeSha1(string text);
        string ComputeSha256(string text);
        string ComputeSha512(string text);
        string ComputeFileHash(string filePath, string algorithm = "SHA256");

        // Case Converters & Text Analysis
        string ToCamelCase(string text);
        string ToPascalCase(string text);
        string ToSnakeCase(string text);
        string ToKebabCase(string text);
        string ToUpperCase(string text);
        string ToLowerCase(string text);
        string ToTitleCase(string text);
        TextStatistics AnalyzeText(string text);
    }
}
