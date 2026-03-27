using System.Globalization;
using System.Text;

namespace Lab_3_Rewritten
{
    class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            ILogger logger = new FileLogger("protocol.txt");

            bool isRunning = true;

            while (isRunning)
            {
                Console.WriteLine();
                Console.WriteLine("======= ГОЛОВНЕ МЕНЮ =======");
                Console.WriteLine("1 - Ввести матрицю вручну");
                Console.WriteLine("0 - Вихід");
                Console.WriteLine("============================");
                Console.Write("=> ");

                int choice = GetValidMenuChoice(0, 1);

                Console.WriteLine();

                if (choice == 0)
                {
                    isRunning = false;
                    break;
                }
                Console.WriteLine("Оберіть тип задачі:");
                Console.WriteLine("1 - Пошук максимума");
                Console.WriteLine("2 - Пошук мінімума");
                Console.Write("=> ");

                choice = GetValidMenuChoice(1, 2);
                Console.WriteLine();

                logger.Log("\n\n=== РОЗВ'ЯЗАННЯ З ВЛАСНОЮ МАТРИЦЕЮ ===");
                Console.Write("Введіть кількість рядків => ");
                int rows = GetValidMenuChoice(2, 20);
                Console.Write("Введіть кількість стовпців => ");
                int cols = GetValidMenuChoice(2, 20);

                Console.WriteLine("\n Заповніть матрицю:");
                double[,] userMatrix = FillTableFromConsole(rows, cols);

                string[] userXLabels = new string[cols];
                for (int i = 0; i < cols - 1; i++) userXLabels[i] = "x" + (i + 1);
                userXLabels[cols - 1] = "1";

                string[] userYLabels = new string[rows];
                for (int i = 0; i < rows - 1; i++) userYLabels[i] = "y" + (i + 1);
                userYLabels[rows - 1] = "z";

                for (int i = 0; i < rows - 1; i++)
                {
                    Console.Write($"Чи зробити мітку рядка {i + 1} нульовою? (1 - Так, 0 - Ні): ");
                    if (GetValidMenuChoice(0, 1) == 1)
                    {
                        userYLabels[i] = "0";
                    }
                }

                double zMax = SolveProblem(userMatrix, userXLabels, userYLabels, logger);

                switch (choice)
                {

                    case 1:
                        Console.WriteLine($"Max (Z) = {zMax:F3}\n");
                        logger.Log($"Max (Z) = {zMax:F3}\n");
                        Console.ResetColor();
                        break;

                    case 2:
                        Console.WriteLine($"MIN (Z) = {-zMax:F3}\n");
                        logger.Log($"Min (Z) = {-zMax:F3}\n");
                        Console.ResetColor();
                        break;

                }
            }
        }

        static double SolveProblem(double[,] matrix, string[] xLabels, string[] yLabels, ILogger logger)
        {
            int originalSize = matrix.GetLength(1);

            Console.WriteLine("\nПочаткова матриця:");
            logger.Log("Початкова матриця:");
            PrintMatrix(matrix, xLabels, yLabels, logger);

            // 1. Позбавляємося нульових міток
            bool hasZeroLabel = yLabels.Contains("0");
            if (hasZeroLabel)
            {
                logger.Log("\n--- Етап 1: Видалення нульових міток ---");
                Console.WriteLine("\n--- Етап 1: Видалення нульових міток ---");
                bool foundZeroLabel = false;
                while (!foundZeroLabel)
                {
                    foundZeroLabel = ProcessZeroLabelRow(ref matrix, ref xLabels, ref yLabels, logger);
                    if (!foundZeroLabel)
                        DeleteZeroCols(ref matrix, ref xLabels, logger);
                }
            }

            // 2. Пошук опорного розв'язку
            logger.Log("\n--- Етап 2: Пошук опорного розв'язку ---");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("\n--- Етап 2: Пошук опорного розв'язку ---");
            Console.ResetColor();

            bool foundBasicSolution = false;
            while (!foundBasicSolution)
            {
                foundBasicSolution = FindBasicSolution(ref matrix, ref xLabels, ref yLabels, logger);
            }

            string basicSolution = GetSolutionPoint(matrix, yLabels, originalSize);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\nОпорний розв'язок знайдено.\nРезультат: X {basicSolution}\n");
            logger.Log($"Опорний розв'язок знайдено. Результат: X {basicSolution}");
            Console.ResetColor();

            // 3. Пошук оптимального розв'язку
            logger.Log("\n--- Етап 3: Пошук оптимального розв'язку ---");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Шукаємо оптимальний розв'язок!");
            Console.ResetColor();

            bool foundOptimalSolution = false;
            while (!foundOptimalSolution)
            {
                foundOptimalSolution = FindOptimalSolution(ref matrix, ref xLabels, ref yLabels, logger);
            }

            string optimalSolution = GetSolutionPoint(matrix, yLabels, originalSize);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\nОптимальний розв'язок знайдено.\nРезультат: X {optimalSolution}\n");
            logger.Log($"Оптимальний розв'язок знайдено. Результат: X {optimalSolution}");

            double zMax = matrix[matrix.GetLength(0) - 1, matrix.GetLength(1) - 1];
            return zMax;
        }

        // ==========================================================
        // АЛГОРИТМИ СИМПЛЕКС-МЕТОДУ
        // ==========================================================

        static bool ProcessZeroLabelRow(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, ILogger logger)
        {
            int numRows = matrix.GetLength(0);
            bool noZeroFound = true;
            int resultCol = 0;

            for (int i = 0; i < numRows - 1; i++)
            {
                if (yLabels[i] == "0")
                {
                    noZeroFound = false;
                    resultCol = FindFirstNegativeOrPositive(matrix, i, "find_zero", logger);

                    if (resultCol != -1)
                    {
                        FindSmallestPositiveAndPivot(ref matrix, ref xLabels, ref yLabels, resultCol, i, "find_zero", logger);
                    }
                    break;
                }
            }

            if (!noZeroFound)
            {
                if (resultCol == -1)
                {
                    WriteError("\nРозв'язок не знайдено (система суперечлива). Вихід...\n");
                    Environment.Exit(1);
                }
                return false;
            }
            return true;
        }

        static bool FindBasicSolution(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, ILogger logger)
        {
            int numRows = matrix.GetLength(0);
            int lastColIndex = matrix.GetLength(1) - 1;
            bool noNegativeFound = true;
            int resultCol = 0;

            for (int i = 0; i < numRows - 1; i++)
            {
                if (matrix[i, lastColIndex] < 0)
                {
                    noNegativeFound = false;
                    resultCol = FindFirstNegativeOrPositive(matrix, i, "basic", logger);

                    if (resultCol != -1)
                    {
                        FindSmallestPositiveAndPivot(ref matrix, ref xLabels, ref yLabels, resultCol, i, "basic", logger);
                    }
                    break;
                }
            }

            if (!noNegativeFound)
            {
                if (resultCol == -1)
                {
                    WriteError("\nНе вдалося знайти опорний розв'язок. Вихід...\n");
                    Environment.Exit(1);
                }
                return false;
            }
            return true;
        }

        static bool FindOptimalSolution(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, ILogger logger)
        {
            int lastRowIndex = matrix.GetLength(0) - 1;
            int numCols = matrix.GetLength(1);
            bool noNegativeFound = true;

            for (int j = 0; j < numCols - 1; j++)
            {
                if (matrix[lastRowIndex, j] < 0)
                {
                    noNegativeFound = false;
                    FindSmallestPositiveAndPivot(ref matrix, ref xLabels, ref yLabels, j, -1, "optimal", logger);
                    break;
                }
            }

            return noNegativeFound;
        }

        static int FindFirstNegativeOrPositive(double[,] matrix, int rowIndex, string solutionType, ILogger logger)
        {
            int numCols = matrix.GetLength(1);
            bool found = false;
            int targetCol = 0;

            for (int j = 0; j < numCols - 1; j++)
            {
                if (solutionType == "basic" && matrix[rowIndex, j] < 0)
                {
                    targetCol = j;
                    found = true;
                    break;
                }
                else if (solutionType == "find_zero" && matrix[rowIndex, j] > 0)
                {
                    targetCol = j;
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                WriteError("\nСистема обмежень ЗЛП суперечлива.\n");
                logger.Log("Система обмежень ЗЛП суперечлива.");
                return -1;
            }
            return targetCol;
        }

        static void FindSmallestPositiveAndPivot(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, int colIndex, int rowIndexLastCol, string solutionType, ILogger logger)
        {
            int numRows = matrix.GetLength(0);
            int numCols = matrix.GetLength(1);
            int smallestInRow = 0;
            double minValue = double.MaxValue;
            bool allNegativeLastCol = true;

            for (int i = 0; i < numRows - 1; i++)
            {
                if (matrix[i, colIndex] == 0) continue;
                if (matrix[i, numCols - 1] == 0 && matrix[i, colIndex] < 0) continue;

                double dividedValue = matrix[i, numCols - 1] / matrix[i, colIndex];

                if (dividedValue == minValue && i == rowIndexLastCol && solutionType == "basic")
                {
                    smallestInRow = i;
                    minValue = dividedValue;
                    allNegativeLastCol = false;
                }

                if (dividedValue >= 0 && dividedValue < minValue)
                {
                    smallestInRow = i;
                    minValue = dividedValue;
                    allNegativeLastCol = false;
                }
            }

            if (allNegativeLastCol && solutionType == "optimal")
            {
                WriteError("\nЦільова функція Z необмежена зверху.\n");
                logger.Log("Цільова функція Z необмежена зверху.");
                Environment.Exit(1);
            }

            PerformModifiedJordanElimination(ref matrix, ref xLabels, ref yLabels, smallestInRow, colIndex, logger);
        }

        static void PerformModifiedJordanElimination(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, int inRow, int inCol, ILogger logger)
        {
            int numRows = matrix.GetLength(0);
            int numCols = matrix.GetLength(1);
            double mainElement = matrix[inRow, inCol];

            if (Math.Abs(mainElement) < 1e-10)
            {
                WriteError("Неможливо виконати ЖВ: головний елемент дорівнює 0.");
                return;
            }

            matrix[inRow, inCol] = 1.0;

            for (int i = 0; i < numRows; i++)
            {
                if (i != inRow)
                {
                    for (int j = 0; j < numCols; j++)
                    {
                        if (j != inCol)
                        {
                            matrix[i, j] = matrix[i, j] * mainElement - matrix[i, inCol] * matrix[inRow, j];
                        }
                    }
                }
            }

            for (int row = 0; row < numRows; row++)
            {
                if (row != inRow) matrix[row, inCol] = -matrix[row, inCol];
            }

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    matrix[i, j] /= mainElement;
                    if (Math.Abs(matrix[i, j]) < 1e-10) matrix[i, j] = 0;
                }
            }

            string temp = xLabels[inCol];
            xLabels[inCol] = yLabels[inRow];
            yLabels[inRow] = temp;

            logger.Log($"\nМатриця після ЖВ (рядок {inRow + 1}, стовпець {inCol + 1}):");
            Console.WriteLine($"\nМатриця після обробки елемента [{inRow + 1}, {inCol + 1}]:");
            PrintMatrix(matrix, xLabels, yLabels, logger);
        }

        static void DeleteZeroCols(ref double[,] matrix, ref string[] xLabels, ILogger logger)
        {
            int numRows = matrix.GetLength(0);
            int numCols = matrix.GetLength(1);

            int colToRemove = Array.IndexOf(xLabels, "0");
            if (colToRemove == -1) return;

            double[,] newMatrix = new double[numRows, numCols - 1];
            string[] newXLabels = new string[numCols - 1];

            for (int i = 0; i < numRows; i++)
            {
                int newCol = 0;
                for (int j = 0; j < numCols; j++)
                {
                    if (j == colToRemove) continue;
                    newMatrix[i, newCol] = matrix[i, j];
                    newCol++;
                }
            }

            int newLabelIndex = 0;
            for (int j = 0; j < numCols; j++)
            {
                if (j == colToRemove) continue;
                newXLabels[newLabelIndex] = xLabels[j];
                newLabelIndex++;
            }

            matrix = newMatrix;
            xLabels = newXLabels;

            logger.Log("Матриця після видалення стовпця '0':");
            Console.WriteLine("\nМатриця після видалення стовпця '0':");
            PrintMatrix(matrix, xLabels, null, logger); // yLabels не змінюються
        }

        static string GetSolutionPoint(double[,] matrix, string[] yLabels, int originalVarsCount)
        {
            int numRows = matrix.GetLength(0);
            int numCols = matrix.GetLength(1);
            string[] solutionArray = new string[originalVarsCount - 1];

            for (int i = 0; i < numRows - 1; i++)
            {
                if (yLabels[i].StartsWith("x"))
                {
                    char lastChar = yLabels[i][yLabels[i].Length - 1];
                    if (int.TryParse(lastChar.ToString(), out int lastDigit))
                    {
                        if (lastDigit - 1 < solutionArray.Length)
                            solutionArray[lastDigit - 1] = matrix[i, numCols - 1].ToString("F3");
                    }
                }
            }

            string solution = "(";
            foreach (string el in solutionArray)
            {
                solution += (el ?? "0") + "; ";
            }
            return solution.TrimEnd(' ', ';') + ")";
        }

        // ==========================================================
        // ДОПОМІЖНІ МЕТОДИ ДЛЯ ІНТЕРФЕЙСУ ТА ЛОГУВАННЯ
        // ==========================================================

        static void PrintMatrix(double[,] matrix, string[] xLabels, string[] yLabels, ILogger logger)
        {
            int numRows = matrix.GetLength(0);
            int numCols = matrix.GetLength(1);

            Console.Write("\t");
            string headerLog = "\t";

            for (int i = 0; i < numCols; i++)
            {
                string label = (i != numCols - 1) ? $"-{xLabels[i]}" : xLabels[i];
                Console.Write(label + "\t");
                headerLog += label + "\t";
            }
            Console.WriteLine();
            logger.Log(headerLog);

            for (int i = 0; i < numRows; i++)
            {
                string rowLog = "";
                if (yLabels != null && i < yLabels.Length)
                {
                    Console.Write(yLabels[i] + "\t");
                    rowLog += yLabels[i] + "\t";
                }
                else
                {
                    Console.Write("\t");
                    rowLog += "\t";
                }

                for (int j = 0; j < numCols; j++)
                {
                    Console.Write($"{matrix[i, j],6:F3}\t");
                    rowLog += $"{matrix[i, j],6:F3}\t";
                }
                Console.WriteLine();
                logger.Log(rowLog);
            }
            logger.Log("");
        }

        static double[,] FillTableFromConsole(int rows, int cols)
        {
            double[,] matrix = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix[i, j] = GetDoubleFromKey();
                    Console.Write("\t");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            return matrix;
        }

        static int GetValidMenuChoice(int min, int max)
        {
            while (true)
            {
                string input = Console.ReadLine()!;

                if (int.TryParse(input, out int result) && result >= min && result <= max)
                    return result;
                WriteError($"Введіть число від {min} до {max} => ");
            }
        }

        static void WriteError(string ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Помилка: ");
            Console.ResetColor();
            Console.Write(ex);
        }

        static double GetDoubleFromKey()
        {
            string input = "";
            char decimalSeparator = Convert.ToChar(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                char keyChar = keyInfo.KeyChar;

                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    if (double.TryParse(input, out double result))
                    {
                        return result;
                    }
                }
                else if (keyInfo.Key == ConsoleKey.Backspace && input.Length > 0)
                {
                    input = input.Substring(0, input.Length - 1);
                    Console.Write("\b \b");
                }
                else if (keyChar == '-' && input.Length == 0)
                {
                    input += keyChar;
                    Console.Write(keyChar);
                }
                else if (char.IsDigit(keyChar))
                {
                    input += keyChar;
                    Console.Write(keyChar);
                }
                else if (keyChar == '.' || keyChar == ',')
                {
                    if (!input.Contains(decimalSeparator.ToString()) && input != "-")
                    {
                        if (input.Length == 0)
                        {
                            input += "0";
                            Console.Write("0");
                        }
                        input += decimalSeparator;
                        Console.Write(decimalSeparator);
                    }
                }
            }
        }
    }

    // ==========================================================
    // ІНТЕРФЕЙСИ ТА КЛАСИ ДЛЯ ЛОГУВАННЯ
    // ==========================================================
    public interface ILogger
    {
        void Log(string message);
    }

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