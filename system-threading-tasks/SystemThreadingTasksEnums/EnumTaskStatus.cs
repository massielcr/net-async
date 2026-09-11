namespace SystemThreadingTasksEnums
{
    internal static class EnumTaskStatus
    {        
        internal static async Task Run()
        {
            var tasks = new List<Task<int>>();
            var source = new CancellationTokenSource();
            var token = source.Token;
            int completedIterations = 0;
            Random random = new();

            for (int n = 0; n <= 19; n++)
                tasks.Add(Task.Run(() => {
                    int iterations = 0;
                    for (int ctr = 1; ctr <= 2000000; ctr++)
                    {
                        token.ThrowIfCancellationRequested();
                        iterations++;
                    }
                    Interlocked.Increment(ref completedIterations);

                    if (completedIterations >= 10)
                    {
                        source.Cancel();
                    }

                    bool shouldThrow;
                    lock (random) { shouldThrow = random.Next(1, 10) == 5; }
                    if (shouldThrow)
                    {
                        throw new Exception("Errrorrrr");
                    }

                    return iterations;
                }, token));

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException)
            {
                Console.WriteLine("Status of tasks:\n");
                Console.WriteLine("{0,10} {1,20} {2,14:N0}", "Task Id", "Status", "Iterations");
                foreach (var t in tasks)
                    Console.WriteLine("{0,10} {1,20} {2,14}",
                                      t.Id, t.Status,
                                      t.Status == TaskStatus.RanToCompletion ? t.Result.ToString("N0") : "n/a");
            }
        }
    }
}
