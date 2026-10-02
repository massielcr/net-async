using System.Collections.Concurrent;

namespace SystemThreadingTasks
{
    internal class TaskClass
    {
        private static Random random = new();

        internal static async Task RunInstantiation()
        {
            Console.WriteLine($"1. Create Action<object>");
            Action<object> action = (object obj) =>
            {
                Console.WriteLine($"Task={Task.CurrentId} obj={obj} Thread={Thread.CurrentThread.ManagedThreadId}");
            };

            Console.WriteLine($"2. Create Task by calling a Task constructor, but started with Start()");
            Task t1 = new(action, "t1");
            t1.Start();

            Console.WriteLine($"3. Create Task by calling Task.Factory.StartNew");
            Task t2 = Task.Factory.StartNew(action, "t2");

            Console.WriteLine($"4. Create Task by calling Task.Run(Action)");
            Task t3 = Task.Run(() => action("t3"));

            Console.WriteLine($"5. Create Task by calling a Task constructor and run it synchronously");
            Task t4 = new(action, "t4");
            t4.RunSynchronously();

            await Task.WhenAll(t1, t2, t3);
        }

        internal static async Task RunWhenAnyTasks(int min, int max, int workersCount)
        {
            Action action = () =>
            {
                int timer = random.Next(min, max);
                Thread.Sleep(timer);

                Console.WriteLine($"Task {Task.CurrentId} slept for {timer} on Thread {Thread.CurrentThread.ManagedThreadId}");
            };

            Task[] tasks = Enumerable.Range(0, workersCount).Select(_ => Task.Run(() => action())).ToArray();

            try
            {
                Task tResult = await Task.WhenAny(tasks);

                int index = Array.IndexOf(tasks, tResult);

                Console.WriteLine($"Task #{index} with Id {tResult.Id} completed first.");
                foreach(Task t in tasks)
                {
                    Console.WriteLine($"Task #{t.Id}: {t.Status}");
                }
            }
            catch(AggregateException ex)
            {
                Console.WriteLine("An exception occurred.");
            }
        }

        internal static async Task RunWhenAllTasks(int timer, int workersCount)
        {
            Console.WriteLine($"1. Created {workersCount} Tasks");

            Task[] tasks = new Task[workersCount];
            for(int i = 0; i < workersCount; i++)
            {
                tasks[i] = Task.Factory.StartNew(async  (state) => {
                    int currentTimer = (int)state! * timer;
                    await Task.Delay(timer);
                    Console.WriteLine($"Task slept for {currentTimer} in Thread {Thread.CurrentThread.ManagedThreadId}");
                }, 
                i,
                CancellationToken.None,
                TaskCreationOptions.DenyChildAttach,
                TaskScheduler.Default)
                .Unwrap();
            }

            Task allTasks = Task.WhenAll(tasks);

            try
            {
                Console.WriteLine($"2. WhenAll tasks to complete");

                await allTasks;
            }
            catch(Exception ex)
            {
                Console.WriteLine("One or more exceptions occurred");
                if (allTasks.Exception != null)
                {
                    foreach (var e in allTasks.Exception.Flatten().InnerExceptions)
                    {
                        Console.WriteLine(e.Message);
                    }
                }                    
            }

            Console.WriteLine($"3. Display Tasks statuses");
            foreach (Task t in tasks)
            {
                Console.WriteLine($"Task {t.Id} {t.Status}");
            }            
        }

