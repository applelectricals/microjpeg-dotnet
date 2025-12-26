# MicroJpeg for .NET

[![MicroJPEG on NuGet](https://img.shields.io/nuget/v/microjpeg.svg?maxAge=2000)](https://www.nuget.org/packages/MicroJpeg)
[![MIT license](https://img.shields.io/github/license/applelectricals/microjpeg-dotnet.svg?maxAge=2592000)](LICENSE)

This is a .NET wrapper around the [MicroJPEG](https://microjpeg.com) image compression and optimization API.

- Supports .NET Core, .NET 6/7/8, and .NET Standard 2.0/2.1
- Non-blocking async throughout
- `Byte[]`, `Stream`, `File`, and `Url` APIs available
- Compression, conversion, AI background removal, and image enhancement

## Installation

Install via NuGet:

```bash
Install-Package MicroJpeg
```

Install via `dotnet`:

```bash
dotnet add package MicroJpeg
```

### Installation via GitHub Packages

If you prefer to use GitHub Packages, add a `nuget.config` file to your project:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="GitHub" value="https://nuget.pkg.github.com/applelectricals/index.json" />
  </packageSources>
</configuration>
```

Then install via `dotnet`:

```bash
dotnet add package MicroJpeg --source https://nuget.pkg.github.com/applelectricals/index.json
```

## Quickstart

```csharp
using MicroJpeg;

using var client = new MicroJpegClient("yourSecretApiKey");
var result = await client.CompressAsync("cat.jpg");

// URL to your compressed version
Console.WriteLine(result.Result.DownloadUrl);

// Compression stats
Console.WriteLine($"Original: {result.Result.OriginalSize} bytes");
Console.WriteLine($"Compressed: {result.Result.CompressedSize} bytes");
Console.WriteLine($"Savings: {result.Result.SavingsPercent}%");
```

## Getting an API Key

1. Sign up at [microjpeg.com](https://microjpeg.com/signup)
2. Subscribe to the Starter plan ($9/month or $49/year)
3. Go to your [API Dashboard](https://microjpeg.com/api-dashboard) to generate an API key

## Compressing Images

```csharp
using var client = new MicroJpegClient("yourSecretApiKey");

// Compress from file path
var result = await client.CompressAsync("path/to/image.jpg");

// Compress from byte array
byte[] imageBytes = File.ReadAllBytes("image.jpg");
var result = await client.CompressAsync(imageBytes, "image.jpg");

// Compress from stream
using var stream = File.OpenRead("image.jpg");
var result = await client.CompressAsync(stream, "image.jpg");

// Compress from URL
var result = await client.CompressAsync(new Uri("https://example.com/image.jpg"));

// Access compression details
Console.WriteLine($"Original size: {result.Result.OriginalSize}");
Console.WriteLine($"Compressed size: {result.Result.CompressedSize}");
Console.WriteLine($"Savings: {result.Result.SavingsPercent}%");
```

### Compression Options

```csharp
var options = new CompressOptions
{
    Quality = 85,                    // 1-100, default: 85
    OutputFormat = "webp",           // jpeg, png, webp, avif, or null for keep-original
    ResizeWidth = 800,               // Optional resize width
    ResizeHeight = 600,              // Optional resize height
    ResizeMode = ResizeMode.Fit      // Fit, Cover, ScaleWidth, ScaleHeight, Thumb
};

var result = await client.CompressAsync("image.jpg", options);
```

## Downloading Compressed Images

```csharp
var result = await client.CompressAsync("cat.jpg");

// Download as byte array
byte[] bytes = await client.DownloadAsync(result.Result.DownloadUrl);

// Download as stream
Stream stream = await client.DownloadAsStreamAsync(result.Result.DownloadUrl);

// Download and save to disk
await client.DownloadToFileAsync(result.Result.DownloadUrl, "compressed-cat.jpg");
```

### Using Extension Methods

```csharp
// Fluent API style
var result = await client.CompressAsync("cat.jpg");

// Get bytes directly from result
byte[] bytes = await result.GetImageByteDataAsync(client);

// Get stream directly from result
Stream stream = await result.GetImageStreamDataAsync(client);

// Save to disk directly from result
await result.SaveImageToDiskAsync(client, "output.jpg");
```

## Converting Formats

Convert images between JPEG, PNG, WebP, and AVIF formats:

```csharp
// Convert to WebP
var result = await client.ConvertAsync("photo.jpg", "webp", quality: 85);

// Convert to AVIF (best compression)
var result = await client.ConvertAsync("photo.png", "avif", quality: 80);

// Download converted image
await client.DownloadToFileAsync(result.Result.DownloadUrl, "photo.webp");
```

## AI Background Removal

Remove backgrounds from images using AI:

```csharp
var options = new BackgroundRemovalOptions
{
    OutputFormat = "png",  // png, webp, avif, jpg
    Quality = 90
};

var result = await client.RemoveBackgroundAsync("product.jpg", options);

// Download the transparent image
await client.DownloadToFileAsync(result.Result.DownloadUrl, "product-nobg.png");

// Check processing time
Console.WriteLine($"Processed in {result.Result.ProcessingTime}ms");
```

## AI Image Enhancement (Upscaling)

Upscale and enhance images using AI:

```csharp
var options = new EnhanceOptions
{
    Scale = 4,              // 2x, 4x, or 8x upscaling
    FaceEnhance = true,     // Enhance faces (requires Starter plan)
    OutputFormat = "png",
    Quality = 95
};

var result = await client.EnhanceAsync("low-res-photo.jpg", options);

// Check new dimensions
Console.WriteLine($"Original: {result.Result.OriginalDimensions.Width}x{result.Result.OriginalDimensions.Height}");
Console.WriteLine($"Enhanced: {result.Result.NewDimensions.Width}x{result.Result.NewDimensions.Height}");

// Download enhanced image
await client.DownloadToFileAsync(result.Result.DownloadUrl, "enhanced-photo.png");
```

## Checking API Usage

```csharp
var usage = await client.GetUsageAsync();

Console.WriteLine($"Tier: {usage.Tier}");
Console.WriteLine($"Compressions this month: {usage.Usage.Compressions}");
Console.WriteLine($"BG Removals: {usage.Usage.BackgroundRemovals}/{usage.Limits.BackgroundRemovalLimit}");
Console.WriteLine($"Enhancements: {usage.Usage.Enhancements}/{usage.Limits.EnhancementLimit}");
```

## Compression Count

Track your API usage by checking the `CompressionCount` property:

```csharp
var result = await client.CompressAsync("cat.jpg");
Console.WriteLine($"Total compressions: {result.CompressionCount}");
// or
Console.WriteLine($"Total compressions: {client.CompressionCount}");
```

## Error Handling

```csharp
try
{
    var result = await client.CompressAsync("image.jpg");
}
catch (MicroJpegApiException ex)
{
    Console.WriteLine($"API Error: {ex.ErrorMessage}");
    Console.WriteLine($"Status Code: {ex.StatusCode}");
    Console.WriteLine($"Error Code: {ex.ErrorCode}");
    
    if (ex.IsLimitReached)
        Console.WriteLine("Monthly limit reached - please upgrade your plan");
    
    if (ex.IsUnauthorized)
        Console.WriteLine("Invalid API key");
    
    if (ex.IsFileTooLarge)
        Console.WriteLine("File exceeds size limit");
    
    if (ex.IsFeatureRestricted)
        Console.WriteLine("This feature requires the Starter plan");
}
```

## Custom HttpClient

You can provide your own `HttpClient` instance for custom configuration:

```csharp
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromMinutes(5)
};

// Note: Don't dispose the client if you're managing HttpClient lifetime externally
var client = new MicroJpegClient("yourApiKey", httpClient);
```

## Supported Formats

### Input Formats
- **Standard**: JPEG, PNG, WebP, AVIF, GIF, BMP, TIFF, SVG
- **RAW Camera**: CR2 (Canon), NEF (Nikon), ARW (Sony), DNG (Adobe), ORF (Olympus), RAF (Fujifilm)

### Output Formats
- **Compression/Conversion**: JPEG, PNG, WebP, AVIF
- **Background Removal**: PNG (with transparency), WebP, AVIF, JPEG
- **Enhancement**: PNG, WebP, AVIF, JPEG

## API Limits

| Feature | Free | Starter ($9/mo) |
|---------|------|-----------------|
| Compress/Convert | 30/month | Unlimited |
| Max File Size | 5MB / 10MB RAW | Unlimited |
| AI Background Removal | 10/month | 300/month |
| AI Enhancement | 10/month | 300/month |
| Max Upscale | 2x | 8x |
| Face Enhancement | No | Yes |
| Output Formats (AI) | PNG only | All |

## License

MIT License - see [LICENSE](LICENSE) for details.

## Links

- [MicroJPEG Website](https://microjpeg.com)
- [API Documentation](https://microjpeg.com/api-docs)
- [Pricing](https://microjpeg.com/pricing)
- [API Dashboard](https://microjpeg.com/api-dashboard)
