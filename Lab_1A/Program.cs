using Lab_1A;
using System.Globalization;
using System.Text;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;

        bool isRunning = true;

        while (isRunning)
        {
            Console.WriteLine();
            Console.WriteLine(UIStrings.MainMenu);
            Console.Write(UIStrings.MenuAnswer);
            int choice = GetValidMenuChoice(0, 3);

            switch (choice)
            {
                case 0:
                    isRunning = false;
                    break;

                case 1:
                    Console.Write(UIStrings.M1Size);
                    int size = GetValidMenuChoice(2, 100);

                    Console.WriteLine($"\n--- Заповнення матриці {size}x{size} ---");
                    double[,] myMatrix = FillMatrixFromConsole(size, size);

                    Console.WriteLine("\n");

                    if (InverseMatrix(myMatrix) != null)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine($"\n--- Обернена матриця ---");

                        PrintMatrix(InverseMatrix(myMatrix), "invers");
                        Console.ResetColor();
                    }
                    Console.WriteLine("\n");
                    break;

                case 2:
                    Console.Write(UIStrings.M2SizeRows);
                    int rows = GetValidMenuChoice(2, 100);
                    Console.Write(UIStrings.M2SizeCols);
                    int cols = GetValidMenuChoice(2, 100);

                    Console.WriteLine($"\n--- Заповнення матриці {rows}x{cols} ---");
                    myMatrix = FillMatrixFromConsole(rows, cols);

                    Console.WriteLine("\n");

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"\nРанг матриці: " + GetMatrixRank(myMatrix));
                    Console.ResetColor();
                    Console.WriteLine();
                    break;

                case 3:
                    Console.Write(UIStrings.M3Size);
                    int n = GetValidMenuChoice(2, 100);

                    Console.WriteLine($"\n--- Заповнення матриці А {n}x{n} ---");
                    myMatrix = FillMatrixFromConsole(n, n);

                    Console.WriteLine("\n");

                    Console.WriteLine($"\n--- Заповнення матриці B {n}x{1} ---");
                    double[,] matrixB = FillMatrixFromConsole(n, 1);

                    Console.WriteLine("\n");

                    if (InverseMatrix(myMatrix) == null)
                    {
                        Console.WriteLine("\n");
                        break;
                    }

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine($"\n--- Обернена матриця A ---");

                    PrintMatrix(InverseMatrix(myMatrix), "invers");

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("\n");
                    Console.WriteLine($"\n--- Обчислення розв'язків Х ---");
                    Console.WriteLine(VectorX(InverseMatrix(myMatrix), matrixB));

                    Console.WriteLine();
                    Console.ResetColor();
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

    static double[,] InverseMatrix(double[,] originalMatrix)
    {
        int rows = originalMatrix.GetLength(0);

        double[,] inverseMatrix = originalMatrix;

        for (int i = 0; i < rows; i++)
        {
            double[,] temp = PivotOperation(inverseMatrix, i, i);

            if (temp != null)
                inverseMatrix = temp;

            else
            {
                WriteError($"Крок неможливий (елемент [{i},{i}] = 0). Матриця вироджена, оберненої не існує.");
                return null!;
            }
        }

        return inverseMatrix;
    }

    static double[,] PivotOperation(double[,] previousMatrix, int r, int s)
    {
        int rows = previousMatrix.GetLength(0);
        int cols = previousMatrix.GetLength(1);
        double pivot = previousMatrix[r, s];

        if (Math.Abs(pivot) < 1e-10)
            return null!;

        double[,] nextMatrix = new double[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (i == r && j == s)
                    nextMatrix[i, j] = 1.0 / pivot;
                else if (i == r)
                    nextMatrix[i, j] = -previousMatrix[i, j] / pivot;
                else if (j == s)
                    nextMatrix[i, j] = previousMatrix[i, j] / pivot;
                else
                    nextMatrix[i, j] = (previousMatrix[i, j] * pivot - previousMatrix[i, s] * previousMatrix[r, j]) / pivot;
            }
        }

        return nextMatrix;
    }

    static int GetMatrixRank(double[,] originalMatrix)
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

            currentMatrix = PivotOperation(currentMatrix, pivotRow, pivotCol);

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
            x = 0;
            for (int j = 0; j < matrixA.GetLength(0); j++)
                x += matrixB[j, 0] * matrixA[i, j];

            str += $"\nX[{i + 1}] = {x:F3}";
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
}