        internal static async Task RunWhenAllTasksInDirectory(string[] dirNames)
        {
            Console.WriteLine($"1. Create ConcurrentBag<string>");
            ConcurrentBag<string> bag = [];

            Console.WriteLine($"2. Create a Task to handle each Directory");
            List<Task> tasks = [];

            foreach(string directory in dirNames)
            {
                string currentDir = directory;

                Task t = Task.Run(() =>
                {
                    try
                    {
                        foreach (string path in Directory.GetFiles(currentDir))
                        {
                            bag.Add(path);
                            Console.WriteLine($"Task {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} Directory: {currentDir} Path: {path}");
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($"[Error] Failed to process directory {currentDir}: {ex.Message}");
                    }                    
                });

                tasks.Add(t);
            }

            Console.WriteLine($"3. Wait for all tasks to complete - Task.WhenAll");
            await Task.WhenAll(tasks);

            Console.WriteLine($"4. Display each task Status");
            foreach (Task t in tasks)
            {
                Console.WriteLine($"Task {t.Id} Status: {t.Status}");
            }

            Console.WriteLine($"5. Display {bag.Count} files");
            int i = 1;
            while (bag.TryTake(out string? result))
            {
                Console.WriteLine($"{i}.- {result}");
                i++;
            }
        }

        internal static void RunTaskCanceledException(int timer, int cancelationTime, int workersCount)
        {
            Console.WriteLine($"1. Create CancellationTokenSource and get Token");
            CancellationTokenSource cts = new();
            CancellationToken cancellationToken = cts.Token;

            Console.WriteLine($"2. Create {workersCount} Tasks");
            Task[] tasks = new Task[workersCount];
            for(int i = 0; i < workersCount; i++)
            {
                switch (i % 4)
                {
                    case 0:
                        tasks[i] = Task.Run(() => Thread.Sleep(timer));
                        break;
                    case 1:
                        tasks[i] = Task.Run(() => Thread.Sleep(timer), cancellationToken);
                        break;
                    case 2:
                        tasks[i] = Task.Run(() => { throw new NotSupportedException(); });
                        break;
                    default:
                        tasks[i] = Task.Run(() =>
                        {
                            Thread.Sleep(timer);
                            if (cancellationToken.IsCancellationRequested)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                            }
                            Thread.Sleep(timer);
                        }, cancellationToken);
                        break;

                }
            }

            cts.CancelAfter(cancelationTime);

            try
            {
                Console.WriteLine($"3. WaitAll tasks");
                Task.WaitAll(tasks);
            }
            catch(AggregateException aex)
            {
                Console.WriteLine("3.1 One or more exceptions occurred");
                foreach(var ex in aex.Flatten().InnerExceptions)
                {
                    Console.WriteLine($"{ex.GetType().Name}: {ex.Message}");
                }
            }

            Console.WriteLine($"4. Status of tasks:");
            foreach(Task task in tasks)
            {
                Console.WriteLine($"Task #{task.Id}, {task.Status}");
                if (task.Exception != null)
                {
                    foreach(Exception e in task.Exception.InnerExceptions)
                    {
                        Console.WriteLine($"{e.GetType().Name}: {e.Message}");
                    }
                }
            }

        }

        internal static async Task RunParallelTasks(string dir, CancellationTokenSource cancellationTokenSource)
        {
            Console.WriteLine($"1. Create List<Tuple<string, string, long, DateTime>>");
            List<(string, string?, long, DateTime)> files = [];

            Console.WriteLine($"2. Create Task to record file info from all files in the {dir} directory in Parrallel");
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Task t = Task.Run(() =>
            {
                object obj = new object();

                if (Directory.Exists(dir))
                {
                    ParallelOptions parallelOptions = new ParallelOptions { CancellationToken = cancellationToken };
                    Parallel.ForEach(Directory.GetFiles(dir),  f =>
                    {
                        if (cancellationToken.IsCancellationRequested)
                            cancellationToken.ThrowIfCancellationRequested();

                        var fi = new FileInfo(f);
                        lock (obj)
                        {
                            files.Add((fi.Name, fi.DirectoryName, fi.Length, fi.LastWriteTimeUtc));
                        }
                    });
                }
            });

            try
            {
                //cancellationTokenSource.Cancel();
                await t;
                Console.WriteLine($"Retrieved information for {files.Count} files.");
            }
            catch(AggregateException ex)
            {
                Console.WriteLine("Exception messages:");
                foreach(var ie in ex.InnerExceptions)
                {
                    Console.WriteLine($"{ie.GetType().Name}: {ie.Message}");
                }
                Console.WriteLine($"Task status: {t.Status}");
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
        }
    }
}

