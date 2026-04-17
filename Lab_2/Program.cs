using System.Globalization;
using System.Text;

namespace Lab_2
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
                Console.Write("┌───────── ГОЛОВНЕ МЕНЮ ──────────┐\n");
                Console.Write("│ 1 - Ввести матрицю вручну       │\n");
                Console.Write("│ 0 - Вихід                       │\n");
                Console.Write("└─────────────────────────────────┘\n");
                Console.Write("Відповідь => ");

                int choice = GetValidMenuChoice(0, 1);

                Console.WriteLine();

                if (choice == 0)
                {
                    isRunning = false;
                    break;
                }

                Console.WriteLine("Оберіть тип задачі:   ");
                Console.Write("1 - Пошук максимума   \n");
                Console.Write("2 - Пошук мінімума    \n");
                Console.Write("Відповідь => ");
                int taskType = GetValidMenuChoice(1, 2);

                double[,] userMatrix;
                int rows, cols;
                logger.Log("\n\n=== РОЗВ'ЯЗАННЯ З ВЛАСНОЮ МАТРИЦЕЮ ===");
                Console.Write("\nВведіть кількість рядків => ");
                rows = GetValidMenuChoice(2, 20);
                Console.Write("Введіть кількість стовпців => ");
                cols = GetValidMenuChoice(2, 20);

                Console.WriteLine("\n Заповніть матрицю:");
                userMatrix = FillTableFromConsole(rows, cols);

                // Ініціалізація міток (Labels)
                string[] xLabels = new string[cols];
                string[] vLabels = new string[cols];
                for (int i = 0; i < cols - 1; i++) { xLabels[i] = "x" + (i + 1); vLabels[i] = "v" + (i + 1); }
                xLabels[cols - 1] = "1"; vLabels[cols - 1] = "W";

                string[] yLabels = new string[rows];
                string[] uLabels = new string[rows];
                for (int i = 0; i < rows - 1; i++) { yLabels[i] = "y" + (i + 1); uLabels[i] = "u" + (i + 1); }
                yLabels[rows - 1] = "z"; uLabels[rows - 1] = "1";

                if (choice == 1)
                {
                    for (int i = 0; i < rows - 1; i++)
                    {
                        Console.Write($"Чи зробити мітку рядка {i + 1} нульовою? (1 - Так, 0 - Ні): ");
                        if (GetValidMenuChoice(0, 1) == 1)
                        {
                            yLabels[i] = "0";
                        }
                    }
                }

                SolveProblem(userMatrix, xLabels, yLabels, uLabels, vLabels, logger, taskType);
            }
        }

        static void SolveProblem(double[,] matrix, string[] xLabels, string[] yLabels, string[] uLabels, string[] vLabels, ILogger logger, int taskType)
        {
            int originalRowSize = matrix.GetLength(0);
            int originalColSize = matrix.GetLength(1);

            Console.WriteLine("\nПочаткова матриця:");
            logger.Log("Початкова матриця:");
            PrintMatrix(matrix, xLabels, yLabels, uLabels, vLabels, false, logger);

            // Отримання та друк рівнянь
            var equations = GetEquations(matrix, uLabels, vLabels);
            PrintEquations(equations, logger);

            // 1. Позбавляємося нульових міток
            bool hasZeroLabel = yLabels.Contains("0");
            if (hasZeroLabel)
            {
                logger.Log("\n--- Етап 1: Видалення нульових міток ---");
                Console.WriteLine("\n--- Етап 1: Видалення нульових міток ---");
                bool foundZeroLabel = false;
                while (!foundZeroLabel)
                {
                    foundZeroLabel = ProcessZeroLabelRow(ref matrix, ref xLabels, ref yLabels, ref uLabels, ref vLabels, false, logger);
                    if (!foundZeroLabel)
                        DeleteZeroCols(ref matrix, ref xLabels, ref vLabels, logger);
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
                foundBasicSolution = FindBasicSolution(ref matrix, ref xLabels, ref yLabels, ref uLabels, ref vLabels, false, logger);
            }

            string xSol = GetSolutionPoint(matrix, yLabels, "x", originalColSize);
            string vSol = GetConstrainedSolutionPoint(matrix, vLabels, "v", originalColSize, out double[] vRaw);
            string uSol = GetUSolutionPoint(matrix, vLabels, originalRowSize, equations, vRaw);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\nОпорний розв'язок знайдено.");
            Console.WriteLine($"Результат для прямої задачі: X {xSol}");
            Console.WriteLine($"Результат для двоїстої: {(hasZeroLabel ? $"V {vSol}   " : "")}U {uSol}\n");
            logger.Log($"Опорний розв'язок знайдено. Результат X: {xSol}, U: {uSol}");
            Console.ResetColor();

            // 3. Пошук оптимального розв'язку
            logger.Log("\n--- Етап 3: Пошук оптимального розв'язку ---");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Шукаємо оптимальний розв'язок!");
            Console.ResetColor();

            bool foundOptimalSolution = false;
            while (!foundOptimalSolution)
            {
                foundOptimalSolution = FindOptimalSolution(ref matrix, ref xLabels, ref yLabels, ref uLabels, ref vLabels, false, logger);
            }

            xSol = GetSolutionPoint(matrix, yLabels, "x", originalColSize);
            vSol = GetConstrainedSolutionPoint(matrix, vLabels, "v", originalColSize, out vRaw);
            uSol = GetUSolutionPoint(matrix, vLabels, originalRowSize, equations, vRaw);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\nОптимальний розв'язок знайдено.");
            Console.WriteLine($"Результат для прямої задачі: X {xSol}");
            Console.WriteLine($"Результат для двоїстої: {(hasZeroLabel ? $"V {vSol}   " : "")}U {uSol}\n");
            logger.Log($"Оптимальний розв'язок знайдено. Результат X: {xSol}, U: {uSol}");
            Console.ResetColor();

            double zMax = matrix[matrix.GetLength(0) - 1, matrix.GetLength(1) - 1];
            if (taskType == 1)
            {
                Console.WriteLine($"Max (Z) = {zMax}");
                Console.WriteLine($"Min (W) = {zMax}\n");
                logger.Log($"Max (Z) = {zMax}");
            }
            else
            {
                Console.WriteLine($"Min (Z) = {-zMax}");
                Console.WriteLine($"Max (W) = {-zMax}\n");
                logger.Log($"Min (Z) = {-zMax}");
            }

            SolveDualMatrix(equations, logger, taskType);
        }

        static void SolveDualMatrix(Dictionary<string, Dictionary<string, double>> equations, ILogger logger, int taskType)
        {
            int numRows = equations.Count;
            int numCols = equations.First().Value.Count;

            double[,] dualMatrix = new double[numRows, numCols];
            int rowIndex = 0;
            foreach (var outerEntry in equations)
            {
                int colIndex = 0;
                if (rowIndex == numRows - 1)
                {
                    foreach (var innerEntry in outerEntry.Value)
                        dualMatrix[rowIndex, colIndex++] = innerEntry.Value;
                }
                else
                {
                    foreach (var innerEntry in outerEntry.Value)
                    {
                        if (colIndex == numCols - 1) dualMatrix[rowIndex, colIndex] = innerEntry.Value;
                        else dualMatrix[rowIndex, colIndex] = -innerEntry.Value;
                        colIndex++;
                    }
                }
                rowIndex++;
            }

            string[] dualXLabels = new string[numCols];
            string[] dualYLabels = new string[numRows]; // Не використовується активно для двоїстої, але потрібно для сумісності 
            string[] dualULabels = new string[numCols];
            string[] dualVLabels = new string[numRows];

            for (int i = 0; i < numCols; i++) dualULabels[i] = (i == numCols - 1) ? "1" : "u" + (i + 1);
            for (int i = 0; i < numRows; i++) dualVLabels[i] = (i == numRows - 1) ? "W" : "v" + (i + 1);

            Console.WriteLine("W_matrix with labels:\n");
            logger.Log("\nW_matrix with labels:");
            PrintMatrix(dualMatrix, dualXLabels, dualYLabels, dualULabels, dualVLabels, true, logger);

            bool foundBasic = false;
            while (!foundBasic)
                foundBasic = FindBasicSolution(ref dualMatrix, ref dualXLabels, ref dualYLabels, ref dualULabels, ref dualVLabels, true, logger);

            bool foundOptimal = false;
            while (!foundOptimal)
                foundOptimal = FindOptimalSolution(ref dualMatrix, ref dualXLabels, ref dualYLabels, ref dualULabels, ref dualVLabels, true, logger);

            string uCheckedSol = GetSolutionPoint(dualMatrix, dualVLabels, "u", numCols);
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\nОптимальний розв'язок двоїстої матриці знайдено.\nРезультат (U): {uCheckedSol}\n");
            logger.Log($"Оптимальний розв'язок двоїстої: U {uCheckedSol}");
            Console.ResetColor();

            double[,] finalMatrix = dualMatrix;
            double minW = -finalMatrix[finalMatrix.GetLength(0) - 1, finalMatrix.GetLength(1) - 1];
            if (taskType == 1)
            {
                Console.WriteLine($"Min (W) = {minW}\n");
                logger.Log($"Min (W) = {minW}");
            }
            else
            {
                Console.WriteLine($"Max (W) = {-minW}\n");
                logger.Log($"Max (W) = {-minW}");
            }
        }

        // ==========================================================
        // МАТЕМАТИЧНА ЛОГІКА ТА РІВНЯННЯ
        // ==========================================================

        static Dictionary<string, Dictionary<string, double>> GetEquations(double[,] matrix, string[] uLabels, string[] vLabels)
        {
            var equation = new Dictionary<string, Dictionary<string, double>>();
            for (int col = 0; col < matrix.GetLength(1); col++)
            {
                var keyValuePairs = new Dictionary<string, double>();
                for (int row = 0; row < matrix.GetLength(0); row++)
                {
                    keyValuePairs.Add(uLabels[row], matrix[row, col]);
                }
                equation.Add(vLabels[col], keyValuePairs);
            }
            return equation;
        }

        static double SolveEquation(Dictionary<string, double> equationTerms, double[] vSolution, double[] uSolution, Dictionary<string, Dictionary<string, double>> allEquations)
        {
            double result = 0;
            foreach (var term in equationTerms)
            {
                string variable = term.Key;
                double coefficient = term.Value;

                if (variable.StartsWith("u"))
                {
                    int index = int.Parse(variable.Substring(1)) - 1;
                    if (allEquations.ContainsKey(variable))
                    {
                        uSolution[index] = SolveEquation(allEquations[variable], vSolution, uSolution, allEquations);
                    }
                    result += coefficient * uSolution[index];
                }
                else if (variable.StartsWith("v"))
                {
                    int index = int.Parse(variable.Substring(1)) - 1;
                    result += coefficient * vSolution[index];
                }
                else if (variable == "1")
                {
                    result += coefficient;
                }
            }
            return result;
        }

        // ==========================================================
        // АЛГОРИТМИ СИМПЛЕКС-МЕТОДУ
        // ==========================================================

        static bool ProcessZeroLabelRow(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, ref string[] uLabels, ref string[] vLabels, bool isDual, ILogger logger)
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
                        FindSmallestPositiveAndPivot(ref matrix, ref xLabels, ref yLabels, ref uLabels, ref vLabels, resultCol, i, "find_zero", isDual, logger);
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

        static bool FindBasicSolution(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, ref string[] uLabels, ref string[] vLabels, bool isDual, ILogger logger)
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
                        FindSmallestPositiveAndPivot(ref matrix, ref xLabels, ref yLabels, ref uLabels, ref vLabels, resultCol, i, "basic", isDual, logger);
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

        static bool FindOptimalSolution(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, ref string[] uLabels, ref string[] vLabels, bool isDual, ILogger logger)
        {
            int lastRowIndex = matrix.GetLength(0) - 1;
            int numCols = matrix.GetLength(1);
            bool noNegativeFound = true;

            for (int j = 0; j < numCols - 1; j++)
            {
                if (matrix[lastRowIndex, j] < 0)
                {
                    noNegativeFound = false;
                    FindSmallestPositiveAndPivot(ref matrix, ref xLabels, ref yLabels, ref uLabels, ref vLabels, j, -1, "optimal", isDual, logger);
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

        static void FindSmallestPositiveAndPivot(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, ref string[] uLabels, ref string[] vLabels, int colIndex, int rowIndexLastCol, string solutionType, bool isDual, ILogger logger)
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

            PerformModifiedJordanElimination(ref matrix, ref xLabels, ref yLabels, ref uLabels, ref vLabels, smallestInRow, colIndex, isDual, logger);
        }

        static void PerformModifiedJordanElimination(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, ref string[] uLabels, ref string[] vLabels, int inRow, int inCol, bool isDual, ILogger logger)
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
                    matrix[i, j] = SmartRound(matrix[i, j]);
                }
            }

            string temp;
            if (!isDual)
            {
                temp = xLabels[inCol]; xLabels[inCol] = yLabels[inRow]; yLabels[inRow] = temp;
                temp = vLabels[inCol]; vLabels[inCol] = uLabels[inRow]; uLabels[inRow] = temp;
            }
            else
            {
                temp = uLabels[inCol]; uLabels[inCol] = vLabels[inRow]; vLabels[inRow] = temp;
            }

            logger.Log($"\nМатриця після ЖВ (рядок {inRow + 1}, стовпець {inCol + 1}):");
            Console.WriteLine($"\nМатриця після обробки елемента [{inRow + 1}, {inCol + 1}]:");
            PrintMatrix(matrix, xLabels, yLabels, uLabels, vLabels, isDual, logger);
        }

        static void DeleteZeroCols(ref double[,] matrix, ref string[] xLabels, ref string[] vLabels, ILogger logger)
        {
            int numRows = matrix.GetLength(0);
            int numCols = matrix.GetLength(1);

            int colToRemove = Array.IndexOf(xLabels, "0");
            if (colToRemove == -1) return;

            double[,] newMatrix = new double[numRows, numCols - 1];
            string[] newXLabels = new string[numCols - 1];
            string[] newVLabels = new string[numCols - 1];

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
                newVLabels[newLabelIndex] = vLabels[j];
                newLabelIndex++;
            }

            matrix = newMatrix;
            xLabels = newXLabels;
            vLabels = newVLabels;

            logger.Log("Матриця після видалення стовпця '0':");
            Console.WriteLine("\nМатриця після видалення стовпця '0':");
            PrintMatrix(matrix, xLabels, null, null, vLabels, false, logger);
        }

        // ==========================================================
        // ДОПОМІЖНІ МЕТОДИ ВИДОБУТКУ РОЗВ'ЯЗКІВ
        // ==========================================================

        static string GetSolutionPoint(double[,] matrix, string[] currentLabels, string prefix, int originalSize)
        {
            int numRows = matrix.GetLength(0);
            int numCols = matrix.GetLength(1);
            double[] solutionArray = new double[originalSize - 1];

            for (int i = 0; i < numRows - 1; i++)
            {
                if (currentLabels[i].StartsWith(prefix))
                {
                    int index = int.Parse(currentLabels[i].Substring(1)) - 1;
                    if (index < solutionArray.Length)
                        solutionArray[index] = matrix[i, numCols - 1];
                }
            }

            return "(" + string.Join("; ", solutionArray.Select(x => x.ToString("F3"))) + ")";
        }

        static string GetConstrainedSolutionPoint(double[,] matrix, string[] vLabels, string labelPrefix, int originalSize, out double[] rawSolution)
        {
            int numRows = matrix.GetLength(0);
            int numCols = matrix.GetLength(1);
            rawSolution = new double[originalSize - 1];

            for (int j = 0; j < numCols - 1; j++)
            {
                if (vLabels[j].StartsWith(labelPrefix))
                {
                    int index = int.Parse(vLabels[j].Substring(1)) - 1;
                    rawSolution[index] = matrix[numRows - 1, j];
                }
            }

            return "(" + string.Join("; ", rawSolution.Select(x => x.ToString("F3"))) + ")";
        }

        static string GetUSolutionPoint(double[,] matrix, string[] currentCols, int originalSize, Dictionary<string, Dictionary<string, double>> equations, double[] vSolution)
        {
            GetConstrainedSolutionPoint(matrix, currentCols, "u", originalSize, out double[] uSolution);

            foreach (var key in equations.Keys.Where(k => k.StartsWith("u")))
            {
                int index = int.Parse(key.Substring(1));
                uSolution[index - 1] = SolveEquation(equations[key], vSolution, uSolution, equations);
            }

            return "(" + string.Join("; ", uSolution.Select(x => x.ToString("F3"))) + ")";
        }

        static double SmartRound(double value)
        {
            double roundedValue = Math.Round(value, 10);
            if (Math.Abs(roundedValue - Math.Round(roundedValue)) < 1e-9)
            {
                return Math.Round(roundedValue);
            }
            return roundedValue;
        }

        // ==========================================================
        // ДОПОМІЖНІ МЕТОДИ ДЛЯ ІНТЕРФЕЙСУ ТА ЛОГУВАННЯ
        // ==========================================================

        static void PrintMatrix(double[,] matrix, string[] xLabels, string[] yLabels, string[] uLabels, string[] vLabels, bool isDual, ILogger logger)
        {
            int numRows = matrix.GetLength(0);
            int numCols = matrix.GetLength(1);

            if (!isDual)
            {
                Console.Write("\t ");
                string headerLog1 = "\t ";
                for (int i = 0; i < numCols; i++)
                {
                    string l = vLabels[i] + "\t ";
                    Console.Write(l); headerLog1 += l;
                }
                Console.WriteLine(); logger.Log(headerLog1);

                Console.Write("\t ");
                string headerLog2 = "\t ";
                for (int i = 0; i < numCols; i++)
                {
                    string label = (i != numCols - 1) ? $"-{xLabels[i]}" : xLabels[i];
                    Console.Write(label + "\t "); headerLog2 += label + "\t ";
                }
                Console.WriteLine(); logger.Log(headerLog2);

                for (int i = 0; i < numRows; i++)
                {
                    string rowLabel = uLabels != null && yLabels != null ? $"{uLabels[i]}, {yLabels[i]}" : "";
                    Console.Write(rowLabel + "\t");
                    string rowLog = rowLabel + "\t";

                    for (int j = 0; j < numCols; j++)
                    {
                        Console.Write($"{matrix[i, j],6:F2}\t");
                        rowLog += $"{matrix[i, j],6:F2}\t";
                    }
                    Console.WriteLine(); logger.Log(rowLog);
                }
            }
            else
            {
                Console.Write("\t");
                string headerLog = "\t";
                for (int i = 0; i < numCols; i++)
                {
                    string label = (i != numCols - 1) ? $"-{uLabels[i]}" : uLabels[i];
                    Console.Write(label + "\t"); headerLog += label + "\t";
                }
                Console.WriteLine(); logger.Log(headerLog);

                for (int i = 0; i < numRows; i++)
                {
                    Console.Write(vLabels[i] + "\t");
                    string rowLog = vLabels[i] + "\t";
                    for (int j = 0; j < numCols; j++)
                    {
                        Console.Write($"{matrix[i, j],6:F2}\t");
                        rowLog += $"{matrix[i, j],6:F2}\t";
                    }
                    Console.WriteLine(); logger.Log(rowLog);
                }
            }
            logger.Log("");
        }

        static void PrintEquations(Dictionary<string, Dictionary<string, double>> equation, ILogger logger)
        {
            if (equation.ContainsKey("W"))
            {
                var wTerms = equation["W"];
                Console.Write("W = ");
                logger.Log("W = ");
                PrintTerms(wTerms, logger, " → min");
            }

            foreach (var mainEntry in equation)
            {
                string resultLabel = mainEntry.Key;
                if (resultLabel.StartsWith("W")) continue;

                Console.Write($"{resultLabel} = ");
                logger.Log($"{resultLabel} = ");
                PrintTerms(mainEntry.Value, logger, resultLabel.StartsWith("v") ? " ≥ 0\n" : "\n\n");
            }
        }

        static void PrintTerms(Dictionary<string, double> terms, ILogger logger, string suffix)
        {
            bool firstTerm = true;
            string line = "";
            foreach (var term in terms)
            {
                double coefficient = term.Value;
                string label = term.Key;
                if (coefficient == 0) continue;

                if (!firstTerm)
                {
                    line += coefficient >= 0 ? " + " : " - ";
                    coefficient = Math.Abs(coefficient);
                }
                else if (coefficient < 0)
                {
                    line += "-";
                    coefficient = Math.Abs(coefficient);
                }

                if (coefficient == 1 && label == "1") line += label;
                else if (coefficient == 1) line += label;
                else if (label == "1") line += $"{coefficient}";
                else line += $"{coefficient}*{label}";

                firstTerm = false;
            }
            line += suffix;
            Console.Write(line);
            logger.Log(line);
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
                        return result;
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
}