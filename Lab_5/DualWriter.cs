using System.Text;

namespace Lab_5
{
    public class DualWriter : TextWriter
    {
        private TextWriter _originalConsoleStream;
        private StreamWriter _fileStream;

        public DualWriter(TextWriter consoleStream, string filePath)
        {
            _originalConsoleStream = consoleStream;
            // Відкриваємо файл в режимі дозапису (Append = true)
            _fileStream = new StreamWriter(filePath, true, Encoding.UTF8) { AutoFlush = true };
        }

        public override Encoding Encoding => _originalConsoleStream.Encoding;

        public override void Write(char value)
        {
            _originalConsoleStream.Write(value);
            _fileStream.Write(value);
        }

        public override void Write(string? value)
        {
            _originalConsoleStream.Write(value);
            _fileStream.Write(value);
        }

        public override void WriteLine(string? value)
        {
            _originalConsoleStream.WriteLine(value);
            _fileStream.WriteLine(value);
        }

        public override void WriteLine()
        {
            _originalConsoleStream.WriteLine();
            _fileStream.WriteLine();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileStream?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
