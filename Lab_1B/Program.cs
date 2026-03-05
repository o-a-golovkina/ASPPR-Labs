using Lab_1A;
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
            Console.WriteLine(UIStrings.MainMenu);
            Console.Write(UIStrings.MenuAnswer);
            int choice = GetValidMenuChoice(0, 4);

            switch (choice)
            {
                case 0:
                    isRunning = false;
                    break;

                case 1:
                    logger.Log("\n\n=== Знаходження оберненої матриці ===\n\nВхідна матриця:");
                    Console.Write(UIStrings.M1Size);
                    int size = GetValidMenuChoice(2, 100);

                    Console.WriteLine($"\n--- Заповнення матриці {size}x{size} ---");
                    double[,] myMatrix = FillMatrixFromConsole(size, size);

                    LogMatrix(myMatrix, logger);
                    logger.Log("Протокол обчислення:\n");

                    Console.WriteLine("\n");
                    double[,] newMatrix = InverseMatrix(myMatrix, logger);

                    if (newMatrix != null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"\n--- Обернена матриця ---");
                        logger.Log("\nОбернена матриця:");
                        LogMatrix(newMatrix, logger);
                        PrintMatrix(newMatrix, "invers");
                        Console.ResetColor();
                    }
                    Console.WriteLine("\n");
                    break;

                case 2:
                    logger.Log("\n\n=== Знаходження рангу матриці ===\n\nВхідна матриця:");
                    Console.Write(UIStrings.M2SizeRows);
                    int rows = GetValidMenuChoice(2, 100);
                    Console.Write(UIStrings.M2SizeCols);
                    int cols = GetValidMenuChoice(2, 100);

                    Console.WriteLine($"\n--- Заповнення матриці {rows}x{cols} ---");
                    myMatrix = FillMatrixFromConsole(rows, cols);

                    LogMatrix(myMatrix, logger);
                    logger.Log("Протокол обчислення:\n");

                    Console.WriteLine("\n");

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    int rank = GetMatrixRank(myMatrix, logger);
                    Console.WriteLine($"\nРанг матриці: " + rank);
                    logger.Log($"РАНГ МАТРИЦІ: " + rank);
                    Console.ResetColor();
                    Console.WriteLine();
                    break;

                case 3:
                    logger.Log("\n\n=== Знаходження розв'язків СЛАР за допомогою оберненої матриці ===\n\nВхідна матриця A:");
                    Console.Write(UIStrings.M3Size);
                    int n = GetValidMenuChoice(2, 100);

                    Console.WriteLine($"\n--- Заповнення матриці А {n}x{n} ---");
                    myMatrix = FillMatrixFromConsole(n, n);

                    LogMatrix(myMatrix, logger);

                    Console.WriteLine("\n");

                    Console.WriteLine($"\n--- Заповнення матриці B {n}x{1} ---");
                    double[,] matrixB = FillMatrixFromConsole(n, 1);

                    logger.Log("Вхідна матриця B:");
                    LogMatrix(matrixB, logger);
                    logger.Log("Протокол обчислення:\n");

                    Console.WriteLine("\n");

                    newMatrix = InverseMatrix(myMatrix, logger);

                    if (newMatrix == null)
                    {
                        Console.WriteLine("\n");
                        break;
                    }

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\n--- Обернена матриця A ---");

                    logger.Log("\nОбернена матриця:");
                    LogMatrix(newMatrix, logger);
                    PrintMatrix(newMatrix, "invers");

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("\n");
                    Console.WriteLine($"\n--- Обчислення розв'язків Х ---");
                    string s = VectorX(newMatrix, matrixB);
                    Console.WriteLine(s);

                    logger.Log("\nОбчислення розв'язків Х:");
                    logger.Log(s);

                    Console.WriteLine();
                    Console.ResetColor();
                    break;

                case 4:
                    logger.Log("\n\n=== Пошук опорного й оптимального розв'язку ЗЛП ===\n\n");

                    Console.WriteLine("Оберіть тип задачі:");
                    Console.WriteLine("1 - Максимізація (Max)");
                    Console.WriteLine("2 - Мінімізація (Min)");
                    Console.Write("=> ");
                    int optChoice = GetValidMenuChoice(1, 2);
                    bool isMax = (optChoice == 1);

                    string optType = isMax ? "MAX" : "MIN";
                    logger.Log($"Тип задачі: {optType}");

                    logger.Log("Вхідна симплекс-таблиця:");
                    Console.Write(UIStrings.M4SizeRows);
                    rows = GetValidMenuChoice(2, 100);
                    Console.Write(UIStrings.M4SizeCols);
                    cols = GetValidMenuChoice(2, 100);

                    Console.WriteLine($"\n--- Заповнення симплекс-таблиці {rows}x{cols} ---");
                    double[,] table = FillTableFromConsole(rows, cols);

                    LogMatrix(table, logger);
                    logger.Log("Протокол обчислення:\n");
                    Console.WriteLine("\n");

                    var result = SolveSimplex(table, isMax, logger);

                    Console.ForegroundColor = result.Success ? ConsoleColor.DarkGreen : ConsoleColor.Red;
                    Console.WriteLine(result.Message);
                    logger.Log(result.Message);

                    if (result.FinalTable != null)
                    {
                        Console.ResetColor();
                        Console.WriteLine("\n--- Фінальна симплекс-таблиця ---");
                        PrintSimplexTable(result.FinalTable, result.VarLabels, result.RowLabels);

                        logger.Log("\nФінальна симплекс-таблиця:");
                        LogMatrix(result.FinalTable, logger);

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("\n--- Розв'язок ---");
                        Console.WriteLine(result.SolutionText);
                        logger.Log("\nРозв'язок:");
                        logger.Log(result.SolutionText);
                    }

                    Console.ResetColor();
                    Console.WriteLine();
                    break;
            }
        }
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
        Console.Write(UIStrings.Error);
        Console.ResetColor();
        Console.Write(ex);
    }

    static double[,] FillMatrixFromConsole(int rows, int cols)
    {
        double[,] matrix = new double[rows, cols];

        for (int i = 0; i < cols; i++)
            Console.Write($"\tX{i + 1}");

        for (int i = 0; i < rows; i++) //Rows
        {
            Console.Write($"\nY{i + 1}\t");
            for (int j = 0; j < cols; j++) //Columns
            {
                matrix[i, j] = GetDoubleFromKey();
                Console.Write("\t");
            }
        }

        return matrix;
    }

    static double[,] FillTableFromConsole(int rows, int cols)
    {
        double[,] matrix = new double[rows, cols];

        for (int j = 0; j < cols - 1; j++)
            Console.Write($"\tX{j + 1}");
        Console.Write($"\t1"); // останній стовпець

        for (int i = 0; i < rows; i++)
        {
            if (i == rows - 1) Console.Write($"\nZ\t");
            else Console.Write($"\nY{i + 1}\t");

            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = GetDoubleFromKey();
                Console.Write("\t");
            }
        }

        return matrix;
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

    static double[,] InverseMatrix(double[,] originalMatrix, ILogger logger)
    {
        int rows = originalMatrix.GetLength(0);
        double[,] inverseMatrix = originalMatrix;

        for (int i = 0; i < rows; i++)
        {
            logger.Log($"Крок {i + 1}:");

            double[,] temp = PivotOperation(inverseMatrix, i, i);

            if (temp != null)
                inverseMatrix = temp;
            else
            {
                WriteError($"Крок неможливий (елемент [{i},{i}] = 0). Матриця вироджена, оберненої не існує.");
                logger.Log($"Крок неможливий (елемент [{i},{i}] = 0). Матриця вироджена, оберненої не існує.\n");
                return null!;
            }

            logger.Log("\nМатриця після виконання ЗЖВ:");
            LogMatrix(inverseMatrix, logger);
        }

        return inverseMatrix;
    }

    static double[,] PivotOperation(double[,] prev, int r, int s)
    {
        int rows = prev.GetLength(0);
        int cols = prev.GetLength(1);
        double[,] next = new double[rows, cols];
        double p = prev[r, s];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (i == r && j == s)
                    next[i, j] = 1.0 / p;                                    // Дозвільний елемент
                else if (i == r)
                    next[i, j] = prev[i, j] / p;                             // Дозвільний рядок
                else if (j == s)
                    next[i, j] = -prev[i, j] / p;                            // Дозвільний стовпець
                else
                    next[i, j] = prev[i, j] - (prev[i, s] * prev[r, j]) / p; // Правило прямокутника
            }
        }
        return next;
    }

    static void LogMatrix(double[,] matrix, ILogger logger)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            string row = "";
            for (int j = 0; j < cols; j++)
                row += $"{matrix[i, j],8:F3}";
            logger.Log(row);
        }
        logger.Log("");
    }

    static int GetMatrixRank(double[,] originalMatrix, ILogger logger)
    {
        int rows = originalMatrix.GetLength(0);
        int cols = originalMatrix.GetLength(1);
        double[,] currentMatrix = originalMatrix;

        // Масиви-прапорці: тут відмічаємо рядки та стовпці, які вже використовували як розв'язувальні
        bool[] usedRows = new bool[rows];
        bool[] usedCols = new bool[cols];

        int rank = 0;
        int maxSteps = Math.Min(rows, cols);

        for (int step = 0; step < maxSteps; step++)
        {
            double maxPivot = 0;
            int pivotRow = -1;
            int pivotCol = -1;

            for (int i = 0; i < rows; i++)
            {
                if (usedRows[i]) continue;

                for (int j = 0; j < cols; j++)
                {
                    if (usedCols[j]) continue;

                    if (Math.Abs(currentMatrix[i, j]) > Math.Abs(maxPivot))
                    {
                        maxPivot = currentMatrix[i, j];
                        pivotRow = i;
                        pivotCol = j;
                    }
                }
            }

            if (Math.Abs(maxPivot) < 1e-10)
                break;

            logger.Log($"Крок {step + 1}:");

            currentMatrix = PivotOperation(currentMatrix, pivotRow, pivotCol);

            logger.Log("\nМатриця після виконання ЗЖВ:");
            LogMatrix(currentMatrix, logger);

            usedRows[pivotRow] = true;
            usedCols[pivotCol] = true;
            rank++;
        }

        return rank;
    }

    static string VectorX(double[,] matrixA, double[,] matrixB)
    {
        string str = "";
        double x;
        for (int i = 0; i < matrixA.GetLength(0); i++)
        {
            str += $"\nX[{i + 1}] = ";
            x = 0;
            for (int j = 0; j < matrixA.GetLength(0); j++)
            {
                x += matrixB[j, 0] * matrixA[i, j];
                if (matrixA[i, j] < 0)
                    str += $"{matrixB[j, 0]} * ({matrixA[i, j]:F3}) + ";
                else
                    str += $"{matrixB[j, 0]} * {matrixA[i, j]:F3} + ";
            }
            str = str.Substring(0, str.Length - 2);
            str += $"= {x:F3}";
        }

        return str;
    }

    static void PrintMatrix(double[,] matrix, string type)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        string topLabel = type == "original" ? "X" : "Y";
        string sideLabel = type == "original" ? "Y" : "X";

        for (int i = 0; i < cols; i++)
            Console.Write($"\t{topLabel}{i + 1}");

        for (int i = 0; i < rows; i++)
        {
            Console.Write($"\n{sideLabel}{i + 1}\t");

            for (int j = 0; j < rows; j++)
                Console.Write($"{matrix[i, j]:F3}\t");
        }
    }
    private sealed class SimplexResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = "";
        public double[,]? FinalTable { get; init; }
        public string[] VarLabels { get; init; } = Array.Empty<string>();
        public string[] RowLabels { get; init; } = Array.Empty<string>();
        public string SolutionText { get; init; } = "";
    }

    static SimplexResult SolveSimplex(double[,] initialTable, bool isMax, ILogger logger)
    {
        double[,] t = initialTable;
        int rows = t.GetLength(0);
        int cols = t.GetLength(1);

        int m = rows - 1;       // кількість обмежень
        int n = cols - 1;       // кількість початкових змінних
        int rhs = cols - 1;     // індекс стовпця вільних членів

        if (rows < 2 || cols < 2)
            return new SimplexResult { Success = false, Message = "Помилка розмірності." };

        string[] varLabels = new string[n];
        for (int j = 0; j < n; j++) varLabels[j] = $"X{j + 1}";

        string[] rowLabels = new string[m + 1];
        for (int i = 0; i < m; i++) rowLabels[i] = $"Y{i + 1}";
        rowLabels[m] = "Z";

        int maxIter = 100;
        int iter = 0;

        // ==========================================
        // Пошук опорного розв'язку
        // ==========================================
        logger.Log("\n=== Пошук опорного розв'язку ===");
        while (true)
        {
            iter++;
            if (iter > maxIter) break;

            int pivotRow = -1;
            double minRhs = -1e-10;
            for (int i = 0; i < m; i++)
            {
                if (t[i, rhs] < minRhs)
                {
                    minRhs = t[i, rhs];
                    pivotRow = i;
                }
            }

            if (pivotRow == -1) break; // Усі вільні члени >= 0, переходимо до пошуку оптимального рішення

            int pivotCol = -1;
            double minVal = -1e-10;
            // Шукаємо перший від'ємний елемент у рядку
            for (int j = 0; j < n; j++)
            {
                if (t[pivotRow, j] < minVal)
                {
                    minVal = t[pivotRow, j];
                    pivotCol = j;
                    break;
                }
            }

            if (pivotCol == -1)
                return new SimplexResult { Success = false, Message = "Система обмежень суперечлива." };

            string entering = varLabels[pivotCol];
            string leaving = rowLabels[pivotRow];

            t = PivotOperation(t, pivotRow, pivotCol);

            // Міняємо мітки місцями
            rowLabels[pivotRow] = entering;
            varLabels[pivotCol] = leaving;

            logger.Log($"\nІтерація {iter} (Фаза 1): Дозвільний елемент [{leaving}, {entering}]");
            LogMatrix(t, logger);
        }

        // ==========================================
        // Пошук оптимального розв'язку (Прямий метод)
        // ==========================================
        logger.Log("\n=== Пошук оптимального розв'язку ===");
        iter = 0;
        while (true)
        {
            iter++;
            if (iter > maxIter) break;

            int pivotCol = -1;
            double mostNeg = -1e-10;
            for (int j = 0; j < n; j++)
            {
                if (t[m, j] <= mostNeg)
                {
                    mostNeg = t[m, j];
                    pivotCol = j;
                }
            }

            if (pivotCol == -1) break; // Немає від'ємних у Z-рядку -> Оптимум!

            int pivotRow = -1;
            double minRatio = double.PositiveInfinity;

            for (int i = 0; i < m; i++)
            {
                if (t[i, pivotCol] > 1e-10) м
                {
                    double ratio = t[i, rhs] / t[i, pivotCol];
                    if (ratio < minRatio)
                    {
                        minRatio = ratio;
                        pivotRow = i;
                    }
                }
            }

            if (pivotRow == -1)
                return new SimplexResult { Success = false, Message = "Задача необмежена." };

            string entering = varLabels[pivotCol];
            string leaving = rowLabels[pivotRow];

            t = PivotOperation(t, pivotRow, pivotCol);

            rowLabels[pivotRow] = entering;
            varLabels[pivotCol] = leaving;

            logger.Log($"\nІтерація {iter}: Дозвільний елемент [{leaving}, {entering}]");
            LogMatrix(t, logger);
        }

        SortSimplexTable(t, varLabels, rowLabels);

        return new SimplexResult
        {
            Success = true,
            Message = "Оптимальний розв'язок знайдено!",
            FinalTable = t,
            VarLabels = varLabels,
            RowLabels = rowLabels,
            SolutionText = BuildSolutionText(t, varLabels, rowLabels, n, isMax)
        };
    }

    static string BuildSolutionText(double[,] t, string[] varLabels, string[] rowLabels, int originalVarsCount, bool isMax)
    {
        int rows = t.GetLength(0);
        int m = rows - 1;
        int rhs = t.GetLength(1) - 1;

        double[] x = new double[originalVarsCount];
        for (int i = 0; i < originalVarsCount; i++)
        {
            string targetVar = $"x{i + 1}";
            int rowIndex = Array.IndexOf(rowLabels, targetVar);

            if (rowIndex >= 0) x[i] = t[rowIndex, rhs];
            else x[i] = 0;
        }

        string xStr = string.Join("; ", x.Select(v => Math.Abs(v) < 1e-10 ? "0" : v.ToString("0.###")));
        string res = $"X = ({xStr})\n";

        double zVal = t[m, rhs];
        if (!isMax) zVal = -zVal; // Для мінімізації просто міняємо знак фінального числа

        string optType = isMax ? "Max" : "Min";
        res += $"{optType} (Z) = {zVal:0.###}";

        return res;
    }

    static void SortSimplexTable(double[,] t, string[] varLabels, string[] rowLabels)
    {
        int rows = t.GetLength(0);
        int cols = t.GetLength(1);
        int m = rows - 1; // Без Z-рядка
        int n = cols - 1; // Без стовпця RHS

        // Сортування стовпців
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (string.Compare(varLabels[i], varLabels[j]) > 0)
                {
                    // Міняємо мітки
                    string temp = varLabels[i];
                    varLabels[i] = varLabels[j];
                    varLabels[j] = temp;

                    // Міняємо самі стовпці у матриці
                    for (int r = 0; r < rows; r++)
                    {
                        double tempVal = t[r, i];
                        t[r, i] = t[r, j];
                        t[r, j] = tempVal;
                    }
                }
            }
        }

        // Сортування рядків
        for (int i = 0; i < m - 1; i++)
        {
            for (int j = i + 1; j < m; j++)
            {
                if (string.Compare(rowLabels[i], rowLabels[j]) > 0)
                {
                    // Міняємо мітки
                    string temp = rowLabels[i];
                    rowLabels[i] = rowLabels[j];
                    rowLabels[j] = temp;

                    // Міняємо самі рядки у матриці
                    for (int c = 0; c < cols; c++)
                    {
                        double tempVal = t[i, c];
                        t[i, c] = t[j, c];
                        t[j, c] = tempVal;
                    }
                }
            }
        }
    }

    static void PrintSimplexTable(double[,] t, string[] varLabels, string[] rowLabels)
    {
        int rows = t.GetLength(0);
        int cols = t.GetLength(1);
        int n = cols - 1;

        Console.Write("\t");
        for (int j = 0; j < n; j++)
            Console.Write($"-{varLabels[j]}\t"); // Додаємо мінус
        Console.Write("1\n");

        for (int i = 0; i < rows; i++)
        {
            Console.Write($"{rowLabels[i]} =\t");
            for (int j = 0; j < cols; j++)
            {
                double val = Math.Abs(t[i, j]) < 1e-10 ? 0.0 : t[i, j];
                Console.Write($"{val:0.###}\t");
            }
            Console.WriteLine();
        }
    }
}

