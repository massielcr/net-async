using SystemThreadingTasks;
using SystemThreadingTasks.Enums;

class Program
{
    static async Task Main()
    {
        while(true)
        {            
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("[1]  - Enums - ConfigureAwaitOptions");
            Console.WriteLine("[2]  - Enums - TaskStatus");
            Console.WriteLine("[5]  - Task  - instantiation");
            Console.WriteLine("[6]  - Task  - Task.WaitAny");
            Console.WriteLine("[7]  - Task  - Task.WhenAll");
            Console.WriteLine("[8]  - Task  - TaskCanceledException");
            Console.WriteLine("[9]  - Task  - Task.WhenAll - ConcurrentBag");
            Console.WriteLine("[10] - Task  - Parallel.ForEach");
            Console.WriteLine("[11] - Task  - Scrambles Words");
            Console.WriteLine("[12] - Task  - MyDocuments Files and Subdirectories");
            Console.WriteLine("[13] - Task  - ContinueWith");
            Console.WriteLine("[14] - Task  - ContinueWith TaskContinuationOptions.ExecuteSynchronously");
            Console.WriteLine("[15] - Task  - ContinueWith TaskContinuationOptions");
            Console.WriteLine("[16] - Task  - ContinueWith - Different scenarios");
            Console.WriteLine("[17] - Task  - Delay");
            Console.WriteLine("[18] - Task  - Delay ContinueWith");
            Console.WriteLine("[19] - Task  - Delay child task");
            Console.WriteLine("[20] - Task  - Delay TimeSpan");

            string? key = Console.ReadLine();

            switch (key)
            {
                case "1":
                    await EnumConfigureAwaitOptions.Run();
                    goto default;
                case "2":
                    await EnumTaskStatus.Run();
                    goto default;
                case "3":
                    goto default;
                case "4":
                    goto default;
                case "5":
                    await TaskClass.RunInstantiation();
                    goto default;
                case "6":
                    await TaskClass.RunWhenAnyTasks(500, 3000, 3);
                    goto default;
                case "7":
                    await TaskClass.RunWhenAllTasks(200, 10);
                    goto default;
                case "8":
                    TaskClass.RunTaskCanceledException(2000, 250, 12);
                    goto default;
                case "9":
                    await TaskClass.RunWhenAllTasksInDirectory([".", ".." ]);
                    goto default;
                case "10":
                    CancellationTokenSource cancellationTokenSource = new();
                    await TaskClass.RunParallelTasks("C:\\Windows\\System32\\", cancellationTokenSource);
                    goto default;
                case "11":
                    string[] words = ["reason", "editor", "rioter", "rental", "senior", "regain", "ordain", "rained"];
                    await TaskClass.RunScramblesWords(words);
                    goto default;
                case "12":
                    await TaskClass.RunMyDocumentsFilesTasks();
                    goto default;
                case "13":
                    await TaskClass.RunRandomDatesContinuationTask();
                    goto default;
                case "14":
                    await TaskClass.RunContinueWithSynchronously();
                    goto default;
                case "15":
                    await TaskClass.RunContinueWithContinuationOptions();
                    goto default;
                case "16":
                    await TaskClass.RunContinuationDifferentScenariosTasks();
                    goto default;
                case "17":
                    await TaskClass.RunDelay();
                    goto default;
                case "18":
                    await TaskClass.RunDelayContinueWith();
                    goto default;
                case "19":
                    await TaskClass.RunDelainInternalWatch();
                    goto default;
                case "20":
                    await TaskClass.RunDelayTimeSpan();
                    goto default;
                default:
                    Console.WriteLine();
                    break;
            }
        }
    }    
}


