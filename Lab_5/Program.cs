using System.Text;

namespace Lab_5
{
    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            //Input data for TEST1
            int[,] test1SP = //prices (small squares)
            {
                {6, 3, 2},
                {2, 1, 5},
                {3, 4, 1}
            };
            int[] test1PO = [30, 20, 50]; //stocks (A)
            int[] test1PN = [10, 65, 25]; //applications (B)

            //Input data for TEST2
            int[,] test2SP = //prices (small squares)
            {
                {7, 6, 4},
                {3, 8, 5},
                {2, 3, 7}
            };
            int[] test2PO = [120, 100, 80]; //stocks (A)
            int[] test2PN = [90, 90, 120]; //applications (B)

            //Input data for VARIANT2
            int[,] MySP = //prices (small squares)
            {
                {5, 7, 8, 9},
                {6, 2, 4, 10},
                {4, 3, 3, 4}
            };
            int[] MyPO = [130, 80, 195]; //stocks (A)
            int[] MyPN = [115, 90, 125, 75]; //applications (B)

            int[,] SP = null!;
            int[] PO = null!;
            int[] PN = null!;

            while (true)
            {
                Console.Write("Оберіть матрицю:\n" +
                                  " 0) Тестові дані №1\n" +
                                  " 1) Тестові дані №2\n" +
                                  " 2) Дані з варіанту №2\n" +
                                  "Відповідь => ");

                int choice = GetValidMenuChoice(0, 2);
                Console.WriteLine();

                switch (choice)
                {
                    case 0:
                        SP = test1SP;
                        PO = test1PO;
                        PN = test1PN;
                        break;

                    case 1:
                        SP = test2SP;
                        PO = test2PO;
                        PN = test2PN;
                        break;

                    case 2:
                        SP = MySP;
                        PO = MyPO;
                        PN = MyPN;
                        break;
                }

                int rows = SP.GetLength(0);
                int cols = SP.GetLength(1);

                int[] tempPO = (int[])PO.Clone();
                int[] tempPN = (int[])PN.Clone();

                int[,] R = new int[rows, cols];

                Console.WriteLine("ВХІДНА ТАБЛИЦЯ:");
                PrintTable(SP, R, PN, PO);

                Console.WriteLine("\nОпорний план перевезень, знайдений методом північно-західного кута:");
                R = NorthwestCornerMethod(SP, tempPO, tempPN);

                PrintTable(SP, R, PN, PO);

                Console.WriteLine("\nВартість перевезень за опорним планом:");
                int sum = TransportationCost(SP, R);

                Console.WriteLine("Пошук оптимального плану перевезень методом потенціалів:");

                CalculatePotentials(SP, R, out int?[] PA, out int?[] PB);

                Console.Write("\nПотенціали постачальників (А):");
                foreach (int p in PA)
                    Console.Write("\t" + p);

                Console.Write("\nПотенціали споживачів (В):");
                foreach (int p in PB)
                    Console.Write("\t" + p);

                Console.WriteLine("\nНепрямі вартості:");
                int?[,] IC = IndirectCosts(SP, R, PA, PB);

                PrintTable(SP, R, PN, PO, IC);

                bool repeat = true;
                while (repeat)
                {
                    CalculatePotentials(SP, R, out PA, out PB);

                    if (CheckOptimum(SP, PA, PB, R, out int nextRow, out int nextCol))
                    {
                        Console.WriteLine("\nУмова оптимальності виконується.");
                        Console.WriteLine("Оптимальний план перевезень:");
                        PrintTable(SP, R, PN, PO);
                        TransportationCost(SP, R);
                        repeat = false;
                    }
                    else
                    {
                        Console.WriteLine($"\nУмова оптимальності НЕ виконується.");
                        Console.WriteLine($"Клітинка для початку аналізу [{nextRow + 1}, {nextCol + 1}]");

                        var cycle = FindCycle(R, nextRow, nextCol);
                        if (cycle != null)
                            RecalculatePlan(R, cycle);
                        else
                            repeat = false;
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

                //static void PrintInputData(int[,] sp, int[] po, int[] pn)
                //{
                //    int rows = sp.GetLength(0);
                //    int cols = sp.GetLength(1);

                //    int lo = po.Length;
                //    int ln = pn.Length;

                //    Console.WriteLine("\nМатриця вартостей:");

                //    for (int i = 0; i < rows; i++)
                //    {
                //        for (int j = 0; j < cols; j++)
                //            Console.Write("\t" + sp[i, j]);

                //        Console.WriteLine();
                //    }

                //    Console.WriteLine("\nВектор запасів:");

                //    for (int i = 0; i < lo; i++)
                //        Console.Write("\t" + po[i]);

                //    Console.WriteLine("\nВектор заявок:");

                //    for (int i = 0; i < ln; i++)
                //        Console.Write("\t" + pn[i]);
                //}

                static int[,] NorthwestCornerMethod(int[,] costs, int[] supply, int[] demand)
                {
                    int rows = supply.Length;
                    int cols = demand.Length;
                    int[,] result = new int[rows, cols];

                    int[] s = (int[])supply.Clone();
                    int[] d = (int[])demand.Clone();

                    int i = 0, j = 0;
                    while (i < rows && j < cols)
                    {
                        int quantity = Math.Min(s[i], d[j]);

                        result[i, j] = quantity;
                        s[i] -= quantity;
                        d[j] -= quantity;

                        if (s[i] == 0) i++;
                        else if (d[j] == 0) j++;
                    }

                    return result;
                }

                static void PrintMatrixResult(int[,] r)
                {
                    int rows = r.GetLength(0);
                    int cols = r.GetLength(1);

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                            Console.Write("\t" + (r[i, j] == 0 ? "x" : r[i, j].ToString()));
                        Console.WriteLine();
                    }
                }

                static int TransportationCost(int[,] sp, int[,] r)
                {
                    int sum = 0;
                    int rows = sp.GetLength(0);
                    int cols = sp.GetLength(1);
                    List<string> parts = new List<string>();

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            if (r[i, j] > 0)
                            {
                                sum += r[i, j] * sp[i, j];
                                parts.Add($"{r[i, j]}*{sp[i, j]}");
                            }
                        }
                    }
                    Console.Write("S = " + string.Join(" + ", parts));
                    Console.WriteLine($" = {sum}\n");

                    return sum;
                }

