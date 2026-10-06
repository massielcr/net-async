using System.Collections.Concurrent;
using System.Diagnostics;

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

        internal static async Task RunScramblesWords(string[] words)
        {
            Console.WriteLine($"1. Create Tasks to scramble each word on the input array");
            List<Task> tasks = [];
            foreach(string word in words)
            {
                Task t = Task.Run(() =>
                {
                    char[] chars = word.ToCharArray();
                    double[] order = new double[chars.Length];
                    for (int i = 0; i < order.Length; i++)
                    {
                        order[i] = Random.Shared.NextDouble();
                    }
                    Array.Sort(order, chars);
                    Console.WriteLine($"{word} --> {new String(chars)}");
                });

                tasks.Add(t);
            }

            Console.WriteLine($"2. Task.WhenAll tasks");
            await Task.WhenAll(tasks);
        }

        internal static async Task RunMyDocumentsFilesTasks()
        {
            Console.WriteLine($"1. Get MyDocuments directory path");
            string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            Console.WriteLine($"2. Create task1 to get files and task2 to get subdirectories");           

            Task<string[]> fileTask = Task.Run(() => Directory.GetFiles(myDocumentsPath));
            Task<string[]> subdirectoryTask = Task.Run(() => Directory.GetDirectories(myDocumentsPath));

            Console.WriteLine($"3. Create continuation task to list files and subdirectories");

            try
            {
                await Task.WhenAll(fileTask, subdirectoryTask);

                string[] files = await fileTask;
                string[] subdirectories = await subdirectoryTask;

                Console.WriteLine($"{myDocumentsPath} contains:");

                foreach (var path in subdirectories)
                {
                    Console.WriteLine($"Subdirectory: {path}");
                }

                foreach (var path in files)
                {
                    Console.WriteLine($"File: {path}");
                }
            }
            catch(UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Denied access to {myDocumentsPath} folder");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }           
        }

        internal static async Task RunRandomDatesContinuationTask()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();

            Console.WriteLine($"1. Create first task to generate random dates");
            Task<DateTime[]> firstTask = Task.Run(() =>
            {
                DateTime[] dates = new DateTime[100];
                byte[] buffer = new byte[8];

                int i = dates.GetLowerBound(0);
                while(i <= dates.GetUpperBound(0))
                {
                    long ticks = Random.Shared.NextInt64(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks);
                    dates[i] = new DateTime(ticks);
                    i++;
                }

                return dates;
            });

            Console.WriteLine($"2. Create continuation task to get earliest and latest dates");
            Task continuationTask = firstTask.ContinueWith((completedTask) =>
            {
                DateTime[] dates = completedTask.Result;
                DateTime earliest = dates[0];
                DateTime latest = earliest;

                for (int i = dates.GetLowerBound(0) + 1; i <= dates.GetUpperBound(0); i++)
                {
                    if (dates[i] < earliest) { earliest = dates[i]; }
                    if (dates[i] > latest) { latest = dates[i]; }
                }

                Console.WriteLine($"Earliest date is {earliest}");
                Console.WriteLine($"Latest date is {latest}");

            });

            await continuationTask;

            stopwatch.Stop();
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds}ms");
        }

        internal static async Task RunContinueWithSynchronously()
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            
            int counter = 0;

            Console.WriteLine($"1. Create first task to increment counter");
            Task firstTask = Task.Run(() =>
            {
                Interlocked.Increment(ref counter);
                Console.WriteLine($"Incremented counter by Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");
            });

            Console.WriteLine($"2. Create continuation task to decrement counter");
            Task continuationTask = firstTask.ContinueWith((completedTask) =>
            {
                Interlocked.Decrement(ref counter);
                Console.WriteLine($"Decremented counter by Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");
            }, 
            TaskContinuationOptions.ExecuteSynchronously);


            await continuationTask;

            Console.WriteLine($"Counter: {counter}");

            stopwatch.Stop();
            Console.WriteLine($"{stopwatch.ElapsedMilliseconds}ms");
        }

        internal static async Task RunContinueWithContinuationOptions()
        {
            Console.WriteLine($"1. Create success Task");
            Action success = () =>
            {
                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} - Begin successful transaction");
            };

            Console.WriteLine($"2. Create failure Task");
            Action failure = () =>
            {
                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} - Begin transaction an encountered an error");
                throw new InvalidOperationException("An error occurred");
            };

            Console.WriteLine($"3. Create commit Task");
            Action<Task> commit = (antecedent) =>
            {
                Console.WriteLine($"Task: {Task.CurrentId} Thread:{Environment.CurrentManagedThreadId} - Commit transaction");
            };

            Console.WriteLine($"4. Create rollback Task");
            Action<Task> rollback = (antecedent) =>
            {
                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} - Rollback Transaction");
            };


            Console.WriteLine($"5. Start continuation after successful task");
            Task successTask = Task.Run(success);
            Task successCommitTask = successTask.ContinueWith(commit, TaskContinuationOptions.OnlyOnRanToCompletion);
            Task successRollbackTask = successTask.ContinueWith(rollback, TaskContinuationOptions.NotOnRanToCompletion);

            try
            {
                await Task.WhenAll(successCommitTask, successRollbackTask);
            }
            catch (TaskCanceledException ex)
            {
                string cancelledTask = successCommitTask.IsCanceled ? nameof(successCommitTask) : nameof(successRollbackTask);
                Console.WriteLine($"Error: {ex.Message} Task: {cancelledTask}");
            }

            Console.WriteLine($"6. Start continuation after failure task");
            Task failureTask = Task.Run(failure);
            Task failureCommitTask = failureTask.ContinueWith(commit, TaskContinuationOptions.OnlyOnRanToCompletion);
            Task failureRollbackTask = failureTask.ContinueWith(rollback, TaskContinuationOptions.NotOnRanToCompletion);

            try
            {
                await Task.WhenAll(failureCommitTask, failureRollbackTask);
            }
            catch(TaskCanceledException ex)
            {
                string cancelledTask = failureCommitTask.IsCanceled ? nameof(failureCommitTask) : nameof(failureRollbackTask);
                Console.WriteLine($"Error: {ex.Message} Task: {cancelledTask}");
            }           
        }

        internal static async Task RunContinuationDifferentScenariosTasks()
        {
            Console.WriteLine($"1. Create an action that prints a string");
            Action<string> action = (str) =>
            {
                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} - {str}");
            };

            Console.WriteLine($"2. Create a function that negates the previous result");
            Func<int, int> negate = (n) =>
            {
                int output = -n;

                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} input: {n} output: {output}");

                return output;
            };

            Console.WriteLine($"3. Sequence of unrelated input tasks ");
            Task alphaTask = Task.Run(() => action("alpha"));
            Task betaTask = alphaTask.ContinueWith(antecedent => action("beta"));
            Task gammaTask = betaTask.ContinueWith(antecedent => action("gamma"));

            await gammaTask;            


            Console.WriteLine($"4. Sequence of dependent tasks ");
            Task dependentChainTask = Task.Run(() => negate(5))
                                      .ContinueWith(antecedent => negate(antecedent.Result))
                                      .ContinueWith(antecedent => negate(antecedent.Result));

            await dependentChainTask;


            Console.WriteLine($"5. Sequence of tasks executing different unrelated actions");
            Task mixedChainTask = Task.Run(() => negate(6))
                                  .ContinueWith(_ => action("x"))
                                  .ContinueWith(_ => negate(7));

            await mixedChainTask;
        }

        internal static async Task RunDelay()
        {
            Console.WriteLine($"1. Create a Task that contains a Task with a 1s delay");
            async Task<int> GetValueAsync()
            {
                Console.WriteLine($"Thread: {Environment.CurrentManagedThreadId} - ready to delay");

                await Task.Delay(1000);

                return 42;
            };

            Task<int> task = GetValueAsync();

            Console.WriteLine($"2. Display Result");

            await task;

            Console.WriteLine($"Task: {task.Id} Status: {task.Status} Result: {task.Result}");
        }

        internal static async Task RunDelayContinueWith()
        {
            Console.WriteLine($"1. Create a Task with a 1s delay and a continuation task");
            Stopwatch stopwatch = Stopwatch.StartNew();
            Task<long> task = Task.Delay(1000).ContinueWith(_ =>
            {
                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            });

            Console.WriteLine($"2. Display Result");

            await task;
            Console.WriteLine($"Elapsed milliseconds: {task.Result}");
        }

        internal static async Task RunDelainInternalWatch()
        {
            Console.WriteLine($"1. Create a Task with a child task with a 1s delay");
            Task<long> task = Task.Run(async () =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                await Task.Delay(1000);

                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            });


            Console.WriteLine($"2. Display Result");
            long result = await task;
            Console.WriteLine($"Elapsed milliseconds: {result}");
        }

        internal static async Task RunDelayTimeSpan()
        {
            Console.WriteLine($"1. Create a Task with a child task with a 1.5s delay");
            Task<int> task = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1.5));
                return 42;
            });

            Console.WriteLine($"2. Display Result");
            await task;
            Console.WriteLine($"Status: {task.Status} Result: {task.Result}");
        }

        internal static async Task RunDelayWithCancellation()
        {
            Console.WriteLine($"1. Create CancellationTokenSource");
            CancellationTokenSource cancellationTokenSource = new();

            Console.WriteLine($"2. Create a Task with a child task with 1s delay and a Cancellation token");
            Task<int> task = Task.Run(async () =>
            {
                Console.WriteLine($"Running Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");

                await Task.Delay(1000, cancellationTokenSource.Token);

                return 42;
            });

            Console.WriteLine($"3. Trigger a Cancellation after 0.5s");
            cancellationTokenSource.CancelAfter(500);
            try
            {
                await task;
            }
            catch(OperationCanceledException ex)
            {
                Console.WriteLine($"OperationCanceledException: {ex.Message}");
            }
            catch(AggregateException ex)
            {
                Console.WriteLine($"AggregateException: {ex.Message}");
                foreach(Exception ie in ex.InnerExceptions)
                {
                    Console.WriteLine($"{ie.GetType().Name}: {ie.Message}");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            Console.WriteLine($"4. Display Results");
            Console.WriteLine($"Status: {task.Status} Result: {(task.Status == TaskStatus.RanToCompletion ? task.Result : -1)}");
        }

        internal static async Task RunDelayTimeSpanWithCancellation()
        {
            Console.WriteLine($"1. Create CancellationTokenSource");
            CancellationTokenSource cancellationTokenSource = new();

            Console.WriteLine($"2. Create a Task with a child task with 1s delay from a TimeSpan and a Cancellation token");
            Task<int> task = Task.Run(async () =>
            {
                Console.WriteLine($"Running Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationTokenSource.Token);

                return 42;
            });


            Console.WriteLine($"3. Trigger a Cancellation after 0.5s");
            cancellationTokenSource.CancelAfter(500);

            try
            {
                await task;
            }
            catch(AggregateException ex)
            {
                Console.WriteLine($"AggregateException: {ex.Message}");
                foreach(Exception ie in ex.InnerExceptions)
                {
                    Console.WriteLine($"{ie.GetType().Name}: {ie.Message}");
                }
            }
            catch(OperationCanceledException ex)
            {
                Console.WriteLine($"OperationCanceledException: {ex.Message}");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            Console.WriteLine($"4. Display Results");
            Console.WriteLine($"Status: {task.Status} Result: {(task.Status == TaskStatus.RanToCompletion ? task.Result : -1)}");
        }

        internal static async Task RunFromExceptionTask(string directoryPath)
        {
            Console.WriteLine($"1. Create a Task that will call the GetFilesLengthAsync method and return a Task.FromException");
            Task<long> task = GetFilesLengthAsync(directoryPath);

            Console.WriteLine($"2. Display Results");
            try
            {
                await task;
                Console.WriteLine($"Status: {task.Status} Result: {(task.Status == TaskStatus.RanToCompletion ? task.Result : -1)}");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        internal static async Task RunFromResultTask(string directoryPath)
        {
            Console.WriteLine($"1. Create a Task that will call the GetFilesLengthAsync method and return a Task.FromResult");
            Task<long> task = GetFilesLengthAsync(directoryPath);

            Console.WriteLine($"2. Display Results");
            try
            {
                long result = await task;
                Console.WriteLine($"Status: {task.Status} Result: {(task.Status == TaskStatus.RanToCompletion ? result : -1)}");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        private static Task<long> GetFilesLengthAsync(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return Task.FromException<long>(new DirectoryNotFoundException("Invalid directory name"));
            }

            string[] files = Directory.GetFiles(directoryPath);

            if (!files.Any())
            {
                return Task.FromResult<long>(0);
            }

            return Task.Run(() =>
            {
                long total = 0;

                Parallel.ForEach(files, (fileName) =>
                {
                    FileStream fileStream = new(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 256, true);
                    Interlocked.Add(ref total, fileStream.Length);
                    fileStream.Close();
                });

                return total;
            });

        }

        public static async Task RunActionTask()
        {
            Console.WriteLine($"1. Create an Action that prints an string");
            Action<string> action = (input) =>
            {
                Console.WriteLine($"{input} - Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");
            };

            Console.WriteLine($"2. Run the action on the main thread");
            action("Application");


            Console.WriteLine($"3. Run a Task and pass the Action to it");
            await Task.Run(() => action("Task"));
        }

        public static async Task RunLambdaTask()
        {
            Console.WriteLine($"1. Run the function on the main thread");
            Console.WriteLine($"Application - Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");


            Console.WriteLine($"2. Run a Task and pass a lambda to it");
            await Task.Run(() => {
                Console.WriteLine($"Task - Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");
            });
        }
    }
}

