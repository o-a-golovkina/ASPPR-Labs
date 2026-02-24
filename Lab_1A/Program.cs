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
                    double[,] myMatrix = FillMatrixFromConsole(size);

                    Console.WriteLine("\n");

                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine($"\n--- Обернена матриця ---");
                    PrintMatrix(InverseMatrix(myMatrix), "invers");
                    Console.ResetColor();

                    Console.WriteLine("\n\n");

                    break;

                case 2:

                    break;

                case 3:
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

    static double[,] FillMatrixFromConsole(int size)
    {
        double[,] matrix = new double[size, size];

        for (int i = 0; i < size; i++)
            Console.Write($"\tX{i + 1}");

        for (int i = 0; i < size; i++) //Rows
        {
            Console.Write($"\nY{i + 1}\t");
            for (int j = 0; j < size; j++) //Columns
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
            else if (char.IsDigit(keyChar) && keyChar != '0')
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
        int cols = originalMatrix.GetLength(1);
        double[,] inverseMatrix = originalMatrix;

        for (int i = 0; i < rows; i++)
        {
            double[,] tempMatrix = null!;
            PivotOperation(ref tempMatrix, inverseMatrix, rows, cols, i, i);
            if (tempMatrix != null)
                inverseMatrix = tempMatrix;
        }

        return inverseMatrix;
    }

    static double[,] PivotOperation(ref double[,] tempMatrix, double[,] previousMatrix, int rows, int cols, int r, int s)
    {
        double pivot = previousMatrix[r, s];

        if (Math.Abs(pivot) < 1e-10)
        {
            WriteError("Розв'язувальний елемент не може дорівнювати нулю.");
            return tempMatrix;
        }

        tempMatrix = new double[rows, cols];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (i == r && j == s) //Main element
                    tempMatrix[i, j] = 1.0 / pivot;

                else if (i == r) //Elements in the same row
                    tempMatrix[i, j] = -previousMatrix[i, j] / pivot;

                else if (j == s) // Elements in the same column
                    tempMatrix[i, j] = previousMatrix[i, j] / pivot;

                else
                    tempMatrix[i, j] = (previousMatrix[i, j] * pivot - previousMatrix[i, s] * previousMatrix[r, j]) / pivot;
            }
        }

        return tempMatrix;
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
