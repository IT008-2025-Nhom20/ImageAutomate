// CoreImitation.cs
// This file provides a simulated format registry for the Core namespace.
// It allows the WebP extension to reference Core types without modifying the actual Core project.
// This is a workaround to avoid circular dependencies and make the extension self-contained.
//
// IMPORTANT: This file illegally declares namespace `ImageAutomate.Core` as requested by the user.
// When Core implements the actual registry, this file should be removed and the extension
// should reference the real implementation from Core.

using SixLabors.ImageSharp.Formats;

namespace ImageAutomate.Core;

/// <summary>
/// Central registry for image format extensions.
/// Maps format names to their encoders, decoders, and options.
/// </summary>
public interface IImageFormatRegistry
{
    /// <summary>
    /// Register a new image format with its associated encoder, decoder, and options.
    /// </summary>
    void RegisterFormat<TFormat, TEncoder, TDecoder, TOptions>(
        string formatName,
        Func<TOptions, TEncoder> encoderFactory,
        Func<TDecoder> decoderFactory)
        where TFormat : IImageFormat, new()
        where TEncoder : IImageEncoder
        where TOptions : class, new();

    /// <summary>
    /// Get encoder for a format.
    /// </summary>
    IImageEncoder? GetEncoder(string formatName, object? options = null);

    /// <summary>
    /// Get decoder for a format.
    /// </summary>
    object? GetDecoder(string formatName);

    /// <summary>
    /// Get the IImageFormat instance for a format.
    /// </summary>
    IImageFormat? GetFormat(string formatName);

    /// <summary>
    /// Check if a format is registered.
    /// </summary>
    bool IsFormatRegistered(string formatName);
}

/// <summary>
/// Default implementation of format registry (empty for imitation purposes).
/// </summary>
public class ImageFormatRegistry : IImageFormatRegistry
{
    public void RegisterFormat<TFormat, TEncoder, TDecoder, TOptions>(
        string formatName,
        Func<TOptions, TEncoder> encoderFactory,
        Func<TDecoder> decoderFactory)
        where TFormat : IImageFormat, new()
        where TEncoder : IImageEncoder
        where TOptions : class, new()
    {
        // Empty implementation - this is just for type checking
    }

    public IImageEncoder? GetEncoder(string formatName, object? options = null)
    {
        return null;
    }

    public object? GetDecoder(string formatName)
    {
        return null;
    }

    public IImageFormat? GetFormat(string formatName)
    {
        return null;
    }

    public bool IsFormatRegistered(string formatName)
    {
        return false;
    }
}
