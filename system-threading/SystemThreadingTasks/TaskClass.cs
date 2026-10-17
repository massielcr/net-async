using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;

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
            Task t1 = new(action!, "t1");
            t1.Start();

            Console.WriteLine($"3. Create Task by calling Task.Factory.StartNew");
            Task t2 = Task.Factory.StartNew(action!, "t2");

            Console.WriteLine($"4. Create Task by calling Task.Run(Action)");
            Task t3 = Task.Run(() => action("t3"));

            Console.WriteLine($"5. Create Task by calling a Task constructor and run it synchronously");
            Task t4 = new(action!, "t4");
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
                Console.WriteLine($"An exception occurred. ex: {ex.Message}");
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
                Console.WriteLine($"One or more exceptions occurred. ex: {ex.Message}");
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
                Console.WriteLine($"Denied access to {myDocumentsPath} folder. ex: {ex.Message}");
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

        public static async Task RunConcurrentBag(string[] directories)
        {
            Console.WriteLine($"1.Create the ConcurrentBag<T>");
            ConcurrentBag<string> bag = [];

            Console.WriteLine($"2.Create a Task to handle each directory from the input");
            Task[] tasks = new Task[directories.Length];
            for(int i = 0; i < directories.Length; i++)
            {
                int locali = i;

                tasks[i] = Task.Run(() =>
                {
                    Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} i:{locali}");

                    foreach(string path in Directory.GetFiles(directories[locali]))
                    {
                        bag.Add(path);
                    }
                });
            }

            await Task.WhenAll(tasks);

            Console.WriteLine($"3. Display Results");            
            Console.WriteLine("Tasks:");
            foreach(Task t in tasks)
            {
                Console.WriteLine($"- Task: {t.Id} Status: {t.Status}");
            }

            Console.WriteLine("Paths:");
            while (bag.TryTake(out string? path))
            {
                Console.WriteLine($"- {path}");
            }
        }

        public static async Task RunFuncWordCounter(string[] fileNames)
        {
            string pattern = @"\p{P}*\s+";

            Console.WriteLine($"1.Create a Task to handle each directory from the input");
            Task<int>[] tasks = new Task<int>[fileNames.Length];
            for (int i = 0; i < fileNames.Length; i++)
            {
                string fileName = $"{fileNames[i]}.txt";

                tasks[i] = Task.Run(async () =>
                {
                    int result = 0;

                    Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} file: {fileName}");

                    if (File.Exists(fileName))
                    {
                        using StreamReader streamReader = new(fileName);
                        string text = await streamReader.ReadToEndAsync();
                        result = Regex.Matches(text, pattern).Count;
                    }

                    return result;
                });
            }

            await Task.WhenAll(tasks);

            Console.WriteLine($"2. Display Results");
            for(int i = 0; i < tasks.Length; i++)
            {
                Console.WriteLine($"- Task: {tasks[i].Id} {fileNames[i]}: {await tasks[i]}");
            }
        }

        public static async Task RunThrowIfCancellationRequested(int workersCount, int iterationsCount)
        {
            Console.WriteLine("1. Create the CancellationTokenSource");
            CancellationTokenSource cancellationTokenSource = new();
            CancellationToken token = cancellationTokenSource.Token;

            Console.WriteLine($"2. Create {workersCount} Tasks and pass CancellationToken");
            Task<int>[] tasks = new Task<int>[workersCount];

            int completedIterations = 0;

            for(int i = 0; i < workersCount; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    int iterations = 0;

                    Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");

                    for(int j = 1; j <= iterationsCount; j++)
                    {
                        token.ThrowIfCancellationRequested();
                        iterations++;
                    }

                    int currentCompleted = Interlocked.Increment(ref completedIterations);

                    if (currentCompleted >= 5)
                    {
                        cancellationTokenSource.Cancel();
                    }

                    return iterations;
                }, token);
            }

            Console.WriteLine($"3. Process Tasks");
            try
            {
                await Task.WhenAll(tasks);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            Console.WriteLine($"4. Display Results");
            Console.WriteLine("{0,10} {1,20} {2,14:N0}", "Task Id", "Status", "Iterations");
            foreach (Task<int> t in tasks)
            {
                Console.WriteLine("{0,10} {1,20} {2,14:N0}", t.Id, t.Status, (t.Status == TaskStatus.RanToCompletion) ? t.Result : 0);
            }
        }

        public static async Task RunIsCancellationRequested(int workersCount, int iterationsCount)
        {
            Console.WriteLine("1. Create the CancellationTokenSource");
            CancellationTokenSource cancellationTokenSource = new();
            CancellationToken token = cancellationTokenSource.Token;


            Console.WriteLine($"2. Create {workersCount} Tasks and pass CancellationToken");
            int completedIterations = 0;

            Task<int>[] tasks = new Task<int>[workersCount];
            for(int i = 0; i < workersCount; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    int iterations = 0;
                    Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");

                    for(int j = 1; j <= iterationsCount; j++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            return iterations;
                        }
                        iterations++;
                    }

                    int currentcompletedIterations = Interlocked.Increment(ref completedIterations);
                    if (currentcompletedIterations >= 5)
                    {
                        cancellationTokenSource.Cancel();
                    }

                    return iterations;
                }, token);
            }


            Console.WriteLine($"3. Process Tasks");
            try
            {
                await Task.WhenAll(tasks);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }

            Console.WriteLine($"4. Display Results");
            Console.WriteLine("{0,10} {1,20} {2,14:N0}", "Task Id", "Status", "Iterations");
            foreach(Task<int> t in tasks)
            {
                Console.WriteLine("{0,10} {1,20} {2,14:N0}", t.Id, t.Status, t.Status == TaskStatus.RanToCompletion ? t.Result : 0);
            }
        }

        public static async Task RunSynchronously(int iterationsCount)
        {
            Console.WriteLine($"Current Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine("1. Create Async Task");
            Task<long> asyncTask = Task.Run(() =>
            {
                long result = 0;
                Console.WriteLine($"Async Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");

                for(int i = 0; i < iterationsCount; i++)
                {
                    result += i;
                }

                return result;
            });

            Console.WriteLine("2. Create Sync Task");
            Task<long> syncTask = new Task<long>(() =>
            {
                long result = 0;
                Console.WriteLine($"Sync Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");

                for (int i = 0; i < iterationsCount; i++)
                {
                    result += i;
                }

                return result;
            });

            syncTask.RunSynchronously();
            await asyncTask;

            Console.WriteLine("3. Display Results");
            Console.WriteLine($"- Async Task: {asyncTask.Id} Result: {asyncTask.Result}");
            Console.WriteLine($"- Sync Task: {syncTask.Id} Result: {syncTask.Result}");
        }

        public static async Task RunStart(int iterationsCount)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine("1. Create a Task");

            Task task = new Task(() =>
            {                
                for(int i = 0; i < iterationsCount; i++)
                {
                    Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} i: {i}");
                }

            });

            Console.WriteLine("2. Start and Wait the Task");
            task.Start();
            await task;
        }

        public static async Task RunWaitTimeoutAndCancellationToken(int taskDelay, int cancelDelay, int waitTimeout)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine($"1. Create a Thread passing a CancellationTokenSource and a timeout of {cancelDelay}ms");
            CancellationTokenSource cancellationTokenSource = new();
            Thread thread = new Thread(() => CancelToken(cancellationTokenSource, cancelDelay));
            thread.Start();

            Console.WriteLine($"Start a task with a Delay of {taskDelay}");
            Task task = Task.Run(() =>
            {
                Thread.Sleep(taskDelay);

                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} delay ended");
            });

            try
            {                
                bool result = task.Wait(waitTimeout, cancellationTokenSource.Token);

                Console.WriteLine($"Task: {task.Id} Result: {result}");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"OperationCanceledException: {ex.Message}");
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
        }

        private static void CancelToken(object obj, int cancelDelay)
        {
            Thread.Sleep(cancelDelay);
            Console.WriteLine($"Cancelling token from Thread {Environment.CurrentManagedThreadId}");
            CancellationTokenSource source = obj as CancellationTokenSource;
            if (source is not null) { source.Cancel(); }
        }

        public static async Task RunWaitTimeSpan(int iterationCount, int timeout)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine($"1. Create a Task");
            Task task = Task.Run(() =>
            {
                long sum = 0;
                for(int i = 0; i < iterationCount; i++)
                {
                    sum += Random.Shared.Next(0, 10);
                }

                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} sum: {sum}");

            });

            Console.WriteLine($"2. Wait for the Task {timeout}ms and Display the Results");
            if (!task.Wait(TimeSpan.FromMilliseconds(timeout)))
            {
                Console.WriteLine("The timeout interval elapsed.");
            }
        }

        public static async Task RunWaitCancellationToken(int taskDelay)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine("1. Create a CancellationTokenSource");
            using CancellationTokenSource cancellationTokenSource = new();
            CancellationToken token = cancellationTokenSource.Token;

            Console.WriteLine("2. Create a Task without passing a CancellationToken and Cancel the source inside it");
            Task task = Task.Run(() =>
            {
                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} calling Cancel");
                cancellationTokenSource.Cancel();
                Thread.Sleep(taskDelay);
                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} ended delay");

            });

            Console.WriteLine("3. Wait for the Task passing a CancellationToken");
            try
            {
                Console.WriteLine("About to wait for the task");
                task.Wait(token);                
            }
            catch(OperationCanceledException ex)
            {
                Console.WriteLine($"OperationCanceledException: {ex.Message}");
            }

            await task;
        }

        public static async Task RunWait(int iterationCount)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine($"1. Create a Task to process {iterationCount} items ");
            Task task = Task.Run(() =>
            {
                for(int i = 0; i < iterationCount; i++)
                {
                    Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} i: {i}");
                }
            });

            task.Wait();
            await task;

            Console.WriteLine("2. Task ended");

        }

        public static async Task RunWaitTimeout(int iterationCount, int timeout)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Stopwatch sw = Stopwatch.StartNew();

            Console.WriteLine($"1. Create a Task to process {iterationCount} items");
            Task task = Task.Run(() =>
            {
                for (int i = 0; i < iterationCount; i++)
                {
                    if (sw.ElapsedMilliseconds >= timeout)
                    {
                        Console.WriteLine($"Task: {Task.CurrentId} aborted early because timeout passed.");
                        break;
                    }

                    Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} i: {i}");
                    Thread.Sleep(100);
                }
            });

            Console.WriteLine($"2. Wait for the Task for {timeout}ms");
            if (!task.Wait(timeout))
            {
                Console.WriteLine("The timeout interval elapsed.");
            }                
            else
            {
                Console.WriteLine("Task ended.");
            }

            await task;
        }            

        public static async Task RunWaitAll(int workersCount)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine("1. Create a Func<T,TResult>");
            Func<object, int> func = (object obj) =>
            {
                int i = (int)obj;

                Thread.Sleep(i * 100);

                if (2 <= i && i <= 5)
                {
                    throw new InvalidOperationException($"SIMULATED EXCEPTION Task: {Task.CurrentId} i: {i}");
                }

                int tikCount = Environment.TickCount;

                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} i: {i}");

                return tikCount;
            };

            Console.WriteLine($"2. Create {workersCount} taks");
            Task<int>[] tasks = new Task<int>[workersCount];
            for (int i= 0; i < workersCount; i++)
            {
                int index = i;
                tasks[i] = Task<int>.Factory.StartNew(func!, index);
            }

            Console.WriteLine($"3. Wait for all tasks");
            try
            {
                Task.WaitAll(tasks);
                Console.WriteLine("Task completed");
            }
            catch(AggregateException ex)
            {
                Console.WriteLine($"AggregateException: {ex.Message}");
                foreach(Exception ie in ex.InnerExceptions)
                {
                    Console.WriteLine($"- {ie.Message}");
                }
            }

            Console.WriteLine($"3. Display Results");
            foreach(Task<int> t in tasks)
            {
                Console.WriteLine($"Task: {t.Id} Status: {t.Status} Result: {(t.Status == TaskStatus.RanToCompletion ? t.Result : -1)}");
            }
        }

        public static async Task RunWaitAny(int workersCount)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine($"1. Create {workersCount} tasks");
            Task[] tasks = new Task[workersCount];
            for(int i = 0; i < workersCount; i++)
            {
                int current = i;
                tasks[i] = Task.Run(() =>
                {
                    Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} i: {current}");
                    Thread.Sleep(current * 250);
                });
            }

            Console.WriteLine("2. Wait for any task");
            int index = Task.WaitAny(tasks);

            Console.WriteLine("3. Display the Resulst");
            foreach(Task t in tasks)
            {
                Console.WriteLine($"Task: {t.Id} Status: {t.Status}");
            }
        }

        public static async Task RunWhenAll(string[] urls)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            int failed = 0;

            Console.WriteLine("1. Create a Task to handle each url");
            Task[] tasks = new Task[urls.Length];
            for(int i = 0; i < urls.Length; i++)
            {
                string url = urls[i];
                tasks[i] = Task.Run(() =>
                {
                    Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} url: {url}");

                    Ping png = new();
                    try
                    {
                        var reply = png.Send(url);
                        if(!(reply.Status == IPStatus.Success))
                        {
                            Interlocked.Increment(ref failed);
                            throw new TimeoutException($"Unable to reach {url}.");
                        }
                    }
                    catch(PingException ex)
                    {
                        Interlocked.Increment(ref failed);
                    }
                });
            }

            Console.WriteLine("2. Await WhenAll tasks");
            try
            {
                await Task.WhenAll(tasks);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            Console.WriteLine("3. Display the Resuls");
            foreach(Task t in tasks)
            {
                Console.WriteLine($"Task: {t.Id} Status: {t.Status}");
            }
            Console.WriteLine($"{failed} failed");
        }

        public static async Task RunWhenAllDelay(int workersCount, int iterationCount)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine($"1. Create {workersCount} Tasks");
            Task<long>[] tasks = new Task<long>[workersCount];
            for(int i = 0; i < workersCount; i++)
            {
                int delayInterval = 18 * i;
                tasks[i] = Task.Run(() =>
                {
                    Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} delayInterval: {delayInterval}");

                    long total = 0;
                    Thread.Sleep(delayInterval);

                    for(int j = 0; j < iterationCount; j++)
                    {
                        total += Random.Shared.Next(1, 15);
                    }

                    return total;
                });
            }

            Console.WriteLine("2. Wait for WhenAll");
            try
            {
                Task tResult = Task.WhenAll(tasks);
                await tResult;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception {ex.Message}");
            }

            Console.WriteLine("3. Display the Results");
            foreach(Task<long> t in tasks)
            {
                Console.WriteLine($"Task: {t.Id} Status: {t.Status} Result: {t.Result}");
            }
        }

        public static async Task<int> RunFunc(int intervalCount)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine("1. Create a Task<TResult>");

            Task<int> task = Task<int>.Run(() =>
            {
                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");

                int counter = 0;

                for(int i = 0; i < intervalCount; i++)
                {
                    if (counter == intervalCount /2 && DateTime.Now.Hour <= 12)
                    {
                        break;
                    }
                    counter++;
                }

                return counter;
            });

            Console.WriteLine("2. Await the Task<TResult>");
            int result = await task;

            Console.WriteLine("3. Return the Result");
            Console.WriteLine($"Task: {task.Id} Status: {task.Status} Result: {result}");

            return result;
        }

        public static async Task<int> RunFactoryFunc(int intervalCount)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine("1. Start a Task<TResult> using the Factory");
            Task<int> task = Task<int>.Factory.StartNew(() =>
            {
                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");

                int counter = 0;
                for(int i =0; i < intervalCount; i++)
                {
                    if (counter >= intervalCount/2 && DateTime.Now.Hour <= 12)
                    {
                        break;
                    }
                    counter++;
                }

                return counter;
            });

            Console.WriteLine("2. Await the Task<TResult>");
            int result = await task;


            Console.WriteLine("3. Return the TResult");
            Console.WriteLine($"Task: {task.Id} Status: {task.Status} Result: {result}");

            return result;
        }

        public static async Task<int[]> RunWordCounter(string[] titles)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            string pattern = @"\p{P}*\s+";

            Console.WriteLine("1. Create a Task to process each title from the input");
            IEnumerable<Task<int>> taskQuery = titles.Select(title =>
                Task<int>.Run(async () =>
                {
                    string currentFile = $"{title}.txt";

                    Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId} File: {currentFile}");

                    if (!File.Exists(currentFile))
                    {
                        Console.WriteLine("File not found");
                        return 0;
                    }

                    using StreamReader streamReader = new(currentFile);
                    string content = await streamReader.ReadToEndAsync();

                    return Regex.Matches(content, pattern).Count;
                }));            

            Console.WriteLine("2. Await for all the Tasks");
            Task<int>[] tasks = taskQuery.ToArray();
            await Task.WhenAll(tasks);

            Console.WriteLine("3. Return the results");
            int[] result = new int[titles.Length];
            for(int j = 0; j < titles.Length; j++)
            {
                result[j] = await tasks[j];
                Console.WriteLine($"- Task: {tasks[j].Id} {titles[j]} : {result[j]}");
            }

            return result;
        }

        public static async Task RunFactoryDifferentTResult()
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine("1.1 Create a Task<TResult> that returns an int");
            Task<int> taskInt = new Task<int>(() =>
            {
                Console.WriteLine($"Taks: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");
                return 1;
            });

            Console.WriteLine("1.2 Start and await the Task<TResult> that returns an int");
            taskInt.Start();
            await taskInt;

            Console.WriteLine("1.3 Display Results from the Task<TResult> that returns an int");
            Console.WriteLine($"Task: {taskInt.Id} Status: {taskInt.Status} Result: {taskInt.Result}");            



            Console.WriteLine("2.1. Create a Task<TResult> that returns a Test");
            Task<Test> taskTest = Task<Test>.Factory.StartNew(() =>
            {
                Console.WriteLine($"Taks: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");
                return new Test { Name = ".NET", Number = 4.0 };
            });

            Console.WriteLine("2.2 Await the Task<TResult> that returns a Test");
            await taskTest;

            Console.WriteLine("2.3 Display Results from the Task<TResult> that returns a Test");
            Console.WriteLine($"Task: {taskTest.Id} Status: {taskTest.Status} Result: {taskTest.Result.Name}-{taskTest.Result.Number}");



            Console.WriteLine("3.1 Create a Task<TResult> that returns a string[]");
            Task<string[]> taskArray = Task<string[]>.Run(() =>
            {
                Console.WriteLine($"Taks: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");

                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                string[] files = Directory.GetFiles(path);

                string[] pics = files.Select(f =>
                                    {
                                        FileInfo file = new FileInfo(f);
                                        return (file.Name, file.Extension);
                                    })
                                     .Where(f => f.Extension == ".png").Select(f => f.Name).ToArray();

                return pics;
            });

            Console.WriteLine("3.2 Await the Task<TResult> that returns a string[]");
            await taskArray;

            Console.WriteLine("3.3 Display Results from the Task<TResult> that returns a string[]");
            foreach(string s in taskArray.Result)
            {
                Console.WriteLine($"- {s}");
            }
        }

        public static async Task RunFuncContinueWithCancellationToken(int upperBound, int timeout)
        {
            Console.WriteLine($"Main Thread: {Environment.CurrentManagedThreadId}");

            Console.WriteLine("1. Create a CancellationTokenSource");
            using CancellationTokenSource cancellationTokenSource = new();
            CancellationToken cancellationToken = cancellationTokenSource.Token;

            Console.WriteLine("2. Create a Timer to trigger a Cancellation");
            System.Timers.Timer timer = new(timeout);
            timer.Elapsed += (source, e) =>
            {
                cancellationTokenSource.Cancel();
                Console.WriteLine("Timer elapsed. Cancellation requested.");
                timer.Dispose();
            };
            timer.AutoReset = false;
            timer.Enabled = true;


            Console.WriteLine($"3. Create a Task with {upperBound} iterations passing a CancellationToken");
            Task<bool[]> task = Task.Run(() =>
            {
                Console.WriteLine($"Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");

                bool[] values = new bool[upperBound + 1];

                for(int i = 2; i <= (int)Math.Sqrt(upperBound); i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!values[i])
                    {
                        for(int j = i * i; j <= upperBound; j = j + i)
                        {
                            values[j] = true;
                        }
                    }                    
                }
                return values;
            }, cancellationToken);            


            Console.WriteLine("3. Create a Continuation task passing the CancellationToken");
            Task continuationTask = task.ContinueWith((antecedent) =>
            {
                Console.WriteLine($"Continuation Task: {Task.CurrentId} Thread: {Environment.CurrentManagedThreadId}");
                List<int> primes = [];
                cancellationToken.ThrowIfCancellationRequested();

                bool[] numbers = antecedent.Result;
                StringBuilder output = new();

                for(int i = 1; i <= numbers.GetUpperBound(0); i++)
                {
                    if (!numbers[i]) { primes.Add(i);  }
                }

                for (int j = 0; j < primes.Count; j++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    output.Append(primes[j].ToString("N0"));

                    if (j < primes.Count - 1)
                    {
                        output.Append(", ");
                    }

                    if ((j + 1) % 8 == 0)
                    {
                        output.Append(Environment.NewLine);
                    }

                    Console.WriteLine($"Prime numbers from 1 to {upperBound}:");
                    Console.WriteLine(output.ToString());
                }
            }, cancellationToken);


            Console.WriteLine("4. Await the Continuation task");
            try
            {
                await continuationTask;
            }
            catch(AggregateException ex)
            {
                foreach(Exception ie in ex.InnerExceptions)
                {
                    if (ie.GetType() == typeof(TaskCanceledException))
                    {
                        Console.WriteLine("The operation was cancelled.");
                    }
                    else
                    {
                        Console.WriteLine($"{ie.GetType().Name}: {ie.Message}");
                    }
                }
            }
        }
    }

    internal class Test
    {
        public string Name { get; set; } = string.Empty;
        public double Number { get; set; }
    }
}

