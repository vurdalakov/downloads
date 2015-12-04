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

            var fileName = args[0];

            switch (args[1].ToLower())
            {
                case "-info":
                    PrintInfo(fileName);
                    break;
                case "-list":
                    ListFiles(fileName);
                    break;
                case "-update":
                    UpdateFiles(fileName);
                    break;
            }
        }

        static private void PrintInfo(String fileName)
        {
            using (FileDatabase fileDatabase = new FileDatabase(fileName))
            {
                int count = fileDatabase.GetFileCount();
                int available = 0;
                int outOfDate = 0;

                for (var i = 0; i < count; i++)
                {
                    var fileDatabaseRecord = fileDatabase.GetFile(i);

                    if (fileDatabaseRecord.Available)
                    {
                        available++;

                        if (fileDatabaseRecord.OutOfDate)
                        {
                            outOfDate++;
                        }
                    }
                }

                Console.WriteLine("Total files:       {0}", count);
                Console.WriteLine("Available files:   {0}", available);
                Console.WriteLine("Up-to-date files:  {0}", available - outOfDate);
                Console.WriteLine("Out-of-date files: {0}", outOfDate);
            }
        }

        static private void ListFiles(String fileName)
        {
            using (FileDatabase fileDatabase = new FileDatabase(fileName))
            {
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
        }

        static private void UpdateFiles(String fileName)
        {
            using (FileDatabase fileDatabase = new FileDatabase(fileName))
            {
                int count = fileDatabase.GetFileCount();

                using (var webCrawler = new WebCrawler(Path.GetDirectoryName(fileName)))
                {
                    for (var i = 0; i < count; i++)
                    {
                        var fileDatabaseRecord = fileDatabase.GetFile(i);

                        if (!fileDatabaseRecord.Available || fileDatabaseRecord.OutOfDate)
                        {
                            webCrawler.UpdateFile(fileDatabaseRecord.Url);
                            break;
                        }
                    }
                }
            }
        }
    }
}
