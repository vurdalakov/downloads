namespace Vurdalakov
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class WebCrawler : WebDownloader
    {
        FileDatabase _fileDatabase;
        String _baseDirectory;
        String _tempDirectory;

        public WebCrawler(String baseDirectory = null, Int32 minDelay = 1000, Int32 maxDelay = 3000) : base(baseDirectory, minDelay, maxDelay)
        {
            _baseDirectory = baseDirectory;
            EnsureDirectoryExists(_baseDirectory);

            _tempDirectory = Path.Combine(_baseDirectory, "_temp");
            EnsureDirectoryExists(_tempDirectory);

            var fileName = Path.Combine(_baseDirectory, "_files.sqlite");
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

            var fileRecord = new FileDatabaseRecord(url, fileName);

            _fileDatabase.AddOrReplaceFile(fileRecord);
        }

        public void UpdateFile(String url) // TODO: remove this overload?
        {
            var fileRecord = _fileDatabase.GetFile(url);
            if (null == fileRecord)
            {
                throw new Exception("Not in database");
            }

            UpdateFile(fileRecord);
        }

        public void UpdateFile(FileDatabaseRecord fileRecord)
        {
            Tracer.Trace("Updating '{0}'", fileRecord.Url);

            var tempFileName = Path.Combine(_tempDirectory, fileRecord.FileName.Replace('\\', '_'));

            var webHeaders = DownloadFile(fileRecord.Url, tempFileName);

            if (webHeaders.ContentLength != new FileInfo(tempFileName).Length)
            {
                throw new Exception("Wrong file size");
            }

            var fileName = Path.Combine(_baseDirectory, fileRecord.FileName);
            EnsureDirectoryExists(Path.GetDirectoryName(fileName));

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            File.Move(tempFileName, fileName);
            File.SetLastWriteTimeUtc(fileName, webHeaders.LastModified);

            fileRecord.Modify(webHeaders.LastModified, webHeaders.ContentLength, webHeaders.ContentType, "", true, false);
            _fileDatabase.AddOrReplaceFile(fileRecord);
        }
    }
}
