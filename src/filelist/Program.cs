namespace Vurdalakov
{
    using System;
    using System.IO;

    // System.Data.SQLite is an ADO.NET provider for SQLite.
    // http://system.data.sqlite.org/

    class Program
    {
        static void Main(string[] args)
        {
            if ((args.Length != 2) || (args[1][0] != '-'))
            {
                Console.WriteLine("filelist file_name -command");
                Console.WriteLine("Commands:");
                Console.WriteLine("\t-info - prints summary info");
                Console.WriteLine("\t-list - lists all files in database");
                return;
            }

            var fileName = Path.GetFullPath(args[0]);

            switch (args[1].ToLower())
            {
                case "-info":
                    PrintInfo(fileName);
                    break;
                case "-list":
                    ListFiles(fileName);
                    break;
                case "-check":
                    CheckFiles(fileName);
                    break;
                case "-update":
                    UpdateFiles(fileName);
                    break;
            }
        }

        static private void PrintInfo(String databaseFileName)
        {
            var fileDatabase = new FileDatabase(databaseFileName);

            int count = fileDatabase.GetFileCount();
            int available = fileDatabase.GetAvailableFileCount();
            int outOfDate = fileDatabase.GetOutOfDateFileCount();

            Console.WriteLine("Total files:         {0}", count);
            Console.WriteLine("Available files:     {0}", available);
            Console.WriteLine("Out-of-date files:   {0}", outOfDate);
            Console.WriteLine("Up-to-date files:    {0} ({1:N1}%)", available - outOfDate, (available - outOfDate) * 100.0 / count);
        }

        static private void ListFiles(String databaseFileName)
        {
            var fileDatabase = new FileDatabase(databaseFileName);

            int count = fileDatabase.GetFileCount();
            Console.WriteLine("{0} files in database:", count);
            Console.WriteLine();

            for (var i = 0; i < count; i++)
            {
                var fileDatabaseRecord = fileDatabase.GetFile(i);

                Console.WriteLine("Url:       {0}", fileDatabaseRecord.Url);
                Console.WriteLine("FileName:  {0}", fileDatabaseRecord.FileName);
                Console.WriteLine("Modified:  {0}", fileDatabaseRecord.Modified);
                Console.WriteLine("Size:      {0}", fileDatabaseRecord.Size);
                Console.WriteLine("Type:      {0}", fileDatabaseRecord.Type);
                Console.WriteLine("Checksum:  {0}", fileDatabaseRecord.Checksum);
                Console.WriteLine("Available: {0}", fileDatabaseRecord.Available);
                Console.WriteLine("OutOfDate: {0}", fileDatabaseRecord.OutOfDate);
                Console.WriteLine();
            }
        }

        static private void CheckFiles(String databaseFileName)
        {
            var fileDatabase = new FileDatabase(databaseFileName);

            int count = fileDatabase.GetFileCount();

            var baseDirectory = Path.GetDirectoryName(databaseFileName);
            var webCrawler = new WebCrawler(baseDirectory);

            for (var i = 0; i < count; i++)
            {
                var fileDatabaseRecord = fileDatabase.GetFile(i);

                if (fileDatabaseRecord.Size < 0)
                {
                    var webHeaders = webCrawler.DownloadHeaders(fileDatabaseRecord.Url);
                    fileDatabaseRecord.Modify(webHeaders.LastModified, webHeaders.ContentLength, webHeaders.ContentType, "", false, false);
                }

                var fileName = Path.Combine(baseDirectory, fileDatabaseRecord.FileName);
                var fileInfo = new FileInfo(fileName);

                if (fileInfo.Exists)
                {
                    fileDatabaseRecord.Available = true;
                    fileDatabaseRecord.OutOfDate = (fileInfo.Length != fileDatabaseRecord.Size) ||
                        (fileInfo.LastWriteTime != fileDatabaseRecord.Modified);
                }

                fileDatabase.AddOrReplaceFile(fileDatabaseRecord);
            }
        }

        static private void UpdateFiles(String databaseFileName)
        {
            var fileDatabase = new FileDatabase(databaseFileName);

            var webCrawler = new WebCrawler(Path.GetDirectoryName(databaseFileName));

            while (true)
            {
                var fileDatabaseRecord = fileDatabase.GetNextNotAvailableOrOutOfDateFile();
                if (null == fileDatabaseRecord)
                {
                    return;
                }

                webCrawler.UpdateFile(fileDatabaseRecord);
            }
        }
    }
}
