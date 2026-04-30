using System.Text;

namespace Lab_4
{
    class Program
    {
        // Додано: об'єкт для накопичення проміжних обчислень у фоновому режимі
        static StringBuilder protocol = new StringBuilder();

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

                string matrixStr = PrintMatrix(matrix!, NLabels, ALabels);
                Console.WriteLine("\nВхідна матриця:\n" + matrixStr + "\nРезультат аналізу:");

                // Додано: Записуємо базову інформацію в протокол
                protocol.AppendLine($"\n\n--- АНАЛІЗ НОВОЇ МАТРИЦІ ({DateTime.Now}) ---");
                protocol.AppendLine("Вхідна матриця:\n" + matrixStr);

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

                // Додано: Фінальні результати у протокол і збереження у файл
                protocol.AppendLine("\n--- ПІДСУМКОВІ РЕЗУЛЬТАТИ ---");
                protocol.AppendLine("Критерій Вальда: " + strWald);
                protocol.AppendLine("Критерій максимаксу: " + strMaxMax);
                protocol.AppendLine($"Критерій Гурвіца (при y = {y}): " + strHurwitz);
                protocol.AppendLine("Критерій Севіджа: " + strSavage);
                protocol.AppendLine($"Критерій Байєса ({PMatrixString(p)}): " + strBayes);
                protocol.AppendLine("Критерій Лапласа: " + strLaplace);
                protocol.AppendLine(new string('-', 50));

                File.AppendAllText(logFilePath, protocol.ToString(), Encoding.UTF8);
                protocol.Clear(); // Очищаємо протокол для наступної ітерації
            }
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
                str += "p" + (i + 1).ToString() + " = " + P[i] + (i == P.Length - 1 ? "" : ", ");

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
            protocol.AppendLine("\n--- 1: Критерій Вальда ---");
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
                protocol.AppendLine($"Мінімум для A{i + 1}: {min:F2}");
            }

            double maxOfMins = minArray[0];
            for (int i = 1; i < rows; i++)
            {
                if (minArray[i] > maxOfMins)
                {
                    maxOfMins = minArray[i];
                }
            }
            protocol.AppendLine($"Максимум з мінімумів: {maxOfMins:F2}");

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
            protocol.AppendLine("\n--- 2: Критерій максимаксу ---");
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
                protocol.AppendLine($"Максимум для A{i + 1}: {max:F2}");
            }

            double maxOfMaxes = maxArray[0];
            for (int i = 1; i < rows; i++)
            {
                if (maxArray[i] > maxOfMaxes)
                {
                    maxOfMaxes = maxArray[i];
                }
            }
            protocol.AppendLine($"Максимум з максимумів: {maxOfMaxes:F2}");

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
            protocol.AppendLine($"\n--- 3: Критерій Гурвіца (y = {y}) ---");
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
                protocol.AppendLine($"Значення для A{i + 1}: {y} * {min:F2} + (1 - {y}) * {max:F2} = {s[i]:F2}");
            }

            double maxS = s[0];
            for (int i = 1; i < rows; i++)
            {
                if (s[i] > maxS)
                    maxS = s[i];
            }
            protocol.AppendLine($"Максимальне значення за Гурвіцом: {maxS:F2}");

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
            protocol.AppendLine("\n--- 4: Критерій Севіджа ---");
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

            protocol.AppendLine("Матриця ризиків R:");
            string[] NLabels = new string[cols];
            string[] ALabels = new string[rows];
            for (int i = 0; i < cols; i++) NLabels[i] = "N" + (i + 1);
            for (int i = 0; i < rows; i++) ALabels[i] = "A" + (i + 1);
            protocol.AppendLine(PrintMatrix(R, NLabels, ALabels));

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
                protocol.AppendLine($"Максимальний ризик для A{i + 1}: {maxInRow:F2}");
            }

            double minOfMaxR = maxR[0];
            for (int i = 1; i < rows; i++)
            {
                if (maxR[i] < minOfMaxR)
                    minOfMaxR = maxR[i];
            }
            protocol.AppendLine($"Мінімальний з максимальних ризиків: {minOfMaxR:F2}");

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
            protocol.AppendLine("\n--- 5: Критерій Байєса ---");
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[] expectedValues = new double[rows];

            for (int i = 0; i < rows; i++)
            {
                double currentMatrix = 0;
                string formula = "";
                for (int j = 0; j < cols; j++)
                {
                    currentMatrix += P[j] * matrix[i, j];
                    formula += $"{P[j]} * {matrix[i, j]:F2}" + (j == cols - 1 ? "" : " + ");
                }

                expectedValues[i] = currentMatrix;
                protocol.AppendLine($"Математичне сподівання для A{i + 1}: {formula} = {currentMatrix:F2}");
            }

            double maxMatrix = expectedValues[0];
            for (int i = 1; i < rows; i++)
            {
                if (expectedValues[i] > maxMatrix)
                    maxMatrix = expectedValues[i];
            }
            protocol.AppendLine($"Максимальне сподівання: {maxMatrix:F2}");

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
            protocol.AppendLine("\n--- 6: Критерій Лапласа ---");
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            double[] averageValues = new double[rows];

            for (int i = 0; i < rows; i++)
            {
                double sum = 0;
                for (int j = 0; j < cols; j++)
                    sum += matrix[i, j];

                averageValues[i] = sum / cols;
                protocol.AppendLine($"Середнє значення для A{i + 1}: {sum:F2} / {cols} = {averageValues[i]:F2}");
            }

            double maxU = averageValues[0];
            for (int i = 1; i < rows; i++)
            {
                if (averageValues[i] > maxU)
                    maxU = averageValues[i];
            }
            protocol.AppendLine($"Максимальне середнє значення: {maxU:F2}");

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