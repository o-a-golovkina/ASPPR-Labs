using System.Text;

namespace Lab_4
{
    class Program
    {
        static void Main()
        {
            //Input data
            double[,] testMatrix1 = new double[3, 4]
            {
                {-1, 1, 1, 4},
                {-1, -2, 2, 3},
                {3, -1, 3, 2}
            };

            double[,] testMatrix2 = new double[3, 4]
            {
                {2, -1, 3, 4},
                {-1, 2, 3, 7},
                {5, 4, 6, 2}
            };

            double[,] myMatrix = new double[3, 4]
            {
                {1, 2, 1, 6},
                {-1, 3, 1, 6},
                {-2, 2, -1, 1}
            };

            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            string logFilePath = "protocol.txt";
            File.WriteAllText(logFilePath, $"--- ПРОТОКОЛ ({DateTime.Now}) ---\n", Encoding.UTF8);

            while (true)
            {
                double[,] matrix = null!;

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
                        matrix = testMatrix1;
                        break;

                    case 1:
                        matrix = testMatrix2;
                        break;

                    case 2:
                        matrix = myMatrix;
                        break;
                }

                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                Console.Write("Введіть значення y => ");
                double y = double.Parse(Console.ReadLine()!);

                Console.Write($"Введіть значення p (через пробіл {cols} числа) => ");
                double[] p = Console.ReadLine()!
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(double.Parse)
                    .ToArray();

                string[] NLabels = new string[cols];
                string[] ALabels = new string[rows];
                for (int i = 0; i < cols; i++) { NLabels[i] = "N" + (i + 1); }
                for (int i = 0; i < rows; i++) { ALabels[i] = "A" + (i + 1); }

                Console.WriteLine("\nВхідна матриця:\n" + PrintMatrix(matrix!, NLabels, ALabels) + "\nРезультат аналізу:");
                int[] waldRes = Wald(matrix);
                int[] maxMaxRes = MaxMax(matrix);
                int[] hurwitzRes = Hurwitz(matrix, y);
                int[] bayesRes = Bayes(matrix, p);
                int[] savageRes = Savage(matrix);
                int[] laplaceRes = Laplace(matrix);

                string strWald = FormatResult(waldRes);
                string strMaxMax = FormatResult(maxMaxRes);
                string strHurwitz = FormatResult(hurwitzRes);
                string strBayes = FormatResult(bayesRes);
                string strSavage = FormatResult(savageRes);
                string strLaplace = FormatResult(laplaceRes);

                Console.WriteLine("Критерій Вальда: " + strWald);
                Console.WriteLine("Критерій максимаксу: " + strMaxMax);
                Console.WriteLine($"Критерій Гурвіца (при y = {y}): " + strHurwitz);
                Console.WriteLine("Критерій Севіджа: " + strSavage);
                Console.WriteLine($"Критерій Байєса ({PMatrixString(p)}): " + strBayes);
                Console.WriteLine("Критерій Лапласа: " + strLaplace + "\n");

            }

            static string FormatResult(int[] res)
            {
                string prefix = res.Length == 1 ? "стратегія" : "стратегії";
                string strategiesNames = string.Join(" або ", ResultString(res));

                return $"{prefix} {strategiesNames}";
            }

            static string PrintMatrix(double[,] matrix, string[] N, string[] A)
            {
                string res = "";
                int cols = matrix.GetLength(1);

                res += "".PadRight(12);

                foreach (var n in N)
                    res += $"{n.Trim(),10}";
                res += "\n";

                for (int i = 0; i < matrix.GetLength(0); i++)
                {
                    res += $"{A[i].Trim(),-12}";

                    for (int j = 0; j < cols; j++)
                        res += $"{(Math.Abs(matrix[i, j]) < 1e-9 ? 0.0 : matrix[i, j]),10:F2}";

                    res += "\n";
                }

                return res;
            }

            static string PMatrixString(double[] P)
            {
                string str = "";
                for (int i = 0; i < P.Length; i++)
                    str += "p" + (i + 1).ToString() + " = " + P[i] + ", ";

                return str;
            }

            static string[] ResultString(int[] result)
            {
                string[] str = new string[result.Length];
                for (int i = 0; i < result.Length; i++)
                    str[i] += "A" + (result[i] + 1).ToString();

                return str;
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

            static int[] Wald(double[,] matrix)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                int[] index = new int[rows];
                double min = 0;
                double[] minArray = new double[rows];

                for (int i = 0; i < rows; i++)
                {
                    min = matrix[i, 0];
                    for (int j = 0; j < cols; j++)
                    {
                        if (matrix[i, j] < min)
                            min = matrix[i, j];
                    }
                    minArray[i] = min;
                }

                double maxOfMins = minArray[0];
                for (int i = 1; i < rows; i++)
                {
                    if (minArray[i] > maxOfMins)
                    {
                        maxOfMins = minArray[i];
                    }
                }

                List<int> indexList = new List<int>();
                for (int i = 0; i < minArray.Length; i++)
                {
                    if (Math.Abs(minArray[i] - maxOfMins) < 1e-9)
                        indexList.Add(i);
                }

                return indexList.ToArray();
            }

