using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SoraBot.Services.Utils
{
    public class HttpClientHelper : IDisposable
    {
        public HttpClient HttpClient { get; private set; }
        
        public HttpClientHelper()
        {
            this.HttpClient = new HttpClient();
        }

        /// <summary>
        /// Downloads a File and saves it to the given path.
        /// Might throw an exception if there's a failure at any stage
        /// </summary>
        public async Task DownloadAndSaveFile(Uri url, string path)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var resp = await HttpClient.SendAsync(request).ConfigureAwait(false);
            
            await using Stream contentStream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
            await using var stream = new FileStream(path, FileMode.Create, 
                FileAccess.Write, FileShare.None, 3145728, true);
            
            await contentStream.CopyToAsync(stream).ConfigureAwait(false);
            await contentStream.FlushAsync().ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            HttpClient?.Dispose();
        }
    }
}