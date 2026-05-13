using System.Text;

namespace Lab_6
{
    internal class Program
    {
        private const string ProtocolFileName = "protocol.txt";

        static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            using StreamWriter protocol = new StreamWriter(ProtocolFileName, false, Encoding.UTF8);

            int[,] testMatrix1 =
            {
                {2, 4, 1, 3, 3},
                {1, 5, 4, 1, 2},
                {3, 5, 2, 2, 4},
                {1, 4, 3, 1, 4},
                {3, 2, 5, 3, 5}
            };

            int[,] testMatrix2 =
            {
                {2, 10, 9, 7},
                {15, 4, 14, 8},
                {13, 14, 16, 11},
                {4, 15, 13, 19}
            };

            int[,] myMatrix =
            {
                {6, 9, 4, 5},
                {4, 13, 7, 11},
                {10, 16, 21, 15},
                {14, 11, 10, 6}
            };

            while (true)
            {
                Console.Write("Оберіть матрицю:\n" +
                              " 0) Тестова матриця №1\n" +
                              " 1) Тестова матриця №2\n" +
                              " 2) Матриця з варіанту №2\n" +
                              " 3) Вийти\n" +
                              "Відповідь => ");

                int choice = GetValidMenuChoice(0, 3);
                Console.WriteLine();

                if (choice == 3)
                    break;

                int[,] matrix = choice switch
                {
                    0 => testMatrix1,
                    1 => testMatrix2,
                    2 => myMatrix,
                    _ => throw new InvalidOperationException("Невідомий пункт меню.")
                };

                protocol.WriteLine(new string('=', 95));
                protocol.WriteLine();

                int[] assignment = SolveHungarian(matrix, protocol);
                PrintResult(matrix, assignment, protocol);

                Console.WriteLine($"\nПротокол обчислень збережено у файл: {ProtocolFileName}\n");
            }
        }

