using System.Text;

namespace DZ
{
    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            string logFilePath = "protocol.txt";
            File.WriteAllText(logFilePath, $"--- ПРОТОКОЛ ({DateTime.Now}) ---\n", Encoding.UTF8);

            DualWriter dualWriter = new DualWriter(Console.Out, logFilePath);
            Console.SetOut(dualWriter);

            //Input data for TEST1
            double[,] test1MatrixZ =
            {
                {2, 2, 1, 1, 1}, //Z1
                {1, -3, 5, -1, -2}, //Z2
                {1, -4, 5, 9, -2} //Z3
            };

            bool[] test1Max =
            [
                true, //Z1 -> max
                false, //Z2 -> min
                true //Z3 -> max
            ];

            double[,] test1MatrixLimits =
            {
                {1, 4, 3, 2, 1, 9},
                {-1, 2, -1, 2, 1, 6},
                {1, 2, 0, 2, -1, 2}
            };

            bool test1Equal = true;

            // Input data for TEST2
            double[,] test2MatrixZ =
            {
                {1, -8, 1, 4}, //Z1
                {-1, 3, 5, 1}, //Z2
                {3, 1, 1, -1} //Z3
            };

            bool[] test2Max =
            [
                true, //Z1 -> max
                false, //Z2 -> min
                true //Z3 -> max
            ];

            double[,] test2MatrixLimits =
            {
                {1, -1, 1, 1, 2},
                {1, 1, 1, -1, 2},
                {-1, 1, 1, 1, 2},
                {1, 1, -1, 1, 2}
            };

            bool test2Equal = false;

            // Input data for VARIANT2
            double[,] MyMatrixZ =
            {
                {-3, -1, 0, 0, 0, 0}, //Z1
                {0, -1, 3, 0, -2, 0}, //Z2
                {3, 1, 2, 1, 0, 0} //Z33
            };

            bool[] MyMatrixMax =
            [
                false, //Z1 -> min
                true, //Z2 -> max
                true //Z3 -> max
            ];

            double[,] MyMatrixLimits =
            {
                {2, 1, -1, 1, 0, 0, 2},
                {2, -1, 5, 0, 1, 0, 6},
                {4, 1, 1, 0, 0, 1, 6}
            };

            bool MyEqual = true;

            double[,] matrixZ = null!;
            bool[] matrixMax = null!;
            double[,] matrixLimits = null!;
            bool equal = true;

