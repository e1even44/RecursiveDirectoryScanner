using Microsoft.Extensions.Configuration;

namespace RecursiveDirScanner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // load settings from appsettings.json
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            string connectionString = configuration["Settings:ConnectionString"];
            string startPath = configuration["Settings:StartPath"];

            // check if path exists
            if (!Directory.Exists(startPath))
            {
                Console.WriteLine("The given path does not exist.");
                return;
            }

            // start scan
            Console.WriteLine("Directory Tree:");
            PrintWithColor("[DIR] ", ConsoleColor.Green);
            Console.WriteLine($"{Path.GetFileName(startPath)}");

            int fileCount = 0;
            int directoryCount = 0;
            long totalSize = 0;
            int rootDirectoryId = DatabaseManager.SaveDirectoryToDatabase(connectionString, startPath, null);

            ScanDirectory(connectionString, startPath, rootDirectoryId, ref fileCount, ref directoryCount, ref totalSize, 1); // start recursion with depth=1

            // store scan summary to database
            DatabaseManager.SaveScanSummary(connectionString, startPath, fileCount, directoryCount, totalSize);

            Console.WriteLine("\nScan completed and data stored in the database.");
        }

        static void ScanDirectory(string connectionString, string directoryPath, int parentDirectoryId, ref int fileCount, ref int directoryCount, ref long totalSize, int depth)
        {
            try
            {
                // save current directory
                int currentDirectoryId = DatabaseManager.SaveDirectoryToDatabase(connectionString, directoryPath, parentDirectoryId);
                directoryCount++;

                // list and save files in the current directory
                string[] files = Directory.GetFiles(directoryPath);
                foreach (string file in files)
                {
                    Console.Write(new string(' ', depth * 2));
                    PrintWithColor("[FILE] ", ConsoleColor.Blue);
                    Console.WriteLine($"{Path.GetFileName(file)}");

                    DatabaseManager.SaveFileToDatabase(connectionString, file, currentDirectoryId);

                    FileInfo fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                    fileCount++;
                }

                // list and recursively scan subdirectories
                string[] subdirectories = Directory.GetDirectories(directoryPath);
                foreach (string subdirectory in subdirectories)
                {
                    Console.Write(new string(' ', depth * 2));
                    PrintWithColor("[DIR] ", ConsoleColor.Green);
                    Console.WriteLine($"{Path.GetFileName(subdirectory)}");

                    ScanDirectory(connectionString, subdirectory, currentDirectoryId, ref fileCount, ref directoryCount, ref totalSize, depth + 1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing {directoryPath}: {ex.Message}");
            }
        }

        static void PrintWithColor(string text, ConsoleColor color)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = originalColor;
        }
    }
}