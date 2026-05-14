namespace Lab_7
{
    public class Work
    {
        class Work
        {
            public int Id { get; set; }
            public List<int> Previous { get; set; }
            public int Duration { get; set; }
            public int People { get; set; }

            public int EarlyStart { get; set; }
            public int EarlyFinish { get; set; }
            public int LateStart { get; set; }
            public int LateFinish { get; set; }
            public int Reserve { get; set; }

            public Work(int id, List<int> previous, int duration, int people)
            {
                Id = id;
                Previous = previous;
                Duration = duration;
                People = people;
            }
        }
    }
