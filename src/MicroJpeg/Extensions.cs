using System.IO;
using System.Threading.Tasks;

namespace MicroJpeg
{
    public static class MicroJpegExtensions
    {
        public static async Task<byte[]> GetImageByteDataAsync(this MicroJpegResult<CompressionInfo> result, MicroJpegClient client)
        {
            return await client.DownloadAsync(result.Result.DownloadUrl);
        }

        public static async Task<Stream> GetImageStreamDataAsync(this MicroJpegResult<CompressionInfo> result, MicroJpegClient client)
        {
            return await client.DownloadAsStreamAsync(result.Result.DownloadUrl);
        }

        public static async Task SaveImageToDiskAsync(this MicroJpegResult<CompressionInfo> result, MicroJpegClient client, string outputPath)
        {
            await client.DownloadToFileAsync(result.Result.DownloadUrl, outputPath);
        }

        public static async Task<byte[]> GetImageByteDataAsync(this MicroJpegResult<EnhancementInfo> result, MicroJpegClient client)
        {
            return await client.DownloadAsync(result.Result.DownloadUrl);
        }

        public static async Task<Stream> GetImageStreamDataAsync(this MicroJpegResult<EnhancementInfo> result, MicroJpegClient client)
        {
            return await client.DownloadAsStreamAsync(result.Result.DownloadUrl);
        }

        public static async Task SaveImageToDiskAsync(this MicroJpegResult<EnhancementInfo> result, MicroJpegClient client, string outputPath)
        {
            await client.DownloadToFileAsync(result.Result.DownloadUrl, outputPath);
        }
    }
}
