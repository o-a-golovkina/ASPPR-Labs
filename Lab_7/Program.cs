using System.Text;

namespace Lab_7
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            while (true)
            {
                Console.Clear();
                Console.WriteLine("==============================================");
                Console.WriteLine("     РОЗРАХУНОК СІТКОВОГО ГРАФІКА РОБІТ");
                Console.WriteLine("==============================================");
                Console.WriteLine("1 - Тестові дані №1");
                Console.WriteLine("2 - Тестові дані №2");
                Console.WriteLine("3 - Мій варіант №2");
                Console.WriteLine("0 - Вихід");
                Console.WriteLine("==============================================");
                Console.Write("Ваш вибір: ");

                string choice = Console.ReadLine();

                if (choice == "0")
                    break;

                List<Work> works = null;
                string dataName = "";

                if (choice == "1")
                {
                    works = GetTest1();
                    dataName = "Тестові дані №1";
                }
                else if (choice == "2")
                {
                    works = GetTest2();
                    dataName = "Тестові дані №2";
                }
                else if (choice == "3")
                {
                    works = GetVariant2();
                    dataName = "Мій варіант №2";
                }
                else
                {
                    Console.WriteLine("Невірний вибір!");
                    Console.ReadKey();
                    continue;
                }

                CalculateNetwork(works);

                string protocol = CreateProtocol(works, dataName);

                Console.Clear();
                Console.WriteLine(protocol);

                string fileName = "protocol_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                File.WriteAllText(fileName, protocol, Encoding.UTF8);

                Console.WriteLine();
                Console.WriteLine("==============================================");
                Console.WriteLine("Протокол обчислень збережено у файл:");
                Console.WriteLine(fileName);
                Console.WriteLine("==============================================");
                Console.WriteLine("Натисніть будь-яку клавішу для повернення в меню...");
                Console.ReadKey();
            }
        }

        static List<Work> GetTest1()
        {
            return new List<Work>
            {
                new Work(1, new List<int>(), 5, 2),
                new Work(2, new List<int>{1}, 8, 3),
                new Work(3, new List<int>{1}, 3, 2),
                new Work(4, new List<int>{1}, 6, 2),
                new Work(5, new List<int>{2}, 7, 3),
                new Work(6, new List<int>{2, 3}, 6, 2),
                new Work(7, new List<int>{4, 5, 6}, 4, 2)
            };
        }

        static List<Work> GetTest2()
        {
            return new List<Work>
            {
                new Work(1, new List<int>(), 3, 2),
                new Work(2, new List<int>{1}, 4, 3),
                new Work(3, new List<int>{1}, 2, 4),
                new Work(4, new List<int>{2}, 5, 3),
                new Work(5, new List<int>{3}, 1, 2),
                new Work(6, new List<int>{3}, 2, 3),
                new Work(7, new List<int>{4, 5}, 4, 2),
                new Work(8, new List<int>{6, 7}, 3, 2)
            };
        }

        static List<Work> GetVariant2()
        {
            return new List<Work>
            {
                new Work(1, new List<int>(), 10, 3),
                new Work(2, new List<int>(), 7, 4),
                new Work(3, new List<int>{1}, 9, 2),
                new Work(4, new List<int>{2}, 7, 1),
                new Work(5, new List<int>{3, 4}, 6, 2),
                new Work(6, new List<int>{4}, 6, 1),
                new Work(7, new List<int>{5, 6}, 12, 2),
                new Work(8, new List<int>{5, 6}, 5, 1),
                new Work(9, new List<int>{7}, 6, 2),
                new Work(10, new List<int>{5, 6}, 11, 5),
                new Work(11, new List<int>{8}, 9, 2)
            };
        }

        static void CalculateNetwork(List<Work> works)
        {
            // Прямий хід: розрахунок ранніх дат
            foreach (Work work in works)
            {
                if (work.Previous.Count == 0)
                {
                    work.EarlyStart = 0;
                }
                else
                {
                    int maxFinish = 0;

                    foreach (int prevId in work.Previous)
                    {
                        Work prevWork = works.First(w => w.Id == prevId);

                        if (prevWork.EarlyFinish > maxFinish)
                            maxFinish = prevWork.EarlyFinish;
                    }

                    work.EarlyStart = maxFinish;
                }

                work.EarlyFinish = work.EarlyStart + work.Duration;
            }

            int projectDuration = works.Max(w => w.EarlyFinish);

            // Зворотний хід: розрахунок пізніх дат
            for (int i = works.Count - 1; i >= 0; i--)
            {
                Work work = works[i];

                List<Work> nextWorks = works
                    .Where(w => w.Previous.Contains(work.Id))
                    .ToList();

                if (nextWorks.Count == 0)
                {
                    work.LateFinish = projectDuration;
                }
                else
                {
                    work.LateFinish = nextWorks.Min(w => w.LateStart);
                }

                work.LateStart = work.LateFinish - work.Duration;
                work.Reserve = work.LateStart - work.EarlyStart;
            }
        }

        static string CreateProtocol(List<Work> works, string dataName)
        {
            StringBuilder sb = new StringBuilder();

            int projectDuration = works.Max(w => w.EarlyFinish);
            List<Work> criticalWorks = works
                .Where(w => w.Reserve == 0)
                .OrderBy(w => w.EarlyStart)
                .ToList();

            sb.AppendLine("==============================================================");
            sb.AppendLine("              ПРОТОКОЛ РОЗРАХУНКУ СІТКОВОГО ГРАФІКА");
            sb.AppendLine("==============================================================");
            sb.AppendLine("Набір даних: " + dataName);
            sb.AppendLine("Дата та час розрахунку: " + DateTime.Now);
            sb.AppendLine();

            sb.AppendLine("Вхідні дані:");
            sb.AppendLine("--------------------------------------------------------------");
            sb.AppendLine(String.Format("{0,-8}{1,-22}{2,-15}{3,-15}",
                "Робота", "Попередні роботи", "Тривалість", "Кільк. людей"));

            foreach (Work w in works)
            {
                string prev = w.Previous.Count == 0 ? "-" : String.Join(",", w.Previous);

                sb.AppendLine(String.Format("{0,-8}{1,-22}{2,-15}{3,-15}",
                    w.Id, prev, w.Duration, w.People));
            }

            sb.AppendLine();
            sb.AppendLine("Пояснення до розрахунків:");
            sb.AppendLine("Ранній старт роботи визначається як найбільше раннє закінчення її попередніх робіт.");
            sb.AppendLine("Раннє закінчення = ранній старт + тривалість роботи.");
            sb.AppendLine("Пізнє закінчення визначається під час зворотного ходу сіткового графіка.");
            sb.AppendLine("Пізній старт = пізнє закінчення - тривалість роботи.");
            sb.AppendLine("Резерв часу = пізній старт - ранній старт.");
            sb.AppendLine("Критичними є роботи, у яких резерв часу дорівнює нулю.");
            sb.AppendLine();

            sb.AppendLine("Прямий хід аналізу сіткового графіка:");
            sb.AppendLine("--------------------------------------------------------------");

            foreach (Work w in works)
            {
                sb.AppendLine("Робота " + w.Id + ":");

                if (w.Previous.Count == 0)
                {
                    sb.AppendLine("  Попередніх робіт немає, тому ранній старт = 0.");
                }
                else
                {
                    sb.AppendLine("  Попередні роботи: " + String.Join(",", w.Previous) + ".");
                    sb.AppendLine("  Ранній старт береться як максимальне раннє закінчення попередніх робіт.");
                }

                sb.AppendLine("  Тривалість роботи: " + w.Duration);
                sb.AppendLine("  Ранній старт: " + w.EarlyStart);
                sb.AppendLine("  Раннє закінчення: " + w.EarlyFinish);
                sb.AppendLine();
            }

            sb.AppendLine("Зворотний хід аналізу сіткового графіка:");
            sb.AppendLine("--------------------------------------------------------------");

            foreach (Work w in works.OrderByDescending(w => w.Id))
            {
                List<Work> nextWorks = works.Where(x => x.Previous.Contains(w.Id)).ToList();

                sb.AppendLine("Робота " + w.Id + ":");

                if (nextWorks.Count == 0)
                {
                    sb.AppendLine("  Наступних робіт немає, тому пізнє закінчення дорівнює тривалості проекту.");
                }
                else
                {
                    sb.AppendLine("  Наступні роботи: " + String.Join(",", nextWorks.Select(x => x.Id)) + ".");
                    sb.AppendLine("  Пізнє закінчення визначається як мінімальний пізній старт наступних робіт.");
                }

                sb.AppendLine("  Пізнє закінчення: " + w.LateFinish);
                sb.AppendLine("  Пізній старт: " + w.LateStart);
                sb.AppendLine("  Резерв часу: " + w.Reserve);
                sb.AppendLine();
            }

            sb.AppendLine("Розраховані параметри сіткового графіка робіт:");
            sb.AppendLine("--------------------------------------------------------------");
            sb.AppendLine(String.Format("{0,-8}{1,-12}{2,-12}{3,-12}{4,-12}{5,-12}{6,-10}",
                "Робота", "Трив.", "Людей", "РС", "РЗ", "ПС", "ПЗ"));

            foreach (Work w in works)
            {
                string mark = w.Reserve == 0 ? "  К" : "";

                sb.AppendLine(String.Format("{0,-8}{1,-12}{2,-12}{3,-12}{4,-12}{5,-12}{6,-10} Резерв: {7}{8}",
                    w.Id,
                    w.Duration,
                    w.People,
                    w.EarlyStart,
                    w.EarlyFinish,
                    w.LateStart,
                    w.LateFinish,
                    w.Reserve,
                    mark));
            }

            sb.AppendLine();
            sb.AppendLine("Позначення:");
            sb.AppendLine("РС - ранній старт");
            sb.AppendLine("РЗ - раннє закінчення");
            sb.AppendLine("ПС - пізній старт");
            sb.AppendLine("ПЗ - пізнє закінчення");
            sb.AppendLine("К  - критична робота");
            sb.AppendLine();

            sb.AppendLine("Тривалість проекту: " + projectDuration);
            sb.AppendLine("Критичний шлях: " + String.Join(" - ", criticalWorks.Select(w => w.Id)));

            sb.AppendLine();
            sb.AppendLine("Висновок:");
            sb.AppendLine("У результаті розрахунку було визначено ранні та пізні терміни виконання робіт,");
            sb.AppendLine("резерви часу, загальну тривалість проекту та критичний шлях.");
            sb.AppendLine("Роботи критичного шляху не мають резерву часу, тому їх затримка призведе");
            sb.AppendLine("до збільшення загальної тривалості всього проекту.");

            return sb.ToString();
        }
    }
}
