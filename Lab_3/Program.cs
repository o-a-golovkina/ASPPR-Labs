using System.Text;

namespace Lab_3
{
    class Program
    {
        static void Main()
        {
            //Input data
            double[,] testMatrix1 = new double[3, 3]
            {
                {5, 2, 7},
                {1, 4, 3},
                {6, 1, 5}
            };

            double[,] testMatrix2 = new double[3, 4]
            {
                {2, -1, 3, 3},
                {-1, 2, 2, 7},
                {1, 1, 1, 2}
            };

            double[,] matrixA1 = new double[3, 3]
            {
                {-2, -1, -2},
                {4, -2, 1},
                {1, 3, -5}
            };

            double[,] matrixA2 = new double[2, 2]
            {
                {3, 8},
                {5, 2}
            };

            double[,] matrixA3 = new double[4, 2]
            {
                {8, 5},
                {10, 2},
                {3, 9},
                {4, 12}
            };

            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            while (true)
            {
                double minDigit = 0;
                double[,] matrix = null!;

                Console.Write("Оберіть матрицю:\n" +
                                  " 0) Тестова матриця №1\n" +
                                  " 1) Тестова матриця №2\n" +
                                  " 2) A1\n" +
                                  " 3) A2\n" +
                                  " 4) A3\n" +
                                  "Відповідь => ");

                int choice = GetValidMenuChoice(0, 4);
                Console.WriteLine();

                switch (choice)
                {
                    case 0:
                        matrix = testMatrix1;
                        break;

                    case 1:
                        matrix = testMatrix2;
                        break;

                    case 2:
                        matrix = matrixA1;
                        break;

                    case 3:
                        matrix = matrixA2;
                        break;

                    case 4:
                        matrix = matrixA3;
                        break;
                }
                Console.WriteLine("Вхідна матриця:\n" + PrintMatrix(matrix!));

                matrix = CheckMatrix(ref minDigit, matrix);
                if (minDigit == 0)
                    Console.WriteLine("Всі елементи матриці додатні");
                else
                {
                    Console.WriteLine($"У матриці наявні від'ємні елементи\n" +
                                      $"Найменший елемент: {minDigit:F2}\n" +
                                      $"Матриця після перетворення:\n" +
                                      PrintMatrix(matrix));
                }

                if (SaddlePoint(matrix) != -1.0)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"Сідлова точка знайдена: {SaddlePoint(matrix) - Math.Abs(minDigit)}\n");
                    Console.ResetColor();
                    continue;
                }
                Console.WriteLine($"Сідлова точка не знайдена\n" +
                                  $"Шукаємо розв'язок шляхом розв'язання пари взаємно двоїстих ЗЛП:");

                int rows = matrix.GetLength(0) + 1;
                int cols = matrix.GetLength(1) + 1;
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
                            newMatrix[i, j] = matrix[i, j];
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

                SolveProblem(newMatrix, tLabels, qLabels, pLabels, rLabels, 1, minDigit);

                if (choice == 2)
                {
                    Console.WriteLine("Моделювання результатів розв'язання матричної гри А1");
                    ModelingSolution(matrixA1);

                }

                if (choice == 3)
                {
                    Console.WriteLine("Розв'язання матричної гри А2 аналітичним способом");
                    AnalyticSolution(matrixA2);

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
            Console.Write("Помилка: ");
            Console.ResetColor();
            Console.Write(ex);
        }

