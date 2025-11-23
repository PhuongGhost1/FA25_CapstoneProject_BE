using System.Text.RegularExpressions;
using CusomMapOSM_Application.Interfaces.Services.Firebase;
using Microsoft.Extensions.Logging;

namespace CusomMapOSM_Infrastructure.Services;

/// <summary>
/// Service to process HTML content and upload base64 images to Firebase Storage,
/// replacing base64 data URLs with Firebase Storage URLs to reduce database size.
/// </summary>
public class HtmlContentImageProcessor
{
    private readonly IFirebaseStorageService _storageService;
    private readonly ILogger<HtmlContentImageProcessor> _logger;
    private const int MaxBase64SizeBytes = 5 * 1024 * 1024; // 5MB limit per image

    // Regex to match base64 data URLs: data:image/[type];base64,[data]
    // This will match both standalone data URLs and those in img src attributes
    private static readonly Regex Base64ImageRegex = new(
        @"data:image/(?<type>[a-zA-Z0-9+/]+);base64,(?<data>[A-Za-z0-9+/=]{100,})",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public HtmlContentImageProcessor(
        IFirebaseStorageService storageService,
        ILogger<HtmlContentImageProcessor> logger)
    {
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// Processes HTML content, extracts base64 images, uploads them to Firebase Storage,
    /// and replaces base64 data URLs with Firebase Storage URLs.
    /// </summary>
    /// <param name="htmlContent">HTML content that may contain base64 images</param>
    /// <param name="folder">Folder path in Firebase Storage (e.g., "poi-tooltips", "poi-popups")</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Processed HTML content with base64 images replaced by Firebase Storage URLs</returns>
    public async Task<string> ProcessHtmlContentAsync(
        string? htmlContent,
        string folder = "poi-content",
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return htmlContent ?? string.Empty;
        }

        var matches = Base64ImageRegex.Matches(htmlContent);
        if (matches.Count == 0)
        {
            // No base64 images found, return as-is
            return htmlContent;
        }

        _logger.LogInformation("Found {Count} base64 images in HTML content, processing...", matches.Count);

        var processedContent = htmlContent;
        var replacements = new List<(string original, string replacement)>();

        foreach (Match match in matches)
        {
            try
            {
                var imageType = match.Groups["type"].Value;
                var base64Data = match.Groups["data"].Value;

                // Decode base64 to get actual size
                var imageBytes = Convert.FromBase64String(base64Data);
                var imageSizeBytes = imageBytes.Length;

                // Skip if image is too large
                if (imageSizeBytes > MaxBase64SizeBytes)
                {
                    _logger.LogWarning(
                        "Skipping base64 image: size {Size} bytes exceeds limit {Limit} bytes",
                        imageSizeBytes, MaxBase64SizeBytes);
                    continue;
                }

                // Determine file extension from MIME type
                var extension = imageType.ToLower() switch
                {
                    "jpeg" or "jpg" => "jpg",
                    "png" => "png",
                    "gif" => "gif",
                    "webp" => "webp",
                    "svg+xml" => "svg",
                    _ => "jpg" // Default to jpg
                };

                var fileName = $"image_{Guid.NewGuid():N}.{extension}";

                // Upload to Firebase Storage
                using var imageStream = new MemoryStream(imageBytes);
                var imageUrl = await _storageService.UploadFileAsync(fileName, imageStream, folder);

                // Replace base64 data URL with Firebase Storage URL
                var originalDataUrl = match.Value;
                replacements.Add((originalDataUrl, imageUrl));

                _logger.LogInformation(
                    "Uploaded base64 image ({Size} bytes) to Firebase Storage: {Url}",
                    imageSizeBytes, imageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing base64 image, skipping...");
                // Continue with next image
            }
        }

        // Apply all replacements
        foreach (var (original, replacement) in replacements)
        {
            processedContent = processedContent.Replace(original, replacement);
        }

        if (replacements.Count > 0)
        {
            _logger.LogInformation(
                "Processed {Count} base64 images, replaced with Firebase Storage URLs",
                replacements.Count);
        }

        return processedContent;
    }

    /// <summary>
    /// Validates HTML content size to prevent database packet size issues.
    /// </summary>
    /// <param name="htmlContent">HTML content to validate</param>
    /// <param name="maxSizeBytes">Maximum allowed size in bytes (default: 1MB)</param>
    /// <returns>True if content is within size limit</returns>
    public bool ValidateContentSize(string? htmlContent, int maxSizeBytes = 1024 * 1024)
    {
        if (string.IsNullOrEmpty(htmlContent))
        {
            return true;
        }

        var sizeBytes = System.Text.Encoding.UTF8.GetByteCount(htmlContent);
        return sizeBytes <= maxSizeBytes;
    }
}
