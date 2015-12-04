namespace Vurdalakov
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    public class WebDownloader
    {
        private String _cacheDirectory;
        private WebClientEx _webClient;

        protected WebDownloader(String baseDirectory = null, Int32 minDelay = 1000, Int32 maxDelay = 3000)
        {
            _cacheDirectory = Path.Combine(baseDirectory ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "_cache");
            EnsureDirectoryExists(_cacheDirectory);

            _minDelay = minDelay;
            _maxDelay = maxDelay;

            _webClient = new WebClientEx();
        }

        public Dictionary<String, String> DownloadRawHeaders(String url)
        {
            Tracer.Trace("Download headers '{0}'", url);

            var rawHeaders = _webClient.DownloadHeaders(url);

            Tracer.Trace(rawHeaders);

            return rawHeaders;
        }

        public WebHeaders DownloadHeaders(String url)
        {
            var rawHeaders = DownloadRawHeaders(url);
            return new WebHeaders(rawHeaders);
        }

        public String DownloadString(String url)
        {
            Tracer.Trace("Download string '{0}'", url);

            var content = ReadFromCache(url);
            if (content != null)
            {
                return content;
            }

            Console.WriteLine("DOWNLOAD");

            Delay();

            try
            {
                content = DownloadHtml(url);
            }
            catch (WebException ex)
            {
                var httpWebResponse = ex.Response as HttpWebResponse;
                if ((httpWebResponse != null) && (httpWebResponse.StatusCode == HttpStatusCode.NotFound))
                {
                    content = "";
                }
                else
                {
                    throw;
                }
            }

            WriteToCache(url, content);

            return content;
        }

        private String DownloadHtml(String url)
        {
            Encoding encoding = Encoding.UTF8;

            var headers =_webClient.DownloadHeaders(url);
            foreach (var header in headers)
            {
                if (header.Key.Equals("content-type", StringComparison.CurrentCultureIgnoreCase))
                {
                    var match = Regex.Match(header.Value, @"charset=([a-zA-Z0-9\-]+)");
                    if (2 == match.Groups.Count)
                    {
                        encoding = Encoding.GetEncoding(match.Groups[1].Value);
                    }
                    break;
                }
            }

            byte[] buffer = _webClient.DownloadData(url);

            byte[] bom = encoding.GetPreamble();

            if ((0 == bom.Length) || (buffer.Length < bom.Length))
            {
                return encoding.GetString(buffer);
            }

            for (int i = 0; i < bom.Length; i++)
            {
                if (buffer[i] != bom[i])
                {
                    return encoding.GetString(buffer);
                }
            }

            return encoding.GetString(buffer, bom.Length, buffer.Length - bom.Length);
        }

        public void DownloadFile(String url, String fileName)
        {
            _webClient.DownloadFile(url, fileName);
        }

        private String UrlToCacheFileName(String url)
        {
            //url = Uri.EscapeDataString(url);
            var fileName = url.Replace("://", "_").Replace('/', '_').Replace('?', '_').Replace(' ', '+');
            fileName = fileName.Trim('_');

            return Path.Combine(_cacheDirectory, fileName);
        }

        private void WriteToCache(String url, String contents)
        {
            File.WriteAllText(UrlToCacheFileName(url), contents);
        }

        private String ReadFromCache(String url)
        {
            try
            {
                return File.ReadAllText(UrlToCacheFileName(url));
            }
            catch
            {
                return null;
            }
        }

        private Random _random = new Random();
        private int _minDelay = -1;
        private int _maxDelay = -1;

        protected void Delay()
        {
            if ((_minDelay < 0) || (_maxDelay < 0) || (_maxDelay < _minDelay))
            {
                return;
            }

            Thread.Sleep(_random.Next(_minDelay, _maxDelay));
        }

        protected void EnsureDirectoryExists(String directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
    }
}
