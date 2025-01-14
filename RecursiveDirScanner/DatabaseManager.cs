using System.Data.SQLite;

namespace RecursiveDirScanner
{
    public static class DatabaseManager
    {
        public static int SaveDirectoryToDatabase(string connectionString, string path, int? parentDirectoryId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        INSERT OR IGNORE INTO Directories (Name, Path, ParentDirectoryID, DateScanned)
                        VALUES (@Name, @Path, @ParentDirectoryID, @DateScanned);
                        
                        SELECT DirectoryID FROM Directories WHERE Path = @Path;
                    ";
                    command.Parameters.AddWithValue("@Name", Path.GetFileName(path));
                    command.Parameters.AddWithValue("@Path", path);
                    command.Parameters.AddWithValue("@ParentDirectoryID", parentDirectoryId);
                    command.Parameters.AddWithValue("@DateScanned", DateTime.Now);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public static void SaveFileToDatabase(string connectionString, string path, int directoryId)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    FileInfo fileInfo = new FileInfo(path);

                    command.CommandText = @"
                        INSERT OR IGNORE INTO Files (Name, Path, Size, DirectoryID, DateScanned)
                        VALUES (@Name, @Path, @Size, @DirectoryID, @DateScanned);
                    ";
                    command.Parameters.AddWithValue("@Name", fileInfo.Name);
                    command.Parameters.AddWithValue("@Path", path);
                    command.Parameters.AddWithValue("@Size", fileInfo.Length);
                    command.Parameters.AddWithValue("@DirectoryID", directoryId);
                    command.Parameters.AddWithValue("@DateScanned", DateTime.Now);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void SaveScanSummary(string connectionString, string startDirectory, int fileCount, int directoryCount, long totalSize)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(connection))
                {
                    command.CommandText = @"
                        INSERT INTO ScanHistory (StartDirectory, DateStarted, FileCount, DirectoryCount, TotalSize)
                        VALUES (@StartDirectory, @DateStarted, @FileCount, @DirectoryCount, @TotalSize);
                    ";
                    command.Parameters.AddWithValue("@StartDirectory", startDirectory);
                    command.Parameters.AddWithValue("@DateStarted", DateTime.Now);
                    command.Parameters.AddWithValue("@FileCount", fileCount);
                    command.Parameters.AddWithValue("@DirectoryCount", directoryCount);
                    command.Parameters.AddWithValue("@TotalSize", totalSize);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
