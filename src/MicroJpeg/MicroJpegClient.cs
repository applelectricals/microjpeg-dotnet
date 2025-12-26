using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MicroJpeg
{
    public class MicroJpegClient : IDisposable
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;
        private readonly bool _disposeHttpClient;
        private int _compressionCount;

        public int CompressionCount => _compressionCount;

        public MicroJpegClient(string apiKey) : this(apiKey, new HttpClient(), true)
        {
        }

        public MicroJpegClient(string apiKey, HttpClient httpClient, bool disposeHttpClient = false)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _disposeHttpClient = disposeHttpClient;

            if (_httpClient.BaseAddress == null)
            {
                _httpClient.BaseAddress = new Uri("https://api.microjpeg.com/v1/");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", 
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"api:{_apiKey}")));
        }

        public async Task<MicroJpegResult<CompressionInfo>> CompressAsync(string filePath, CompressOptions options = null)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return await CompressAsync(stream, Path.GetFileName(filePath), options);
            }
        }

        public async Task<MicroJpegResult<CompressionInfo>> CompressAsync(byte[] data, string fileName, CompressOptions options = null)
        {
            using (var stream = new MemoryStream(data))
            {
                return await CompressAsync(stream, fileName, options);
            }
        }

        public async Task<MicroJpegResult<CompressionInfo>> CompressAsync(Stream stream, string fileName, CompressOptions options = null)
        {
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
            content.Add(streamContent, "file", fileName);

            if (options != null)
            {
                if (options.Quality.HasValue) content.Add(new StringContent(options.Quality.Value.ToString()), "quality");
                if (!string.IsNullOrEmpty(options.OutputFormat)) content.Add(new StringContent(options.OutputFormat), "format");
                if (options.ResizeWidth.HasValue) content.Add(new StringContent(options.ResizeWidth.Value.ToString()), "width");
                if (options.ResizeHeight.HasValue) content.Add(new StringContent(options.ResizeHeight.Value.ToString()), "height");
                if (options.ResizeMode.HasValue) content.Add(new StringContent(options.ResizeMode.Value.ToString().ToLower()), "mode");
            }

            return await PostAsync<CompressionInfo>("compress", content);
        }

        public async Task<MicroJpegResult<CompressionInfo>> CompressAsync(Uri url, CompressOptions options = null)
        {
            var request = new
            {
                url = url.ToString(),
                quality = options?.Quality,
                format = options?.OutputFormat,
                width = options?.ResizeWidth,
                height = options?.ResizeHeight,
                mode = options?.ResizeMode?.ToString().ToLower()
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
            return await PostAsync<CompressionInfo>("compress", content);
        }

        public async Task<MicroJpegResult<CompressionInfo>> ConvertAsync(string filePath, string format, int? quality = null)
        {
            return await CompressAsync(filePath, new CompressOptions { OutputFormat = format, Quality = quality });
        }

        public async Task<MicroJpegResult<CompressionInfo>> RemoveBackgroundAsync(string filePath, BackgroundRemovalOptions options = null)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return await RemoveBackgroundAsync(stream, Path.GetFileName(filePath), options);
            }
        }

        public async Task<MicroJpegResult<CompressionInfo>> RemoveBackgroundAsync(Stream stream, string fileName, BackgroundRemovalOptions options = null)
        {
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
            content.Add(streamContent, "file", fileName);

            if (options != null)
            {
                if (options.Quality.HasValue) content.Add(new StringContent(options.Quality.Value.ToString()), "quality");
                if (!string.IsNullOrEmpty(options.OutputFormat)) content.Add(new StringContent(options.OutputFormat), "format");
            }

            return await PostAsync<CompressionInfo>("remove-background", content);
        }

        public async Task<MicroJpegResult<EnhancementInfo>> EnhanceAsync(string filePath, EnhanceOptions options = null)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return await EnhanceAsync(stream, Path.GetFileName(filePath), options);
            }
        }

        public async Task<MicroJpegResult<EnhancementInfo>> EnhanceAsync(Stream stream, string fileName, EnhanceOptions options = null)
        {
            var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
            content.Add(streamContent, "file", fileName);

            if (options != null)
            {
                content.Add(new StringContent(options.Scale.ToString()), "scale");
                content.Add(new StringContent(options.FaceEnhance.ToString().ToLower()), "face_enhance");
                if (options.Quality.HasValue) content.Add(new StringContent(options.Quality.Value.ToString()), "quality");
                if (!string.IsNullOrEmpty(options.OutputFormat)) content.Add(new StringContent(options.OutputFormat), "format");
            }

            return await PostAsync<EnhancementInfo>("enhance", content);
        }

        public async Task<UsageInfo> GetUsageAsync()
        {
            var response = await _httpClient.GetAsync("usage");
            await EnsureSuccess(response);
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UsageInfo>(json);
        }

        public async Task<byte[]> DownloadAsync(string url)
        {
            return await _httpClient.GetByteArrayAsync(url);
        }

        public async Task<Stream> DownloadAsStreamAsync(string url)
        {
            return await _httpClient.GetStreamAsync(url);
        }

        public async Task DownloadToFileAsync(string url, string outputPath)
        {
            using (var stream = await DownloadAsStreamAsync(url))
            using (var fileStream = File.Create(outputPath))
            {
                await stream.CopyToAsync(fileStream);
            }
        }

        private async Task<MicroJpegResult<T>> PostAsync<T>(string endpoint, HttpContent content)
        {
            var response = await _httpClient.PostAsync(endpoint, content);
            await EnsureSuccess(response);
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<MicroJpegResult<T>>(json);
            _compressionCount = result.CompressionCount;
            return result;
        }

        private async Task EnsureSuccess(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    var error = JsonConvert.DeserializeAnonymousType(content, new { error = "", message = "" });
                    throw new MicroJpegApiException((int)response.StatusCode, error?.error ?? "unknown_error", error?.message ?? "An error occurred");
                }
                catch (JsonException)
                {
                    throw new MicroJpegApiException((int)response.StatusCode, "unknown_error", content);
                }
            }
        }

        private string GetMimeType(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            switch (ext)
            {
                case ".jpg":
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".webp": return "image/webp";
                case ".avif": return "image/avif";
                case ".gif": return "image/gif";
                case ".bmp": return "image/bmp";
                case ".tiff": return "image/tiff";
                case ".svg": return "image/svg+xml";
                default: return "application/octet-stream";
            }
        }

        public void Dispose()
        {
            if (_disposeHttpClient)
            {
                _httpClient.Dispose();
            }
        }
    }
}