            while (true)
            {
                Console.Write("Оберіть матрицю:\n" +
                                  " 0) Тестова матриця №1\n" +
                                  " 1) Тестова матриця №2\n" +
                                  " 2) Матриця з варіанту №2\n" +
                                  "Відповідь => ");

                int choice = GetValidMenuChoice(0, 2);
                Console.WriteLine();

                switch (choice)
                {
                    case 0:
                        matrixZ = test1MatrixZ;
                        matrixMax = test1Max;
                        matrixLimits = test1MatrixLimits;
                        equal = test1Equal;
                        break;

                    case 1:
                        matrixZ = test2MatrixZ;
                        matrixMax = test2Max;
                        matrixLimits = test2MatrixLimits;
                        equal = test2Equal;
                        break;

                    case 2:
                        matrixZ = MyMatrixZ;
                        matrixMax = MyMatrixMax;
                        matrixLimits = MyMatrixLimits;
                        equal = MyEqual; ;
                        break;
                }
                Console.WriteLine("ВХІДНІ ДАНІ:\n" + PrintInputData(matrixZ, matrixMax, matrixLimits, equal));

                int rowsZ = matrixZ.GetLength(0);
                int colsZ = matrixZ.GetLength(1);
                double[,] matrixZTemp = (double[,])matrixZ.Clone();

                for (int i = 0; i < rowsZ; i++)
                {
                    if (!matrixMax[i])
                    {
                        for (int j = 0; j < colsZ; j++)
                            matrixZTemp[i, j] *= -1;
                    }
                }


                List<double[,]> simplexTables = [];
                List<double[]> optimalSolutions = [];

                for (int i = 0; i < rowsZ; i++)
                    simplexTables.Add(GetSimplexTable(matrixZTemp, i, matrixLimits));

                Console.WriteLine("ПОШУК ОПТИМАЛЬНИХ ВЕКТОРІВ:\n");

                switch (equal ? 1 : 0)
                {
                    case 1:
                        for (int i = 0; i < rowsZ; i++)
                        {
                            string[] userXLabels = new string[simplexTables[i].GetLength(1)];
                            for (int j = 0; j < simplexTables[i].GetLength(1) - 1; j++) userXLabels[j] = "x" + (j + 1);
                            userXLabels[simplexTables[i].GetLength(1) - 1] = "1";

                            string[] userYLabels = new string[simplexTables[i].GetLength(0)];
                            for (int j = 0; j < simplexTables[i].GetLength(0) - 1; j++) userYLabels[j] = "0";
                            userYLabels[simplexTables[i].GetLength(0) - 1] = $"Z{i + 1}";

                            Console.WriteLine($"Вхідна сиплекс-таблиця для Z{i + 1}:\n");
                            PrintMatrix(simplexTables[i], userXLabels, userYLabels);

                            optimalSolutions.Add(SolveProblem(simplexTables[i], userXLabels, userYLabels, matrixMax[i], i));
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"X{i + 1}* = ({string.Join("; ", optimalSolutions[i].Select(x => x.ToString("F2")))})\n");
                            Console.ResetColor();
                        }
                        break;

                    case 0:
                        for (int i = 0; i < rowsZ; i++)
                        {
                            string[] userXLabels = new string[simplexTables[i].GetLength(1)];
                            for (int j = 0; j < simplexTables[i].GetLength(1) - 1; j++) userXLabels[j] = "x" + (j + 1);
                            userXLabels[simplexTables[i].GetLength(1) - 1] = "1";

                            string[] userYLabels = new string[simplexTables[i].GetLength(0)];
                            for (int j = 0; j < simplexTables[i].GetLength(0) - 1; j++) userYLabels[j] = "y" + (j + 1);
                            userYLabels[simplexTables[i].GetLength(0) - 1] = $"Z{i + 1}";

                            Console.WriteLine($"Вхідна сиплекс-таблиця для Z{i + 1}:\n");
                            PrintMatrix(simplexTables[i], userXLabels, userYLabels);

                            optimalSolutions.Add(SolveProblem(simplexTables[i], userXLabels, userYLabels, matrixMax[i], i));
                            Console.ForegroundColor = ConsoleColor.Blue;
                            Console.WriteLine($"X{i + 1}* = ({string.Join("; ", optimalSolutions[i].Select(x => x.ToString("F2")))})\n");
                            Console.ResetColor();
                        }
                        break;
                }

                int rows = optimalSolutions.Count;
                int cols = optimalSolutions[0].Length;

                double[,] matrixOS = new double[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                        matrixOS[i, j] = optimalSolutions[i][j];
                }

                string[] label1 = new string[colsZ];
                for (int j = 0; j < colsZ; j++) label1[j] = "Z" + (j + 1);

                string[] label2 = new string[rowsZ];
                for (int j = 0; j < rowsZ; j++) label2[j] = $"X{j + 1}*:";

                Console.WriteLine($"Отрималми k = {optimalSolutions.Count} оптимальних вектори:\n");
                PrintOptimalVectors(matrixOS, label1, label2);

                label2 = new string[rowsZ];
                for (int j = 0; j < rowsZ; j++) label2[j] = $"C{j + 1}:";

                Console.WriteLine($"\nМатриця коефіцієнтів функції мети:\n");
                PrintOptimalVectors(matrixZ, label1, label2);

                double[,] R = CalculateRegretMatrix(matrixOS, matrixZ, matrixMax);

                Console.WriteLine("\nМатриця неоптимальних розв'язків:");
                for (int i = 0; i < R.GetLength(0); i++)
                {
                    for (int j = 0; j < R.GetLength(1); j++)
                    {
                        Console.Write($"{R[i, j]:F2}\t");
                    }
                    Console.WriteLine();
                }

                double[,] RTemp = (double[,])R.Clone();

                Console.WriteLine("\nМатриця програшу:\n");

                for (int i = 0; i < R.GetLength(0); i++)
                {
                    for (int j = 0; j < R.GetLength(1); j++)
                    {
                        if (RTemp[i, j] != 0)
                            RTemp[i, j] *= -1;

                        Console.Write($"{RTemp[i, j]}\t");
                    }
                    Console.WriteLine();
                }

