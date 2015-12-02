namespace Vurdalakov
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class WebCrawler : WebDownloader, IDisposable
    {
        FileDatabase _fileDatabase;

        public WebCrawler(String baseDirectory = null, Int32 minDelay = 1000, Int32 maxDelay = 3000) : base(baseDirectory, minDelay, maxDelay)
        {
            EnsureDirectoryExists(baseDirectory);

            var fileName = Path.Combine(baseDirectory, "_files.sqlite");
            _fileDatabase = new FileDatabase(fileName);
        }

        public String Extract(String url, String pattern)
        {
            var html = this.DownloadString(url);

            var match = Regex.Match(html, pattern);

            return (2 == match.Groups.Count) ? match.Groups[1].Value : null;
        }

        public String[] ExtractAll(String url, String pattern)
        {
            var html = this.DownloadString(url);

            var matches = Regex.Matches(html, pattern);

            return matches.Cast<Match>().Select(m => m.Groups[1].Value).ToArray();
        }

        public void AddFile(String url, String fileName)
        {
            Tracer.Trace("Adding '{0}'", url);
            var httpHeaders = DownloadHeaders(url);

            var fileRecord = _fileDatabase.GetFile(url);
            if (fileRecord != null)
            {
                Tracer.Trace("Already in database");

                if ((fileRecord.Modified < httpHeaders.LastModified) || (fileRecord.Size != httpHeaders.ContentLength))
                {
                    Tracer.Trace("Is out-of-date");
                    fileRecord.OutOfDate = true;
                }
            }
            else
            {
                fileRecord = new FileDatabaseRecord(url, fileName, httpHeaders.LastModified, httpHeaders.ContentLength, httpHeaders.ContentType);
            }

            _fileDatabase.AddOrReplaceFile(fileRecord);
        }

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _fileDatabase.Dispose();
                    _fileDatabase = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
