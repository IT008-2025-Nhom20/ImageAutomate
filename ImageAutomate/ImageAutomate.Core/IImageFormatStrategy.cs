using SixLabors.ImageSharp.Formats;

namespace ImageAutomate.Core;

/// <summary>
/// Strategy interface for image format handling.
/// Encapsulates format-specific behavior for encoding, extensions, and UI.
/// </summary>
public interface IImageFormatStrategy
{
    /// <summary>
    /// Gets the format name (e.g., "JPEG", "PNG").
    /// </summary>
    string FormatName { get; }

    /// <summary>
    /// Gets the primary file extension (e.g., ".jpg").
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Gets the MIME type for this format.
    /// </summary>
    string MimeType { get; }

    /// <summary>
    /// Creates an encoder with the specified options.
    /// </summary>
    /// <param name="options">Format-specific options (can be null for defaults).</param>
    /// <param name="skipMetadata">Whether to skip metadata during encoding.</param>
    /// <returns>An ImageSharp encoder.</returns>
    IImageEncoder CreateEncoder(object? options = null, bool skipMetadata = false);

    /// <summary>
    /// Gets a UI-friendly summary of the format options.
    /// </summary>
    /// <param name="options">Format-specific options.</param>
    /// <returns>Human-readable options summary.</returns>
    string GetOptionsSummary(object? options);

    /// <summary>
    /// Validates that options are of the correct type for this format.
    /// </summary>
    /// <param name="options">Options to validate.</param>
    /// <returns>True if options type is correct.</returns>
    bool IsValidOptionsType(object? options);
}
