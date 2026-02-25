public class FileLogger : ILogger
{
    private readonly string _path;

    public FileLogger(string path)
    {
        _path = path;
        File.WriteAllText(_path, $"ПРОТОКОЛ ОБЧИСЛЕНЬ — {DateTime.Now}\n");
    }

    public void Log(string message)
    {
        File.AppendAllText(_path, message + "\n");
    }
}