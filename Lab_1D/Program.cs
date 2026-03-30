using System.Globalization;
using System.Text;

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
            Console.BackgroundColor = ConsoleColor.Magenta;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("┌───────── ГОЛОВНЕ МЕНЮ ──────────┐\n");
            Console.Write("│ 1 - Ввести матрицю вручну       │\n");
            Console.Write("│ 0 - Вихід                       │\n");
            Console.Write("└─────────────────────────────────┘\n");
            Console.ResetColor();
            Console.Write("Відповідь => ");

            int choice = GetValidMenuChoice(0, 1);

            Console.WriteLine();

            if (choice == 0)
            {
                isRunning = false;
                break;
            }

            double[,] matrix = null!;
            string[] xLabels = null!;
            string[] yLabels = null!;

            if (choice == 1)
            {
                logger.Log("\n\n=== РОЗВ'ЯЗАННЯ З ВЛАСНОЮ МАТРИЦЕЮ ===");
                Console.Write("Введіть кількість рядків => ");
                int rows = GetValidMenuChoice(2, 20);
                Console.Write("Введіть кількість стовпців => ");
                int cols = GetValidMenuChoice(2, 20);

                Console.WriteLine("\nЗаповніть матрицю:");
                matrix = FillTableFromConsole(rows, cols);

                xLabels = new string[cols];
                for (int i = 0; i < cols - 1; i++) xLabels[i] = "x" + (i + 1);
                xLabels[cols - 1] = "1";

                yLabels = new string[rows];
                for (int i = 0; i < rows - 1; i++) yLabels[i] = "y" + (i + 1);
                yLabels[rows - 1] = "z";
            }
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Оберіть тип задачі:   ");
            Console.Write("1 - Пошук максимума   \n");
            Console.Write("2 - Пошук мінімума    \n");
            Console.ResetColor();
            Console.Write("Відповідь => ");
            int taskType = GetValidMenuChoice(1, 2);

            double zResult = SolveIntegerProblem(matrix, xLabels, yLabels, logger);

            switch (taskType)
            {
                case 1:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.Write($"Max (Z) = {zResult:F2}");
                    Console.ResetColor();
                    Console.WriteLine("\n");
                    logger.Log($"Max (Z) = {zResult:F2}\n");
                    break;
                case 2:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.DarkGreen;
                    Console.Write($"MIN (Z) = {-zResult:F2}");
                    Console.ResetColor();
                    Console.WriteLine("\n");
                    logger.Log($"Min (Z) = {-zResult:F2}\n");
                    break;
            }
        }
    }

    static double SolveIntegerProblem(double[,] matrix, string[] xLabels, string[] yLabels, ILogger logger)
    {
        int originalVarsCount = matrix.GetLength(1);

        Console.WriteLine("\nПочаткова матриця:");
        logger.Log("Початкова матриця:");
        PrintMatrix(matrix, xLabels, yLabels, logger);

        bool integersFound = false;

        while (!integersFound)
        {
            // 1. Пошук опорного розв'язку
            logger.Log("\n--- Етап 1: Пошук опорного розв'язку ---");
            Console.WriteLine("\n--- Етап 1: Пошук опорного розв'язку ---");

            bool foundBasicSolution = false;
            while (!foundBasicSolution)
            {
                foundBasicSolution = FindBasicSolution(ref matrix, ref xLabels, ref yLabels, logger);
            }

            string basicSolution = GetSolutionPoint(matrix, yLabels, originalVarsCount);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\nОпорний розв'язок знайдено.\nРезультат: X {basicSolution}\n");
            logger.Log($"Опорний розв'язок знайдено. Результат: X {basicSolution}");
            Console.ResetColor();

            // 2. Пошук оптимального розв'язку
            logger.Log("\n--- Етап 2: Пошук оптимального розв'язку ---");
            Console.WriteLine("\n--- Етап 2: Пошук оптимального розв'язку ---");
            Console.ResetColor();

            bool foundOptimalSolution = false;
            while (!foundOptimalSolution)
            {
                foundOptimalSolution = FindOptimalSolution(ref matrix, ref xLabels, ref yLabels, logger);
            }

            string optimalSolution = GetSolutionPoint(matrix, yLabels, originalVarsCount);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\nОптимальний розв'язок знайдено.\nРезультат: X {optimalSolution}\n");
            logger.Log($"Оптимальний розв'язок знайдено. Результат: X {optimalSolution}");
            Console.ResetColor();

            // 3. Перевірка на цілочисельність (Метод Гоморі)
            integersFound = CheckAndApplyGomoryCut(ref matrix, ref xLabels, ref yLabels, logger);
        }

        string finalSolution = GetSolutionPoint(matrix, yLabels, originalVarsCount);
        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.DarkGreen;
        Console.Write($"\nОПТИМАЛЬНИЙ ЦІЛОЧИСЕЛЬНИЙ РОЗВ'ЯЗОК ЗНАЙДЕНО.\nРезультат: X {finalSolution}\n");
        Console.ResetColor();
        logger.Log($"Оптимальний ЦІЛОЧИСЕЛЬНИЙ розв'язок знайдено. Результат: X {finalSolution}");

        double zMax = matrix[matrix.GetLength(0) - 1, matrix.GetLength(1) - 1];
        return zMax;
    }

    // ==========================================================
    // АЛГОРИТМИ СИМПЛЕКС-МЕТОДУ ТА ГОМОРІ
    // ==========================================================

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
                resultCol = FindFirstNegativeInRow(matrix, i, logger);

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
                WriteError("\nНе вдалося знайти опорний розв'язок (система суперечлива). Вихід...\n");
                Environment.Exit(1);
            }

            logger.Log("Знайдено від'ємне значення в останньому стовпці. Потрібна ще одна ітерація.");
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

        if (!noNegativeFound)
        {
            logger.Log("Знайдено від'ємне значення в останньому рядку. Потрібна ще одна ітерація.");
            return false;
        }

        return true;
    }

    static int FindFirstNegativeInRow(double[,] matrix, int rowIndex, ILogger logger)
    {
        int numCols = matrix.GetLength(1);
        bool found = false;
        int targetCol = 0;

        for (int j = 0; j < numCols - 1; j++)
        {
            if (matrix[rowIndex, j] < 0)
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
                matrix[i, j] = CustomRound(matrix[i, j]);
            }
        }

        string temp = xLabels[inCol];
        xLabels[inCol] = yLabels[inRow];
        yLabels[inRow] = temp;

        logger.Log($"\nМатриця після ЖВ (рядок {inRow + 1}, стовпець {inCol + 1}):");
        Console.WriteLine($"\nМатриця після обробки елемента [{inRow + 1}, {inCol + 1}]:");
        PrintMatrix(matrix, xLabels, yLabels, logger);
    }

    // Перевірка на цілочисельність та додавання рядка Гоморі
    static bool CheckAndApplyGomoryCut(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, ILogger logger)
    {
        int numRows = matrix.GetLength(0);
        int numCols = matrix.GetLength(1);

        for (int i = 0; i < numRows; i++)
        {
            if (yLabels[i].StartsWith("x"))
            {
                double lastValue = matrix[i, numCols - 1];

                // Перевірка чи є значення не цілим
                if (Math.Abs(lastValue % 1) > 1e-5)
                {
                    logger.Log("\n--- Етап 3: Відсікання Гоморі ---");
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine("\n--- Етап: Відсікання Гоморі ---");
                    Console.WriteLine("Знайдено дробове значення для змінної. Додаємо нове обмеження.");
                    Console.ResetColor();

                    char lastChar = yLabels[i][yLabels[i].Length - 1];
                    double[] fractionalRow = new double[numCols];

                    for (int j = 0; j < numCols; j++)
                    {
                        fractionalRow[j] = -FractionalPart(matrix[i, j]);
                    }

                    AddRow(ref matrix, ref yLabels, fractionalRow, "s" + lastChar);

                    logger.Log($"Додано рядок Гоморі (s{lastChar}).");
                    PrintMatrix(matrix, xLabels, yLabels, logger);

                    return false; // Знайдено дріб, цілочисельний розв'язок ще не знайдено
                }
            }
        }

        return true; // Всі 'x' є цілими
    }

    static double FractionalPart(double x)
    {
        return x - Math.Floor(x);
    }

    static void AddRow(ref double[,] matrix, ref string[] yLabels, double[] newRow, string newLabel)
    {
        int numRows = matrix.GetLength(0);
        int numCols = matrix.GetLength(1);

        double[,] updatedMatrix = new double[numRows + 1, numCols];

        for (int i = 0; i < numRows - 1; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                updatedMatrix[i, j] = matrix[i, j];
            }
        }

        for (int j = 0; j < numCols; j++)
        {
            updatedMatrix[numRows - 1, j] = newRow[j];
        }

        for (int j = 0; j < numCols; j++)
        {
            updatedMatrix[numRows, j] = matrix[numRows - 1, j];
        }

        string[] updatedYLabels = new string[numRows + 1];

        for (int i = 0; i < numRows - 1; i++)
        {
            updatedYLabels[i] = yLabels[i];
        }

        updatedYLabels[numRows - 1] = newLabel;
        updatedYLabels[numRows] = yLabels[numRows - 1];

        matrix = updatedMatrix;
        yLabels = updatedYLabels;
    }

    static double CustomRound(double value)
    {
        double roundedValue = Math.Round(value, 2);
        int secondDecimal = (int)(Math.Round(value * 100) % 10);

        if (Math.Abs(secondDecimal) > 7)
        {
            roundedValue = Math.Ceiling(value * 10) / 10;
        }

        return roundedValue;
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
                        solutionArray[lastDigit - 1] = matrix[i, numCols - 1].ToString("F2");
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
                Console.Write($"{matrix[i, j],6:F2}\t");
                rowLog += $"{matrix[i, j],6:F2}\t";
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