            static int[] MaxMax(double[,] matrix)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                int[] index = new int[rows];
                double max = 0;
                double[] maxArray = new double[rows];

                for (int i = 0; i < rows; i++)
                {
                    max = matrix[i, 0];
                    for (int j = 0; j < cols; j++)
                    {
                        if (matrix[i, j] > max)
                            max = matrix[i, j];
                    }
                    maxArray[i] = max;
                }

                double maxOfMaxes = maxArray[0];
                for (int i = 1; i < rows; i++)
                {
                    if (maxArray[i] > maxOfMaxes)
                    {
                        maxOfMaxes = maxArray[i];
                    }
                }

                List<int> indexList = new List<int>();
                for (int i = 0; i < maxArray.Length; i++)
                {
                    if (Math.Abs(maxArray[i] - maxOfMaxes) < 1e-9)
                    {
                        indexList.Add(i);
                    }
                }

                return indexList.ToArray();
            }

            static int[] Hurwitz(double[,] matrix, double y)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                double[] s = new double[rows];

                for (int i = 0; i < rows; i++)
                {
                    double min = matrix[i, 0];
                    double max = matrix[i, 0];

                    for (int j = 1; j < cols; j++)
                    {
                        if (matrix[i, j] < min) min = matrix[i, j];
                        if (matrix[i, j] > max) max = matrix[i, j];
                    }

                    s[i] = y * min + (1 - y) * max;
                }

                double maxS = s[0];
                for (int i = 1; i < rows; i++)
                {
                    if (s[i] > maxS)
                        maxS = s[i];
                }

                List<int> indexList = new List<int>();
                for (int i = 0; i < rows; i++)
                {
                    if (Math.Abs(s[i] - maxS) < 1e-9)
                        indexList.Add(i);
                }

                return indexList.ToArray();
            }

            static int[] Savage(double[,] matrix)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);

                double[,] R = new double[rows, cols];
                for (int j = 0; j < cols; j++)
                {
                    double maxInColumn = matrix[0, j];
                    for (int i = 1; i < rows; i++)
                    {
                        if (matrix[i, j] > maxInColumn)
                            maxInColumn = matrix[i, j];
                    }

                    for (int i = 0; i < rows; i++)
                        R[i, j] = maxInColumn - matrix[i, j];
                }

                double[] maxR = new double[rows];
                for (int i = 0; i < rows; i++)
                {
                    double maxInRow = R[i, 0];
                    for (int j = 1; j < cols; j++)
                    {
                        if (R[i, j] > maxInRow)
                            maxInRow = R[i, j];
                    }
                    maxR[i] = maxInRow;
                }

                double minOfMaxR = maxR[0];
                for (int i = 1; i < rows; i++)
                {
                    if (maxR[i] < minOfMaxR)
                        minOfMaxR = maxR[i];
                }

                List<int> indexList = new List<int>();
                for (int i = 0; i < rows; i++)
                {
                    if (Math.Abs(maxR[i] - minOfMaxR) < 1e-9)
                        indexList.Add(i);
                }

                return indexList.ToArray();
            }

            static int[] Bayes(double[,] matrix, double[] P)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                double[] expectedValues = new double[rows];

                for (int i = 0; i < rows; i++)
                {
                    double currentMatrix = 0;
                    for (int j = 0; j < cols; j++)
                        currentMatrix += P[j] * matrix[i, j];

                    expectedValues[i] = currentMatrix;
                }

                double maxMatrix = expectedValues[0];
                for (int i = 1; i < rows; i++)
                {
                    if (expectedValues[i] > maxMatrix)
                        maxMatrix = expectedValues[i];
                }

                List<int> indexList = new List<int>();
                for (int i = 0; i < rows; i++)
                {
                    if (Math.Abs(expectedValues[i] - maxMatrix) < 1e-9)
                        indexList.Add(i);
                }

                return indexList.ToArray();
            }

            static int[] Laplace(double[,] matrix)
            {
                int rows = matrix.GetLength(0);
                int cols = matrix.GetLength(1);
                double[] averageValues = new double[rows];

                for (int i = 0; i < rows; i++)
                {
                    double sum = 0;
                    for (int j = 0; j < cols; j++)
                        sum += matrix[i, j];

                    averageValues[i] = sum / cols;
                }

                double maxU = averageValues[0];
                for (int i = 1; i < rows; i++)
                {
                    if (averageValues[i] > maxU)
                        maxU = averageValues[i];
                }

                List<int> indexList = new List<int>();
                for (int i = 0; i < rows; i++)
                {
                    if (Math.Abs(averageValues[i] - maxU) < 1e-9)
                        indexList.Add(i);
                }

                return indexList.ToArray();
            }
        }
    }
}