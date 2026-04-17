namespace Lab_2
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath;

        public FileLogger(string filePath)
        {
            _filePath = filePath;
            File.WriteAllText(_filePath, $"--- Протокол запуску: {DateTime.Now} ---\n\n");
        }

        public void Log(string message)
        {
            File.AppendAllText(_filePath, message + "\n");
        }
    }
}
