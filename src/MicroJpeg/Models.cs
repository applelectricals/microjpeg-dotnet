using System;
using Newtonsoft.Json;

namespace MicroJpeg
{
    public class CompressOptions
    {
        public int? Quality { get; set; }
        public string OutputFormat { get; set; }
        public int? ResizeWidth { get; set; }
        public int? ResizeHeight { get; set; }
        public ResizeMode? ResizeMode { get; set; }
    }

    public enum ResizeMode
    {
        Fit,
        Cover,
        ScaleWidth,
        ScaleHeight,
        Thumb
    }

    public class BackgroundRemovalOptions
    {
        public string OutputFormat { get; set; }
        public int? Quality { get; set; }
    }

    public class EnhanceOptions
    {
        public int Scale { get; set; } = 2; // 2, 4, 8
        public bool FaceEnhance { get; set; }
        public string OutputFormat { get; set; }
        public int? Quality { get; set; }
    }

    public class MicroJpegResult<T>
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("result")]
        public T Result { get; set; }

        [JsonProperty("compressionCount")]
        public int CompressionCount { get; set; }
    }

    public class CompressionInfo
    {
        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonProperty("originalSize")]
        public long OriginalSize { get; set; }

        [JsonProperty("compressedSize")]
        public long CompressedSize { get; set; }

        [JsonProperty("savingsPercent")]
        public double SavingsPercent { get; set; }

        [JsonProperty("processingTime")]
        public int ProcessingTime { get; set; }
    }

    public class EnhancementInfo
    {
        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonProperty("originalDimensions")]
        public Dimensions OriginalDimensions { get; set; }

        [JsonProperty("newDimensions")]
        public Dimensions NewDimensions { get; set; }

        [JsonProperty("processingTime")]
        public int ProcessingTime { get; set; }
    }

    public class Dimensions
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class UsageInfo
    {
        [JsonProperty("tier")]
        public string Tier { get; set; }

        [JsonProperty("usage")]
        public UsageStats Usage { get; set; }

        [JsonProperty("limits")]
        public UsageLimits Limits { get; set; }
    }

    public class UsageStats
    {
        [JsonProperty("compressions")]
        public int Compressions { get; set; }

        [JsonProperty("backgroundRemovals")]
        public int BackgroundRemovals { get; set; }

        [JsonProperty("enhancements")]
        public int Enhancements { get; set; }
    }

    public class UsageLimits
    {
        [JsonProperty("compressionLimit")]
        public int CompressionLimit { get; set; }

        [JsonProperty("backgroundRemovalLimit")]
        public int BackgroundRemovalLimit { get; set; }

        [JsonProperty("enhancementLimit")]
        public int EnhancementLimit { get; set; }
    }
}