                double max = AddMaxToAllElements(ref RTemp);
                Console.WriteLine("\nПошук розв'язків матричної гри:\n");

                if (SaddlePoint(RTemp) != -1.0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"Сідлова точка знайдена: {SaddlePoint(RTemp) - Math.Abs(max)}\n");
                    Console.ResetColor();
                    continue;
                }
                Console.WriteLine($"Сідлова точка не знайдена\n" +
                                  $"Шукаємо розв'язок шляхом розв'язання пари взаємно двоїстих ЗЛП:");

                rows = RTemp.GetLength(0) + 1;
                cols = RTemp.GetLength(1) + 1;
                double[,] newMatrix = new double[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (j == cols - 1)
                            newMatrix[i, j] = 1;
                        else if (i == rows - 1)
                            newMatrix[i, j] = -1;
                        else
                            newMatrix[i, j] = RTemp[i, j];
                    }
                }
                newMatrix[rows - 1, cols - 1] = 0;

                // Ініціалізація міток
                string[] pLabels = new string[rows];
                string[] rLabels = new string[rows];
                for (int i = 0; i < rows - 1; i++) { pLabels[i] = "p" + (i + 1); rLabels[i] = "r" + (i + 1); }
                pLabels[rows - 1] = "1"; rLabels[rows - 1] = "Z";

                string[] tLabels = new string[cols];
                string[] qLabels = new string[cols];
                for (int i = 0; i < cols - 1; i++) { tLabels[i] = "t" + (i + 1); qLabels[i] = "-q" + (i + 1); }
                tLabels[cols - 1] = "W"; qLabels[cols - 1] = "1";

                double[] X0 = new double[rows];
                double[] Y0 = new double[rows];

                SolveProblemPair(newMatrix, tLabels, qLabels, pLabels, rLabels, 1, max, ref X0, ref Y0);

                Console.Write("\nВагові коефіцієнти розв'язків: ");
                foreach (double el in X0)
                    Console.Write(el.ToString("F2") + "; ");

                Console.WriteLine("\n\nКомпропісний розв'язок: ");

                label1 = new string[colsZ];
                for (int j = 0; j < colsZ; j++) label1[j] = "x" + (j + 1);

                PrintResult(XCompromis(matrixOS, X0), label1);
                Console.WriteLine();
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

            static string PrintInputData(double[,] matrixZ, bool[] matrixMax, double[,] matrixLimits, bool equal)
            {
                StringBuilder sb = new();
                int rowsZ = matrixZ.GetLength(0);
                int colsZ = matrixZ.GetLength(1);

                int rowsL = matrixLimits.GetLength(0);
                int colsL = matrixLimits.GetLength(1);

                for (int i = 0; i < rowsZ; i++)
                {
                    sb.Append($"Z{i + 1} = ");
                    bool firstTerm = true;

                    for (int j = 0; j < colsZ; j++)
                    {
                        double val = matrixZ[i, j];
                        if (val == 0) continue;

                        if (val > 0)
                            sb.Append(firstTerm ? "" : " + ");
                        else
                            sb.Append(firstTerm ? "-" : " - ");

                        double absVal = Math.Abs(val);
                        if (absVal != 1)
                            sb.Append(absVal);

                        sb.Append($"x{j + 1}");
                        firstTerm = false;
                    }

                    sb.AppendLine(matrixMax[i] ? " -> max," : " -> min,");
                }

                sb.AppendLine("\n   при обмеженнях:");

                for (int i = 0; i < rowsL; i++)
                {
                    bool firstTerm = true;
                    for (int j = 0; j < colsL - 1; j++)
                    {
                        double val = matrixLimits[i, j];
                        if (val == 0) continue;

                        if (val > 0)
                            sb.Append(firstTerm ? "" : " + ");
                        else
                            sb.Append(firstTerm ? "-" : " - ");

                        double absVal = Math.Abs(val);
                        if (absVal != 1)
                            sb.Append(absVal);

                        sb.Append($"x{j + 1}");
                        firstTerm = false;
                    }

                    double rightSide = matrixLimits[i, colsL - 1];
                    if (equal)
                        sb.AppendLine($" = {rightSide};");
                    else
                    {
                        string sign = (rightSide >= 0) ? "-" : "+";
                        sb.AppendLine($" {sign} {Math.Abs(rightSide)} >= 0;");
                    }
                }

                return sb.ToString();
            }

            static double[,] GetSimplexTable(double[,] matrixZ, int zIndex, double[,] matrixLimits)
            {
                int rowsL = matrixLimits.GetLength(0);
                int colsL = matrixLimits.GetLength(1);

                double[,] table = new double[rowsL + 1, colsL];

                for (int i = 0; i < rowsL; i++)
                {
                    for (int j = 0; j < colsL; j++)
                    {
                        if (table[i, j] < 0)
                            table[i, j] = -matrixLimits[i, j];
                        else
                            table[i, j] = matrixLimits[i, j];
                    }

                }
                for (int j = 0; j < colsL - 1; j++)
                {
                    table[rowsL, j] = -matrixZ[zIndex, j];
                }
                table[rowsL, colsL - 1] = 0;

                return table;
            }

            static double[] SolveProblem(double[,] matrix, string[] xLabels, string[] yLabels, bool max, int index)
            {
                int originalSize = matrix.GetLength(1);

                // 1. Позбавляємося нульових міток
                bool hasZeroLabel = yLabels.Contains("0");
                if (hasZeroLabel)
                {
                    bool foundZeroLabel = false;
                    while (!foundZeroLabel)
                    {
                        foundZeroLabel = ProcessZeroLabelRow(ref matrix, ref xLabels, ref yLabels);
                        if (!foundZeroLabel)
                            DeleteZeroCols(ref matrix, ref xLabels);
                    }
                }

                bool foundBasicSolution = false;
                while (!foundBasicSolution)
                {
                    foundBasicSolution = FindBasicSolution(ref matrix, ref xLabels, ref yLabels);
                }

                double[] basicSolution = GetSolutionPoint(matrix, yLabels, originalSize);
                Console.WriteLine($"\nОпорний розв'язок знайдено. Симплекс-таблиця:");
                PrintMatrix(matrix, xLabels, yLabels);
                Console.WriteLine($"\nX = ({string.Join("; ", basicSolution.Select(val => val.ToString("F2")))})\n");

                bool foundOptimalSolution = false;
                while (!foundOptimalSolution)
                {
                    foundOptimalSolution = FindOptimalSolution(ref matrix, ref xLabels, ref yLabels);
                }

                double[] optimalSolution = GetSolutionPoint(matrix, yLabels, originalSize);
                Console.WriteLine($"\nОптимальний розв'язок знайдено. Симплекс-таблиця:");
                PrintMatrix(matrix, xLabels, yLabels);

                double zMax = matrix[matrix.GetLength(0) - 1, matrix.GetLength(1) - 1];

                if (max)
                    Console.WriteLine($"\nMax (Z{index + 1}) = {zMax:F2}");
                else
                    Console.WriteLine($"\nMin (Z{index + 1}) = {-zMax:F2}");

                return optimalSolution;
            }

            static bool ProcessZeroLabelRow(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels)
            {
                int numRows = matrix.GetLength(0);
                bool noZeroFound = true;
                int resultCol = 0;

                for (int i = 0; i < numRows - 1; i++)
                {
                    if (yLabels[i] == "0")
                    {
                        noZeroFound = false;
                        resultCol = FindFirstNegativeOrPositive(matrix, i, "find_zero");

                        if (resultCol != -1)
                        {
                            FindSmallestPositiveAndPivot(ref matrix, ref xLabels, ref yLabels, resultCol, i, "find_zero");
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

            static bool FindBasicSolution(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels)
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
                        resultCol = FindFirstNegativeOrPositive(matrix, i, "basic");

                        if (resultCol != -1)
                        {
                            FindSmallestPositiveAndPivot(ref matrix, ref xLabels, ref yLabels, resultCol, i, "basic");
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

            static bool FindOptimalSolution(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels)
            {
                int lastRowIndex = matrix.GetLength(0) - 1;
                int numCols = matrix.GetLength(1);
                bool noNegativeFound = true;

                for (int j = 0; j < numCols - 1; j++)
                {
                    if (matrix[lastRowIndex, j] < 0)
                    {
                        noNegativeFound = false;
                        FindSmallestPositiveAndPivot(ref matrix, ref xLabels, ref yLabels, j, -1, "optimal");
                        break;
                    }
                }

                return noNegativeFound;
            }

            static int FindFirstNegativeOrPositive(double[,] matrix, int rowIndex, string solutionType)
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
                    return -1;
                }
                return targetCol;
            }

            static void FindSmallestPositiveAndPivot(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, int colIndex, int rowIndexLastCol, string solutionType)
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
                    Environment.Exit(1);
                }

                PerformModifiedJordanElimination(ref matrix, ref xLabels, ref yLabels, smallestInRow, colIndex);
            }

            static void PerformModifiedJordanElimination(ref double[,] matrix, ref string[] xLabels, ref string[] yLabels, int inRow, int inCol)
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

                (yLabels[inRow], xLabels[inCol]) = (xLabels[inCol], yLabels[inRow]);
            }

            static void DeleteZeroCols(ref double[,] matrix, ref string[] xLabels)
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
            }

            static double[] GetSolutionPoint(double[,] matrix, string[] yLabels, int originalVarsCount)
            {
                int numRows = matrix.GetLength(0);
                int numCols = matrix.GetLength(1);

                double[] solutionArray = new double[originalVarsCount - 1];

                for (int i = 0; i < numRows - 1; i++)
                {
                    if (yLabels[i].StartsWith('x'))
                    {
                        string digitPart = yLabels[i][1..];
                        if (int.TryParse(digitPart, out int index))
                        {
                            if (index - 1 < solutionArray.Length)
                                solutionArray[index - 1] = matrix[i, numCols - 1];
                        }
                    }
                }

                return solutionArray;
            }

            static void PrintMatrix(double[,] matrix, string[] xLabels, string[] yLabels)
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
                }
            }

            static void PrintOptimalVectors(double[,] vectors, string[] l1, string[] l2)
            {
                foreach (string el in l1)
                    Console.Write("\t" + el);

                int rows = vectors.GetLength(0);
                int cols = vectors.GetLength(1);

                for (int i = 0; i < rows; i++)
                {
                    Console.Write($"\n{l2[i]}\t");

                    for (int j = 0; j < cols; j++)
                        Console.Write($"{vectors[i, j]:F2}\t");

                    Console.WriteLine();
                }
            }

            static double[,] CalculateRegretMatrix(double[,] matrixX, double[,] matrixC, bool[] isMax)
            {
                int n = matrixX.GetLength(0);
                double[,] regretMatrix = new double[n, n];
                double[] idealValues = new double[n];

                for (int i = 0; i < n; i++)
                    idealValues[i] = CalculateZValueScaled(matrixX, matrixC, i, i);

                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        double currentValue = CalculateZValueScaled(matrixX, matrixC, i, j);

                        double numerator = Math.Abs(currentValue - idealValues[j]);
                        double denominator = Math.Abs(idealValues[j]);

                        if (denominator > 1e-10)
                            regretMatrix[i, j] = Math.Round(numerator / denominator, 2);
                        else
                            regretMatrix[i, j] = 0;
                    }
                }

                return regretMatrix;
            }

            static double CalculateZValueScaled(double[,] matrixX, double[,] matrixC, int xRow, int cRow)
            {
                double z = 0;
                int cols = matrixX.GetLength(1);
                for (int j = 0; j < cols; j++)
                    z += (matrixX[xRow, j] / 100.0) * matrixC[cRow, j];

                return z;
            }

            static double AddMaxToAllElements(ref double[,] matrix)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                double maxElement = double.MinValue;

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (Math.Abs(matrix[i, j]) > maxElement)
                            maxElement = Math.Abs(matrix[i, j]);
                    }
                }

                Console.WriteLine($"\nmax = |-{maxElement:F2}| = {Math.Abs(maxElement):F2}");

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                        matrix[i, j] += maxElement;
                }
                return maxElement;
            }

            static string PrintPairMatrix(double[,] matrix, string[] t, string[] q, string[] p, string[] r)
            {
                string res = "";
                int cols = matrix.GetLength(1);

                res += "".PadRight(12);
                foreach (var n in t) res += $"{n.Trim(),10}";
                res += "\n" + "".PadRight(12);
                foreach (var n in q) res += $"{n.Trim(),10}";
                res += "\n" + new string('-', 12 + cols * 10) + "\n";

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    res += $"{(p[i].Trim() + " " + r[i].Trim()),-12}";

                    for (int j = 0; j < cols; j++)
                    {
                        res += $"{(Math.Abs(matrix[i, j]) < 1e-9 ? 0.0 : matrix[i, j]),10:F2}";
                    }
                    res += "\n";
                }

                return res;
            }

            static double SaddlePoint(double[,] matrix)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                for (int i = 0; i < rows; i++)
                {
                    //Знаходимо мінімум у рядку i
                    double rowMin = matrix[i, 0];
                    int colIndex = 0;
                    for (int j = 1; j < cols; j++)
                    {
                        if (matrix[i, j] < rowMin)
                        {
                            rowMin = matrix[i, j];
                            colIndex = j;
                        }
                    }

                    //Перевіряємо, чи є цей мінімум максимумом у стовпці colIndex
                    bool isSaddlePoint = true;
                    for (int k = 0; k < rows; k++)
                    {
                        if (matrix[k, colIndex] > rowMin)
                        {
                            isSaddlePoint = false;
                            break;
                        }
                    }

                    if (isSaddlePoint)
                        return colIndex;
                }
                return -1;
            }

            static void PrintFinalOptimalSolutions(double[,] matrix, string[] tL, string[] pL, int originalRows, int originalCols, double minDigit, ref double[] X0, ref double[] Y0)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                double zSum = matrix[rows - 1, cols - 1];
                double gameValue = 1.0 / zSum;

                int n = tL.Length - 1;
                int m = pL.Length - 1;

                double[] X = new double[n];
                double[] U = new double[m];

                for (int j = 0; j < n; j++)
                {
                    string target = "t" + (j + 1);
                    int foundRow = -1;
                    for (int i = 0; i < rows - 1; i++)
                        if (pL[i].Contains(target)) { foundRow = i; break; }

                    X[j] = (foundRow != -1) ? matrix[foundRow, cols - 1] : 0;
                }

                for (int i = 0; i < m; i++)
                {
                    string target = "p" + (i + 1);
                    int foundCol = -1;
                    for (int j = 0; j < cols - 1; j++)
                        if (tL[j].Contains(target)) { foundCol = j; break; }

                    U[i] = (foundCol != -1) ? matrix[rows - 1, foundCol] : 0;
                }

                // Вивід результату
                Console.WriteLine("\nОптимальні розв’язки:");
                Console.ResetColor();

                Console.WriteLine($"  U = ({string.Join("; ", U.Select(v => (Math.Abs(v) < 1e-9 ? 0.0 : v).ToString("F2")))});");
                Console.WriteLine($"  X = ({string.Join("; ", X.Select(v => (Math.Abs(v) < 1e-9 ? 0.0 : v).ToString("F2")))});");
                Console.WriteLine($"  Max (Z) = Min (W) = {zSum:F2}.\n");

                X0 = U.Select(v => Math.Abs(v) < 1e-9 ? 0.0 : v / zSum).ToArray();
                Y0 = X.Select(v => Math.Abs(v) < 1e-9 ? 0.0 : v / zSum).ToArray();

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Змішана стратегія 1-го гравця:");
                Console.ResetColor();
                Console.WriteLine($"  X0 = ({string.Join("; ", U.Select(v => (Math.Abs(v) < 1e-9 ? 0.0 : (v / zSum)).ToString("F2")))});\n");

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("Змішана стратегія 2-го гравця:");
                Console.ResetColor();
                Console.WriteLine($"  Y0 = ({string.Join("; ", X.Select(v => (Math.Abs(v) < 1e-9 ? 0.0 : (v / zSum)).ToString("F2")))});\n");

                // Повертаємо ціну гри до початкового стану (віднімаємо Abs(minDigit))
                double finalPrice = gameValue;

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine($"Ціна гри: {finalPrice:F2}.\n");
                Console.ResetColor();
            }

            static void SolveProblemPair(double[,] matrix, string[] tL, string[] qL, string[] pL, string[] rL, int taskType, double minDigit, ref double[] X0, ref double[] Y0)
            {
                Console.WriteLine("\nПочаткова симплекс-таблиця:");
                Console.WriteLine(PrintPairMatrix(matrix, tL, qL, pL, rL));

                // Пошук опорного розв'язку
                bool foundBasic = false;
                while (!foundBasic)
                {
                    foundBasic = FindBasicSolutionPair(ref matrix, ref tL, ref qL, ref pL, ref rL);
                }

                // Пошук оптимального розв'язку
                bool foundOptimal = false;
                while (!foundOptimal)
                {
                    foundOptimal = FindOptimalSolutionPair(ref matrix, ref tL, ref qL, ref pL, ref rL);
                }

                Console.ForegroundColor = ConsoleColor.DarkGreen;
                int originalRows = matrix.GetLength(0) - 1;
                int originalCols = matrix.GetLength(1) - 1;

                PrintFinalOptimalSolutions(matrix, tL, pL, originalRows, originalCols, minDigit, ref X0, ref Y0);
            }

            static bool FindBasicSolutionPair(ref double[,] matrix, ref string[] t, ref string[] q, ref string[] p, ref string[] r)
            {
                int numRows = matrix.GetLength(0);
                int lastCol = matrix.GetLength(1) - 1;

                for (int i = 0; i < numRows - 1; i++)
                {
                    if (matrix[i, lastCol] < 0)
                    {
                        int targetCol = -1;
                        for (int j = 0; j < lastCol; j++)
                        {
                            if (matrix[i, j] < 0) { targetCol = j; break; }
                        }

                        if (targetCol != -1)
                        {
                            FindPivotAndEliminate(ref matrix, ref t, ref q, ref p, ref r, targetCol);
                            return false;
                        }
                    }
                }
                return true;
            }

            static bool FindOptimalSolutionPair(ref double[,] matrix, ref string[] t, ref string[] q, ref string[] p, ref string[] r)
            {
                int lastRow = matrix.GetLength(0) - 1;
                int lastCol = matrix.GetLength(1) - 1;

                for (int j = 0; j < lastCol; j++)
                {
                    if (matrix[lastRow, j] < 0)
                    {
                        FindPivotAndEliminate(ref matrix, ref t, ref q, ref p, ref r, j);
                        return false;
                    }
                }
                return true;
            }

            static void FindPivotAndEliminate(ref double[,] matrix, ref string[] t, ref string[] q, ref string[] p, ref string[] r, int col)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                int pivotRow = -1;
                double minRatio = double.MaxValue;

                for (int i = 0; i < rows - 1; i++)
                {
                    if (matrix[i, col] > 0)
                    {
                        double ratio = matrix[i, cols - 1] / matrix[i, col];
                        if (ratio < minRatio) { minRatio = ratio; pivotRow = i; }
                    }
                }

                if (pivotRow == -1) { WriteError("Задача не має розв'язку"); Environment.Exit(1); }

                // Виконуємо Жорданове виключення
                double pivot = matrix[pivotRow, col];
                double[,] next = new double[rows, cols];

                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        if (i == pivotRow && j == col) next[i, j] = 1 / pivot;
                        else if (i == pivotRow) next[i, j] = matrix[i, j] / pivot;
                        else if (j == col) next[i, j] = -matrix[i, j] / pivot;
                        else next[i, j] = matrix[i, j] - (matrix[pivotRow, j] * matrix[i, col] / pivot);

                        next[i, j] = Math.Round(next[i, j], 4);
                    }
                }
                matrix = next;

                string tempT = t[col]; t[col] = p[pivotRow]; p[pivotRow] = tempT;
                string tempQ = q[col]; q[col] = r[pivotRow]; r[pivotRow] = tempQ;

                Console.WriteLine($"Крок: Ведучий елемент [{pivotRow + 1},{col + 1}] = {pivot:F2}");
                Console.WriteLine(PrintPairMatrix(matrix, t, q, p, r));
            }

            static double[] XCompromis(double[,] matrix, double[] k)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                double[] result = new double[cols];

                for (int j = 0; j < cols; j++)
                {
                    for (int i = 0; i < rows; i++)
                        result[j] += matrix[i, j] * k[i];
                }

                return result;
            }

            static void PrintResult(double[] X, string[] l1)
            {
                Console.Write("\t");

                foreach (string el in l1)
                    Console.Write("\t" + el);

                int l = X.Length;
                Console.Write($"\nX*(комп)\t");

                for (int j = 0; j < l; j++)
                    Console.Write($"{X[j]:F2}\t");

                Console.WriteLine();
            }
        }
    }
}