                static void CalculatePotentials(int[,] sp, int[,] r, out int?[] u, out int?[] v)
                {
                    int rows = sp.GetLength(0);
                    int cols = sp.GetLength(1);

                    u = new int?[rows];
                    v = new int?[cols];

                    u[0] = 0;

                    bool changed = true;
                    while (changed)
                    {
                        changed = false;
                        for (int i = 0; i < rows; i++)
                        {
                            for (int j = 0; j < cols; j++)
                            {
                                if (r[i, j] > 0)
                                {
                                    if (u[i].HasValue && !v[j].HasValue)
                                    {
                                        v[j] = sp[i, j] - u[i].Value;
                                        changed = true;
                                    }
                                    else if (v[j].HasValue && !u[i].HasValue)
                                    {
                                        u[i] = sp[i, j] - v[j]!.Value;
                                        changed = true;
                                    }
                                }
                            }
                        }
                    }
                }

                static int?[,] IndirectCosts(int[,] sp, int[,] r, int?[] u, int?[] v)
                {
                    int rows = sp.GetLength(0);
                    int cols = sp.GetLength(1);
                    int?[,] c = new int?[rows, cols];

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            if (r[i, j] > 0)
                            {
                                c[i, j] = null!;
                                continue;
                            }

                            c[i, j] = u[i] + v[j];
                        }
                    }
                    return c;
                }

                static bool CheckOptimum(int[,] sp, int?[] u, int?[] v, int[,] r, out int row, out int col)
                {
                    int rows = sp.GetLength(0);
                    int cols = sp.GetLength(1);

                    row = -1;
                    col = -1;
                    double maxDelta = 0;

                    for (int i = 0; i < rows; i++)
                    {
                        for (int j = 0; j < cols; j++)
                        {
                            if (r[i, j] == 0)
                            {
                                int delta = u[i]!.Value + v[j]!.Value - sp[i, j];

                                if (delta > maxDelta)
                                {
                                    maxDelta = delta;
                                    row = i;
                                    col = j;
                                }
                            }
                        }
                    }
                    return row == -1;
                }

                static List<int[]> FindCycle(int[,] r, int startR, int startC)
                {
                    List<int[]> path = new List<int[]>();
                    path.Add(new int[] { startR, startC });
                    if (SearchPath(r, path, startR, startC, true)) return path;
                    return null!;
                }

                static bool SearchPath(int[,] plan, List<int[]> path, int targetR, int targetC, bool moveHorizontally)
                {
                    int currR = path.Last()[0];
                    int currC = path.Last()[1];

                    if (moveHorizontally)
                    {
                        for (int c = 0; c < plan.GetLength(1); c++)
                        {
                            if (c == currC) continue;
                            if (c == targetC && currR == targetR && path.Count > 2) return true;
                            if (plan[currR, c] > 0)
                            {
                                path.Add(new int[] { currR, c });
                                if (SearchPath(plan, path, targetR, targetC, false)) return true;
                                path.RemoveAt(path.Count - 1);
                            }
                        }
                    }
                    else
                    {
                        for (int r = 0; r < plan.GetLength(0); r++)
                        {
                            if (r == currR) continue;
                            if (r == targetR && currC == targetC && path.Count > 2) return true;
                            if (plan[r, currC] > 0)
                            {
                                path.Add(new int[] { r, currC });
                                if (SearchPath(plan, path, targetR, targetC, true)) return true;
                                path.RemoveAt(path.Count - 1);
                            }
                        }
                    }
                    return false;
                }

                static void RecalculatePlan(int[,] plan, List<int[]> cycle)
                {
                    int lambda = int.MaxValue;
                    for (int i = 1; i < cycle.Count; i += 2)
                    {
                        int r = cycle[i][0];
                        int c = cycle[i][1];
                        if (plan[r, c] < lambda) lambda = plan[r, c];
                    }

                    Console.WriteLine($"\nВизначене значення λ = {lambda}");

                    for (int i = 0; i < cycle.Count; i++)
                    {
                        int r = cycle[i][0];
                        int c = cycle[i][1];

                        if (i % 2 == 0)
                            plan[r, c] += lambda;
                        else
                            plan[r, c] -= lambda;
                    }
                }

                static void PrintTable(
                    int[,] sp,
                    int[,] result,
                    int[] pn,
                    int[] po,
                    int?[,]? green = null)
                {
                    int rows = sp.GetLength(0);
                    int cols = sp.GetLength(1);

                    const int firstW = 8;
                    const int secondW = 8;
                    const int cellW = 12;

                    string Center(string text, int width)
                    {
                        if (text.Length >= width)
                            return text.Substring(0, width);

                        int left = (width - text.Length) / 2;
                        int right = width - text.Length - left;

                        return new string(' ', left) + text + new string(' ', right);
                    }

                    void WriteGreen(string text)
                    {
                        ConsoleColor oldColor = Console.ForegroundColor;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(text);
                        Console.ForegroundColor = oldColor;
                    }

                    void PrintBorder(char left, char middle, char right)
                    {
                        Console.Write(left);
                        Console.Write(new string('─', firstW));
                        Console.Write(middle);
                        Console.Write(new string('─', secondW));
                        Console.Write(middle);

                        for (int j = 0; j < cols; j++)
                        {
                            Console.Write(new string('─', cellW));
                            Console.Write(j == cols - 1 ? right : middle);
                        }

                        Console.WriteLine();
                    }

                    // Верхня межа
                    PrintBorder('┌', '┬', '┐');

                    // 1-й рядок: ПН, B1, B2, B3...
                    Console.Write("│");
                    Console.Write(new string(' ', firstW));
                    Console.Write("│");
                    Console.Write(Center("ПН", secondW));
                    Console.Write("│");

                    for (int j = 0; j < cols; j++)
                    {
                        Console.Write(Center($"B{j + 1}", cellW));
                        Console.Write("│");
                    }

                    Console.WriteLine();

                    PrintBorder('├', '┼', '┤');

                    // 2-й рядок: ПО, потреби po[]
                    Console.Write("│");
                    Console.Write(Center("ПО", firstW));
                    Console.Write("│");
                    Console.Write(new string(' ', secondW));
                    Console.Write("│");

                    for (int j = 0; j < cols; j++)
                    {
                        Console.Write(Center(pn[j].ToString(), cellW));
                        Console.Write("│");
                    }

                    Console.WriteLine();

                    PrintBorder('├', '┼', '┤');

                    // Основна частина таблиці
                    for (int i = 0; i < rows; i++)
                    {
                        // Верхній рядок клітинки: A1, A2... / запаси pn[] / вартості sp[,]
                        Console.Write("│");
                        Console.Write(Center($"A{i + 1}", firstW));
                        Console.Write("│");
                        Console.Write(Center(po[i].ToString(), secondW));
                        Console.Write("│");

                        for (int j = 0; j < cols; j++)
                        {
                            string cost = $"[ {sp[i, j]} ]";
                            Console.Write(cost.PadLeft(cellW - 1));
                            Console.Write(" │");
                        }

                        Console.WriteLine();

                        // Нижній рядок клітинки: зелене число + перевезення result[,]
                        Console.Write("│");
                        Console.Write(new string(' ', firstW));
                        Console.Write("│");
                        Console.Write(new string(' ', secondW));
                        Console.Write("│");

                        for (int j = 0; j < cols; j++)
                        {
                            string value = result[i, j] == 0 ? "" : result[i, j].ToString();

                            if (green != null && green[i, j].HasValue)
                            {
                                string greenValue = green[i, j].Value.ToString();

                                WriteGreen(greenValue);

                                int spaces = cellW - greenValue.Length - value.Length;

                                if (spaces < 0)
                                    spaces = 0;

                                Console.Write(new string(' ', spaces));
                                Console.Write(value);
                            }
                            else
                            {
                                Console.Write(value.PadRight(cellW));
                            }

                            Console.Write("│");
                        }

                        Console.WriteLine();

                        if (i < rows - 1)
                            PrintBorder('├', '┼', '┤');
                    }

                    // Нижня межа
                    PrintBorder('└', '┴', '┘');

                    Console.ResetColor();
                }
            }
        }
    }
}
