namespace Vurdalakov
{
    using System;
    using System.Data.SQLite;
    using System.IO;

    // System.Data.SQLite is an ADO.NET provider for SQLite.
    // http://system.data.sqlite.org/

    public class FileDatabase
    {
        private String _connectionString;

        public FileDatabase(String fileName)
        {
            _connectionString = "Data Source=" + fileName + ";Pooling=true;FailIfMissing=false";

            if (!System.IO.File.Exists(fileName))
            {
                SQLiteConnection.CreateFile(fileName);

                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SQLiteCommand("CREATE TABLE files (url TEXT PRIMARY KEY, filename TEXT, modified TEXT, size INTEGER, type TEXT, checksum TEXT, available INTEGER, outofdate INTEGER)", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public Int32 GetFileCount()
        {
            return GetFileCount("");
        }

        public Int32 GetAvailableFileCount()
        {
            return GetFileCount("WHERE available=1");
        }

        public Int32 GetOutOfDateFileCount()
        {
            return GetFileCount("WHERE outofdate=1");
        }

        private Int32 GetFileCount(String whereClause)
        {
            var commandText = String.Format("SELECT COUNT(*) FROM files " + whereClause);

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(commandText, connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public void AddOrReplaceFile(FileDatabaseRecord fileDatabaseRecord)
        {
            var commandText = String.Format("INSERT OR REPLACE INTO files (url, filename, modified, size, type, checksum, available, outofdate) VALUES ('{0}', '{1}', '{2:O}', {3}, '{4}', '{5}', {6}, {7})",
                fileDatabaseRecord.Url, fileDatabaseRecord.FileName, fileDatabaseRecord.Modified, fileDatabaseRecord.Size, fileDatabaseRecord.Type,
                fileDatabaseRecord.Checksum, fileDatabaseRecord.Available ? 1 : 0, fileDatabaseRecord.OutOfDate ? 1 : 0);

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(commandText, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public FileDatabaseRecord GetFile(String url)
        {
            return GetFileEx(String.Format("WHERE url='{0}'", url), "");
        }

        public FileDatabaseRecord GetFile(Int32 index)
        {
            return GetFileEx("", String.Format("OFFSET {0}", index));
        }

        public FileDatabaseRecord GetNextNotAvailableOrOutOfDateFile()
        {
            return GetFileEx("WHERE available=0 OR outofdate=1", "");
        }

        private FileDatabaseRecord GetFileEx(String whereClause, String offsetClause)
        {
            var commandText = "SELECT url, filename, modified, size, type, checksum, available, outofdate FROM files " + whereClause + " ORDER BY filename ASC LIMIT 1 " + offsetClause;

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                using (var command = new SQLiteCommand(commandText, connection))
                {
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        return new FileDatabaseRecord(reader.GetString(0), reader.GetString(1), DateTime.ParseExact(reader.GetString(2), "O", null), reader.GetInt64(3), reader.GetString(4),
                            reader.GetString(5), 1 == reader.GetInt64(6), 1 == reader.GetInt64(7));
                    }
                }
            }

            return null;
        }
    }

    public class FileDatabaseRecord
    {
        public String Url { get; private set; }
        public String FileName { get; private set; }
        public DateTime Modified { get; private set; }
        public Int64 Size { get; private set; }
        public String Type { get; private set; }
        public String Checksum { get; private set; }
        public Boolean Available { get; set; }
        public Boolean OutOfDate { get; set; }

        public FileDatabaseRecord(String url, String fileName)
            : this(url, fileName, new DateTime(1900,1,1,0,0,0), -1, "", "", false, false)
        {
        }

        public FileDatabaseRecord(String url, String fileName, DateTime modified, Int64 size, String type, String checksum, Boolean available, Boolean outOfDate)
        {
            Url = url;
            FileName = fileName;

            Modify(modified, size, type, checksum, available, outOfDate);
        }

        public void Modify(DateTime modified, Int64 size, String type, String checksum, Boolean available, Boolean outOfDate)
        {
            Modified = modified;
            Size = size;
            Type = type;
            Checksum = checksum;
            Available = available;
            OutOfDate = outOfDate;
        }
    }
}