        static int[] SolveHungarian(int[,] originalMatrix, StreamWriter protocol)
        {
            EnsureSquareMatrix(originalMatrix);

            int n = originalMatrix.GetLength(0);
            int[,] matrix = CopyMatrix(originalMatrix);

            PrintSection("Початкова матриця вартостей C");
            PrintMatrix(matrix);
            WriteProtocolSection(protocol, "Початкова матриця вартостей C");
            WriteProtocolMatrix(protocol, matrix);

            PrintSection("Крок 1. Зведення рядків");
            protocol.WriteLine("Крок 1. Зведення рядків");
            protocol.WriteLine("У кожному рядку знаходимо мінімальний елемент і віднімаємо його від усіх елементів цього рядка.");
            protocol.WriteLine();

            for (int i = 0; i < n; i++)
            {
                int min = matrix[i, 0];

                for (int j = 1; j < n; j++)
                    if (matrix[i, j] < min)
                        min = matrix[i, j];

                protocol.WriteLine($"R{i + 1}: min = {min}; віднімаємо {min} від кожного елемента рядка R{i + 1}.");
                Console.WriteLine($"Рядок {i + 1}: мінімальний елемент = {min}");

                for (int j = 0; j < n; j++)
                    matrix[i, j] -= min;
            }

            Console.WriteLine();
            PrintMatrix(matrix);
            protocol.WriteLine();
            protocol.WriteLine("Матриця після зведення рядків:");
            WriteProtocolMatrix(protocol, matrix);

            PrintSection("Крок 2. Зведення стовпців");
            protocol.WriteLine("Крок 2. Зведення стовпців");
            protocol.WriteLine("У кожному стовпці знаходимо мінімальний елемент і віднімаємо його від усіх елементів цього стовпця.");
            protocol.WriteLine();

            for (int j = 0; j < n; j++)
            {
                int min = matrix[0, j];

                for (int i = 1; i < n; i++)
                    if (matrix[i, j] < min)
                        min = matrix[i, j];

                protocol.WriteLine($"C{j + 1}: min = {min}; віднімаємо {min} від кожного елемента стовпця C{j + 1}.");
                Console.WriteLine($"Стовпець {j + 1}: мінімальний елемент = {min}");

                for (int i = 0; i < n; i++)
                    matrix[i, j] -= min;
            }

            Console.WriteLine();
            PrintMatrix(matrix);
            protocol.WriteLine();
            protocol.WriteLine("Матриця після зведення стовпців:");
            WriteProtocolMatrix(protocol, matrix);

            int iteration = 1;

            while (true)
            {
                PrintSection($"Крок 3. Пошук незалежних нулів. Ітерація {iteration}");
                protocol.WriteLine($"Крок 3. Пошук незалежних нулів. Ітерація {iteration}");
                protocol.WriteLine();

                int[] assignment = FindMaximumZeroAssignment(matrix);
                int assignedCount = assignment.Count(x => x != -1);

                Console.WriteLine($"Кількість незалежних нулів: {assignedCount} з {n}");
                protocol.WriteLine($"Кількість незалежних нулів: {assignedCount} з {n}");
                protocol.WriteLine("Нулі, взяті як поточні незалежні призначення, позначені символом *.");
                WriteProtocolMatrix(protocol, matrix, assignment);

                if (assignedCount == n)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Знайдено повну систему незалежних нулів.");
                    Console.ResetColor();

                    protocol.WriteLine("Знайдено повну систему незалежних нулів.");
                    protocol.WriteLine("На основі цих нулів будуємо оптимальну матрицю призначень.");
                    protocol.WriteLine();

                    return assignment;
                }

                PrintSection("Крок 4. Викреслення нулів мінімальною кількістю ліній");
                protocol.WriteLine("Крок 4. Викреслення нулів мінімальною кількістю ліній");
                protocol.WriteLine("Викреслюємо всі нулі мінімальною кількістю горизонтальних та вертикальних ліній.");
                protocol.WriteLine("У таблиці: H — викреслений рядок, V — викреслений стовпець, X — перетин двох ліній.");
                protocol.WriteLine();

                LineCover cover = FindMinimumLineCover(matrix, assignment);

                PrintCoverInfo(cover);
                WriteProtocolCoverInfo(protocol, cover);
                protocol.WriteLine();
                protocol.WriteLine("Матриця з показом викреслень:");
                WriteProtocolCoveredMatrix(protocol, matrix, cover, assignment);

                int lineCount = cover.Rows.Count(r => r) + cover.Columns.Count(c => c);

                Console.WriteLine($"Кількість ліній: {lineCount}");
                protocol.WriteLine($"Кількість ліній: {lineCount}");
                protocol.WriteLine();

                PrintSection("Крок 5. Перетворення матриці");
                protocol.WriteLine("Крок 5. Перетворення матриці");
                protocol.WriteLine("1) Серед невикреслених елементів знаходимо мінімальний елемент M.");
                protocol.WriteLine("2) M віднімаємо від усіх невикреслених елементів.");
                protocol.WriteLine("3) M додаємо до елементів, які лежать на перетині двох ліній.");
                protocol.WriteLine("4) Елементи, викреслені тільки однією лінією, залишаємо без змін.");
                protocol.WriteLine();

                int minUncovered = FindMinUncovered(matrix, cover);

                Console.WriteLine($"Мінімальний невикреслений елемент M = {minUncovered}");
                protocol.WriteLine($"Мінімальний невикреслений елемент M = {minUncovered}");
                protocol.WriteLine();

                protocol.WriteLine("Пояснення змін:");
                WriteProtocolTransformation(protocol, matrix, cover, minUncovered);

                TransformMatrix(matrix, cover, minUncovered);

                Console.WriteLine("Матриця після перетворення:");
                PrintMatrix(matrix);

                protocol.WriteLine();
                protocol.WriteLine("Матриця після перетворення:");
                WriteProtocolMatrix(protocol, matrix);

                iteration++;
            }
        }

        static int[] FindMaximumZeroAssignment(int[,] matrix)
        {
            int n = matrix.GetLength(0);
            int[] rowToColumn = Enumerable.Repeat(-1, n).ToArray();
            int[] columnToRow = Enumerable.Repeat(-1, n).ToArray();

            for (int row = 0; row < n; row++)
            {
                bool[] visitedColumns = new bool[n];
                TryFindAugmentingPath(row, matrix, visitedColumns, rowToColumn, columnToRow);
            }

            return rowToColumn;
        }

        static bool TryFindAugmentingPath(int row, int[,] matrix, bool[] visitedColumns, int[] rowToColumn, int[] columnToRow)
        {
            int n = matrix.GetLength(0);

            for (int column = 0; column < n; column++)
            {
                if (matrix[row, column] != 0 || visitedColumns[column])
                    continue;

                visitedColumns[column] = true;

                if (columnToRow[column] == -1 ||
                    TryFindAugmentingPath(columnToRow[column], matrix, visitedColumns, rowToColumn, columnToRow))
                {
                    rowToColumn[row] = column;
                    columnToRow[column] = row;
                    return true;
                }
            }

            return false;
        }

        static LineCover FindMinimumLineCover(int[,] matrix, int[] assignment)
        {
            int n = matrix.GetLength(0);
            int[] columnToRow = Enumerable.Repeat(-1, n).ToArray();

            for (int row = 0; row < n; row++)
                if (assignment[row] != -1)
                    columnToRow[assignment[row]] = row;

            bool[] markedRows = new bool[n];
            bool[] markedColumns = new bool[n];
            Queue<int> queue = new Queue<int>();

            for (int row = 0; row < n; row++)
            {
                if (assignment[row] == -1)
                {
                    markedRows[row] = true;
                    queue.Enqueue(row);
                }
            }

            while (queue.Count > 0)
            {
                int row = queue.Dequeue();

                for (int column = 0; column < n; column++)
                {
                    if (matrix[row, column] == 0 && !markedColumns[column])
                    {
                        markedColumns[column] = true;
                        int assignedRow = columnToRow[column];

                        if (assignedRow != -1 && !markedRows[assignedRow])
                        {
                            markedRows[assignedRow] = true;
                            queue.Enqueue(assignedRow);
                        }
                    }
                }
            }

            bool[] coveredRows = new bool[n];
            bool[] coveredColumns = new bool[n];

            for (int row = 0; row < n; row++)
                coveredRows[row] = !markedRows[row];

            for (int column = 0; column < n; column++)
                coveredColumns[column] = markedColumns[column];

            return new LineCover(coveredRows, coveredColumns, markedRows, markedColumns);
        }

        static int FindMinUncovered(int[,] matrix, LineCover cover)
        {
            int n = matrix.GetLength(0);
            int min = int.MaxValue;

            for (int i = 0; i < n; i++)
            {
                if (cover.Rows[i])
                    continue;

                for (int j = 0; j < n; j++)
                {
                    if (cover.Columns[j])
                        continue;

                    if (matrix[i, j] < min)
                        min = matrix[i, j];
                }
            }

            if (min == int.MaxValue)
                throw new InvalidOperationException("Не вдалося знайти невикреслений елемент.");

            return min;
        }

        static void TransformMatrix(int[,] matrix, LineCover cover, int minUncovered)
        {
            int n = matrix.GetLength(0);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    bool rowCovered = cover.Rows[i];
                    bool columnCovered = cover.Columns[j];

                    if (!rowCovered && !columnCovered)
                        matrix[i, j] -= minUncovered;
                    else if (rowCovered && columnCovered)
                        matrix[i, j] += minUncovered;
                }
            }
        }

        static void PrintResult(int[,] originalMatrix, int[] assignment, StreamWriter protocol)
        {
            int n = originalMatrix.GetLength(0);
            int totalCost = 0;

            PrintSection("ОПТИМАЛЬНЕ ПРИЗНАЧЕННЯ");

            protocol.WriteLine("ОПТИМАЛЬНЕ ПРИЗНАЧЕННЯ");
            protocol.WriteLine(new string('-', 95));

            Console.WriteLine("Матриця призначень:");
            protocol.WriteLine("Матриця призначень:");

            int[,] assignmentMatrix = new int[n, n];

            for (int i = 0; i < n; i++)
            {
                int j = assignment[i];
                if (j < 0)
                    throw new InvalidOperationException("Неможливо побудувати результат: не всі рядки мають призначення.");

                assignmentMatrix[i, j] = 1;
                totalCost += originalMatrix[i, j];
            }

            PrintMatrix(assignmentMatrix);
            WriteProtocolMatrix(protocol, assignmentMatrix);

            Console.WriteLine("Обрані призначення:");
            protocol.WriteLine("Обрані призначення:");

            for (int i = 0; i < n; i++)
            {
                int j = assignment[i];
                string line = $"Працівник {i + 1} → робота {j + 1}, вартість = {originalMatrix[i, j]}";

                Console.WriteLine(line);
                protocol.WriteLine(line);
            }

            Console.WriteLine($"\nМінімальна загальна вартість робіт S = {totalCost}");

            protocol.WriteLine();
            protocol.WriteLine($"Мінімальна загальна вартість робіт S = {totalCost}");
        }

        static void WriteProtocolTransformation(StreamWriter protocol, int[,] matrix, LineCover cover, int minUncovered)
        {
            int n = matrix.GetLength(0);
            WriteColumnHeader(protocol, n);

            for (int i = 0; i < n; i++)
            {
                protocol.Write($"R{i + 1,-3}");

                for (int j = 0; j < n; j++)
                {
                    bool rowCovered = cover.Rows[i];
                    bool columnCovered = cover.Columns[j];
                    string text;

                    if (!rowCovered && !columnCovered)
                        text = $"{matrix[i, j]}-{minUncovered}";
                    else if (rowCovered && columnCovered)
                        text = $"{matrix[i, j]}+{minUncovered}";
                    else
                        text = matrix[i, j].ToString();

                    protocol.Write($"{text,9}");
                }

                protocol.WriteLine();
            }
        }

        static void WriteProtocolCoveredMatrix(StreamWriter protocol, int[,] matrix, LineCover cover, int[] assignment)
        {
            int n = matrix.GetLength(0);

            protocol.Write("      ");
            for (int j = 0; j < n; j++)
            {
                string marker = cover.Columns[j] ? "V" : " ";
                protocol.Write($"{marker + "C" + (j + 1),8}");
            }
            protocol.WriteLine();

            for (int i = 0; i < n; i++)
            {
                string rowMarker = cover.Rows[i] ? "H" : " ";
                protocol.Write($"{rowMarker}R{i + 1,-3}");

                for (int j = 0; j < n; j++)
                {
                    bool rowCovered = cover.Rows[i];
                    bool columnCovered = cover.Columns[j];
                    bool chosenZero = assignment[i] == j;

                    string lineMark = rowCovered && columnCovered ? "X" : rowCovered ? "H" : columnCovered ? "V" : " ";
                    string star = chosenZero ? "*" : " ";
                    string cell = $"{matrix[i, j]}{star}{lineMark}";

                    protocol.Write($"{cell,8}");
                }

                protocol.WriteLine();
            }

            protocol.WriteLine();
        }

        static void WriteProtocolMatrix(StreamWriter protocol, int[,] matrix, int[]? assignment = null)
        {
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);

            WriteColumnHeader(protocol, columns);

            for (int i = 0; i < rows; i++)
            {
                protocol.Write($"R{i + 1,-3}");

                for (int j = 0; j < columns; j++)
                {
                    string star = assignment != null && assignment[i] == j ? "*" : " ";
                    protocol.Write($"{matrix[i, j] + star,7}");
                }

                protocol.WriteLine();
            }

            protocol.WriteLine();
        }

        static void WriteColumnHeader(StreamWriter protocol, int columns)
        {
            protocol.Write("    ");
            for (int j = 0; j < columns; j++)
                protocol.Write($"{"C" + (j + 1),7}");
            protocol.WriteLine();
        }

        static void PrintMatrix(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                Console.Write("| ");

                for (int j = 0; j < columns; j++)
                {
                    if (matrix[i, j] == 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write($"{matrix[i, j],5}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write($"{matrix[i, j],5}");
                    }
                }

                Console.WriteLine(" |");
            }

            Console.WriteLine();
        }

        static int[,] CopyMatrix(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int columns = matrix.GetLength(1);
            int[,] copy = new int[rows, columns];

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < columns; j++)
                    copy[i, j] = matrix[i, j];

            return copy;
        }

        static void EnsureSquareMatrix(int[,] matrix)
        {
            if (matrix.GetLength(0) != matrix.GetLength(1))
                throw new ArgumentException("Угорський метод у цій реалізації працює лише з квадратною матрицею.");
        }

        static void PrintCoverInfo(LineCover cover)
        {
            Console.WriteLine("Викреслені рядки: " + FormatIndexes(cover.Rows, "R"));
            Console.WriteLine("Викреслені стовпці: " + FormatIndexes(cover.Columns, "C"));
        }

        static void WriteProtocolCoverInfo(StreamWriter protocol, LineCover cover)
        {
            protocol.WriteLine("Позначені рядки: " + FormatIndexes(cover.MarkedRows, "R"));
            protocol.WriteLine("Позначені стовпці: " + FormatIndexes(cover.MarkedColumns, "C"));
            protocol.WriteLine("Мінімальне покриття = непозначені рядки + позначені стовпці.");
            protocol.WriteLine("Викреслені рядки: " + FormatIndexes(cover.Rows, "R"));
            protocol.WriteLine("Викреслені стовпці: " + FormatIndexes(cover.Columns, "C"));
        }

        static string FormatIndexes(bool[] flags, string prefix)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < flags.Length; i++)
                if (flags[i])
                    result.Add($"{prefix}{i + 1}");

            return result.Count == 0 ? "немає" : string.Join(", ", result);
        }

        static void PrintTitle(string title)
        {
            Console.WriteLine(new string('=', 70));
            Console.WriteLine(title);
            Console.WriteLine(new string('=', 70));
            Console.WriteLine();
        }

        static void PrintSection(string title)
        {
            Console.WriteLine();
            Console.WriteLine(title);
            Console.WriteLine(new string('-', title.Length));
        }

        static void WriteProtocolSection(StreamWriter protocol, string title)
        {
            protocol.WriteLine(title);
            protocol.WriteLine(new string('-', title.Length));
            protocol.WriteLine();
        }

        static int GetValidMenuChoice(int min, int max)
        {
            while (true)
            {
                string? input = Console.ReadLine();

                if (int.TryParse(input, out int result) && result >= min && result <= max)
                    return result;

                WriteError($"Введіть число від {min} до {max} => ");
            }
        }

        static void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Помилка: ");
            Console.ResetColor();
            Console.Write(message);
        }
    }

    internal class LineCover
    {
        public bool[] Rows { get; }
        public bool[] Columns { get; }
        public bool[] MarkedRows { get; }
        public bool[] MarkedColumns { get; }

        public LineCover(bool[] rows, bool[] columns, bool[] markedRows, bool[] markedColumns)
        {
            Rows = rows;
            Columns = columns;
            MarkedRows = markedRows;
            MarkedColumns = markedColumns;
        }
    }
}