        static double[,] CheckMatrix(ref double min, double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    if (matrix[i, j] < min)
                        min = matrix[i, j];
                }
            }
            if (min == 0)
                return matrix;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                    matrix[i, j] += Math.Abs(min);
            }
            return matrix;
        }

        static string PrintMatrix(double[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            string res = "";
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    res += matrix[i, j].ToString("F2");
                    res += "\t";
                }
                res += "\n";
            }

            return res;
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

        static void ModelingSolution(double[,] A1)
        {
            double[] X_theory = { 0.17, 0.17, 0.67 }; // Гравець А
            double[] Y_theory = { 0.33, 0.33, 0.33 }; // Гравець В

            int nParties = 20;

            Random rand = new Random();
            double totalPayoff = 0;
            int[] countA = new int[X_theory.Length];
            int[] countB = new int[Y_theory.Length];

            Console.WriteLine("\n" + new string('-', 110));
            Console.WriteLine($"{"№",-4} | {"Вип. А",-8} | {"Стр. А",-6} | {"Вип. В",-8} | {"Стр. В",-6} | {"Виграш",-8} | {"Накопич.",-8} | {"Середній",-10}");
            Console.WriteLine(new string('-', 110));

            for (int k = 1; k <= nParties; k++)
            {
                // Вибір стратегії для гравця А
                double rA = rand.NextDouble();
                int stratA = GetStrategy(rA, X_theory);
                countA[stratA]++;

                // Вибір стратегії для гравця В
                double rB = rand.NextDouble();
                int stratB = GetStrategy(rB, Y_theory);
                countB[stratB]++;

                // Виграш А в цій партії
                double payoff = A1[stratA, stratB];
                totalPayoff += payoff;
                double averagePayoff = totalPayoff / k;

                // Вивід рядка протоколу
                Console.WriteLine($"{k,-4} | {rA,8:F4} | {stratA + 1,-6} | {rB,8:F4} | {stratB + 1,-6} | {payoff,8:F2} | {totalPayoff,8:F2} | {averagePayoff,10:F4}");
            }
            Console.WriteLine(new string('-', 110));

            // Експериментальні стратегії
            Console.WriteLine("\nЕкспериментальні змішані стратегії:");
            Console.Write("X* = (");
            for (int i = 0; i < countA.Length; i++) Console.Write($"{(double)countA[i] / nParties:F2}" + (i == countA.Length - 1 ? "" : "; "));
            Console.WriteLine(")");

            Console.Write("Y* = (");
            for (int i = 0; i < countB.Length; i++) Console.Write($"{(double)countB[i] / nParties:F2}" + (i == countB.Length - 1 ? "" : "; "));
            Console.WriteLine(")\n");
        }

        // Допоміжний метод для вибору стратегії
        static int GetStrategy(double randomVal, double[] probabilities)
        {
            double cumulative = 0;
            for (int i = 0; i < probabilities.Length; i++)
            {
                cumulative += probabilities[i];
                if (randomVal < cumulative) return i;
            }
            return probabilities.Length - 1;
        }

        static void AnalyticSolution(double[,] matrix)
        {
            double a11 = matrix[0, 0];
            double a12 = matrix[0, 1];
            double a21 = matrix[1, 0];
            double a22 = matrix[1, 1];

            // 1. Розрахунок нижньої та верхньої ціни гри (max min / min max)
            double alpha = Math.Max(Math.Min(a11, a12), Math.Min(a21, a22));
            double beta = Math.Min(Math.Max(a11, a21), Math.Max(a12, a22));

            Console.WriteLine($"max min = {alpha}");
            Console.WriteLine($"min max = {beta}");

            // 2. Вивід системи рівнянь як на картинці
            Console.WriteLine("\nПобудуємо систему лінійних рівнянь для 1-го гравця:");
            Console.WriteLine("{");
            Console.WriteLine($"  {a11}x1 + {a21}x2 = v");
            Console.WriteLine($"  {a12}x1 + {a22}x2 = v");
            Console.WriteLine("  x1 + x2 = 1");
            Console.WriteLine("}");

            // 3. Математичний розв'язок системи
            // Формула виведена з віднімання рівнянь: x1(a11 - a12) + x2(a21 - a22) = 0
            double denominator = (a11 + a22) - (a12 + a21);

            if (Math.Abs(denominator) < 1e-9)
            {
                Console.WriteLine("\nЗнаменник дорівнює 0. Система не має одного чіткого розв'язку.");
                return;
            }

            double x1 = (a22 - a21) / denominator;
            double x2 = 1 - x1;
            double v = a11 * x1 + a21 * x2;

            Console.WriteLine("\nРезультат розв'язання системи:");
            Console.WriteLine($"x1 = {x1:F4}");
            Console.WriteLine($"x2 = {x2:F4}");
            Console.WriteLine($"v = {v:F4}");

            Console.WriteLine("\nПобудуємо систему лінійних рівнянь для 2-го гравця:");
            Console.WriteLine("{");
            Console.WriteLine($"  {a11}y1 + {a12}y2 = v");
            Console.WriteLine($"  {a21}y1 + {a22}y2 = v");
            Console.WriteLine("  y1 + y2 = 1");
            Console.WriteLine("}");

            // 2. Розрахунок
            denominator = (a11 + a22) - (a12 + a21);

            if (Math.Abs(denominator) < 1e-9)
            {
                Console.WriteLine("Система не має однозначного розв'язку.");
                return;
            }

            // Формули для y1 та y2
            double y1 = (a22 - a12) / denominator;
            double y2 = 1 - y1;
            v = a11 * y1 + a12 * y2;

            // 3. Вивід результату
            Console.WriteLine("\nРезультат розв'язання системи для 2-го гравця:");
            Console.WriteLine($"y1 = {y1:F4}");
            Console.WriteLine($"y2 = {y2:F4}");
            Console.WriteLine($"v = {v:F4}");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\nОптимальнs стратегії гравців:");
            Console.ResetColor();
            Console.WriteLine($"  X = ({x1:F2}; {x2:F2})");
            Console.WriteLine($"  Y = ({y1:F2}; {y2:F2})");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"\nЦіна гри v = {v:F2}\n");
            Console.ResetColor();
        }

        static void PrintFinalOptimalSolutions(double[,] matrix, string[] tL, string[] pL, int originalRows, int originalCols, double minDigit)
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
            Console.WriteLine($"  X = ({string.Join("; ", X.Select(v => (Math.Abs(v) < 1e-9 ? 0.0 : v).ToString("F2")))});");
            Console.WriteLine($"  U = ({string.Join("; ", U.Select(v => (Math.Abs(v) < 1e-9 ? 0.0 : v).ToString("F2")))});");
            Console.WriteLine($"  Max (Z) = Min (W) = {zSum:F2}.\n");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Змішана стратегія 1-го гравця:");
            Console.ResetColor();
            Console.WriteLine($"  X0 = ({string.Join("; ", U.Select(v => (Math.Abs(v) < 1e-9 ? 0.0 : (v / zSum)).ToString("F2")))});\n");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("Змішана стратегія 2-го гравця:");
            Console.ResetColor();
            Console.WriteLine($"  Y0 = ({string.Join("; ", X.Select(v => (Math.Abs(v) < 1e-9 ? 0.0 : (v / zSum)).ToString("F2")))});\n");

            // Повертаємо ціну гри до початкового стану (віднімаємо Abs(minDigit))
            double finalPrice = gameValue - Math.Abs(minDigit);

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"Ціна гри: {finalPrice:F2}.\n");
            Console.ResetColor();
        }

        static void SolveProblem(double[,] matrix, string[] tL, string[] qL, string[] pL, string[] rL, int taskType, double minDigit)
        {
            Console.WriteLine("\nПочаткова симплекс-таблиця:");
            Console.WriteLine(PrintPairMatrix(matrix, tL, qL, pL, rL));

            // Пошук опорного розв'язку
            bool foundBasic = false;
            while (!foundBasic)
            {
                foundBasic = FindBasicSolution(ref matrix, ref tL, ref qL, ref pL, ref rL);
            }

            // Пошук оптимального розв'язку
            bool foundOptimal = false;
            while (!foundOptimal)
            {
                foundOptimal = FindOptimalSolution(ref matrix, ref tL, ref qL, ref pL, ref rL);
            }

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            int originalRows = matrix.GetLength(0) - 1;
            int originalCols = matrix.GetLength(1) - 1;

            PrintFinalOptimalSolutions(matrix, tL, pL, originalRows, originalCols, minDigit);
        }

        static bool FindBasicSolution(ref double[,] matrix, ref string[] t, ref string[] q, ref string[] p, ref string[] r)
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

        static bool FindOptimalSolution(ref double[,] matrix, ref string[] t, ref string[] q, ref string[] p, ref string[] r)
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

            // ОБМІН ВАШИХ МІТОК
            string tempT = t[col]; t[col] = p[pivotRow]; p[pivotRow] = tempT;
            string tempQ = q[col]; q[col] = r[pivotRow]; r[pivotRow] = tempQ;

            Console.WriteLine($"Крок: Ведучий елемент [{pivotRow + 1},{col + 1}] = {pivot:F2}");
            Console.WriteLine(PrintPairMatrix(matrix, t, q, p, r));
        }
    }
}