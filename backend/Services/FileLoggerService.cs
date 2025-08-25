using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CARDB_EF.Services
{
    /// <summary>
    /// Provides a thread-safe service for logging messages to daily text files.
    /// </summary>
    public class FileLoggerService
    {
        private static readonly SemaphoreSlim logSemaphore = new SemaphoreSlim(1, 1);
        private readonly string logDirectoryPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLoggerService"/>, creating the log directory if it doesn't exist.
        /// </summary>
        public FileLoggerService()
        {
            logDirectoryPath = Path.Combine(AppContext.BaseDirectory, "logs");

            if (!Directory.Exists(logDirectoryPath))
                Directory.CreateDirectory(logDirectoryPath);
        }

        /// <summary>
        /// Asynchronously writes a log message to a file named with the current date.
        /// </summary>
        /// <param name="message">The message to write to the log.</param>
        /// <remarks>
        /// This method is thread-safe. If an error occurs during file writing, it will be printed to the console.
        /// Log timestamps are recorded in UTC for consistency.
        /// </remarks>
        public async Task LogAsync(string message)
        {
            var timestamp = DateTime.UtcNow;
            string logFilePath = Path.Combine(this.logDirectoryPath, $"log-{timestamp:yyyy-MM-dd}.txt");
            string logEntry = $"{timestamp:yyyy-MM-dd HH:mm:ss.fffZ}\t{message}";

            try
            {
                await logSemaphore.WaitAsync();
                try
                {
                    await File.AppendAllTextAsync(logFilePath, logEntry + Environment.NewLine);
                }
                finally
                {
                    logSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL: Error writing to log file: {ex.Message}");
            }
        }
    }
}