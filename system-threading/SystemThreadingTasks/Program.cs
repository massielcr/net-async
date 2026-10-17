using System.Runtime.CompilerServices;
using SystemThreadingTasks;
using SystemThreadingTasks.Enums;

class Program
{
    static async Task Main()
    {
        while(true)
        {            
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("[1]  - Enums         - ConfigureAwaitOptions");
            Console.WriteLine("[2]  - Enums         - TaskStatus");
            Console.WriteLine("[5]  - Task          - instantiation");
            Console.WriteLine("[6]  - Task          - Task.WaitAny");
            Console.WriteLine("[7]  - Task          - Task.WhenAll");
            Console.WriteLine("[8]  - Task          - TaskCanceledException");
            Console.WriteLine("[9]  - Task          - Task.WhenAll - ConcurrentBag");
            Console.WriteLine("[10] - Task          - Parallel.ForEach");
            Console.WriteLine("[11] - Task          - Scrambles Words");
            Console.WriteLine("[12] - Task          - MyDocuments Files and Subdirectories");
            Console.WriteLine("[13] - Task          - ContinueWith");
            Console.WriteLine("[14] - Task          - ContinueWith TaskContinuationOptions.ExecuteSynchronously");
            Console.WriteLine("[15] - Task          - ContinueWith TaskContinuationOptions");
            Console.WriteLine("[16] - Task          - ContinueWith - Different scenarios");
            Console.WriteLine("[17] - Task          - Delay");
            Console.WriteLine("[18] - Task          - Delay ContinueWith");
            Console.WriteLine("[19] - Task          - Delay child task");
            Console.WriteLine("[20] - Task          - Delay TimeSpan");
            Console.WriteLine("[21] - Task          - Delay CancellationTokenSource");
            Console.WriteLine("[22] - Task          - Delay TimeSpan CancellationTokenSource");
            Console.WriteLine("[23] - Task          - FromException");
            Console.WriteLine("[24] - Task          - FromResult");
            Console.WriteLine("[25] - Task          - Run Action");
            Console.WriteLine("[26] - Task          - Run Lambda");
            Console.WriteLine("[27] - Task          - Run ConcurrentBag");
            Console.WriteLine("[28] - Task          - Run Func<T> - WordCounter");
            Console.WriteLine("[29] - Task          - Run ThrowIfCancellationRequested");
            Console.WriteLine("[30] - Task          - Run IsCancellationRequested");
            Console.WriteLine("[31] - Task          - RunSynchronously");
            Console.WriteLine("[32] - Task          - Start");
            Console.WriteLine("[33] - Task          - Wait - Timeout & CancellationToken");
            Console.WriteLine("[34] - Task          - Wait - TimeSpan");
            Console.WriteLine("[35] - Task          - Wait - CancellationToken");
            Console.WriteLine("[36] - Task          - Wait");
            Console.WriteLine("[37] - Task          - Wait - Timeout");
            Console.WriteLine("[38] - Task          - WaitAll");
            Console.WriteLine("[39] - Task          - WaitAny");
            Console.WriteLine("[40] - Task          - WhenAll");
            Console.WriteLine("[41] - Task          - WhenAll Delay");
            Console.WriteLine("[42] - Task<TResult> - Run");
            Console.WriteLine("[43] - Task<TResult> - Run Func<T> Factory.StartNew");
            Console.WriteLine("[44] - Task<TResult> - Run Func<T> WordCounter");
            Console.WriteLine("[45] - Task<TResult> - Run Func<T> Factory.StartNew - Different TResult");
            Console.WriteLine("[46] - Task<TResult> - Run ContinueWith CancellationToken");

            string dir = GetCurrentFolder();

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
                case "21":
                    await TaskClass.RunDelayWithCancellation();
                    goto default;
                case "22":
                    await TaskClass.RunDelayTimeSpanWithCancellation();
                    goto default;
                case "23":
                    //await TaskClass.RunFromExceptionTask(GetCurrentFolder());
                    await TaskClass.RunFromExceptionTask("c:\\FakeDirectory");
                    goto default;
                case "24":
                    //await TaskClass.RunFromExceptionTask(GetCurrentFolder());
                    await TaskClass.RunFromResultTask(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "temp"));
                    goto default;
                case "25":
                    await TaskClass.RunActionTask();
                    goto default;
                case "26":
                    await TaskClass.RunLambdaTask();
                    goto default;
                case "27":
                    await TaskClass.RunConcurrentBag([".", ".."]);
                    goto default;
                case "28":                    
                    await TaskClass.RunFuncWordCounter([$"{dir}/files/don-quijote-406", $"{dir}/files/little-prince-219"]);
                    goto default;
                case "29":
                    await TaskClass.RunThrowIfCancellationRequested(10, 20);
                    goto default;
                case "30":
                    await TaskClass.RunIsCancellationRequested(10, 20);
                    goto default;
                case "31":
                    await TaskClass.RunSynchronously(100);
                    goto default;
                case "32":
                    await TaskClass.RunStart(10);
                    goto default;
                case "33":
                    await TaskClass.RunWaitTimeoutAndCancellationToken(5000, 1500, 2500);
                    await TaskClass.RunWaitTimeoutAndCancellationToken(500, 1500, 2500);
                    goto default;
                case "34":
                    await TaskClass.RunWaitTimeSpan(5000, 150);
                    goto default;
                case "35":
                    await TaskClass.RunWaitCancellationToken(150);
                    goto default;
                case "36":
                    await TaskClass.RunWait(15);
                    goto default;
                case "37":
                    await TaskClass.RunWaitTimeout(15, 2000);
                    await TaskClass.RunWaitTimeout(15, 20);
                    goto default;
                case "38":
                    await TaskClass.RunWaitAll(10);
                    goto default;
                case "39":
                    await TaskClass.RunWaitAny(4);
                    goto default;
                case "40":
                    string[] urls = [ "www.adatum.com", "www.cohovineyard.com", "www.cohowinery.com", 
                                      "www.northwindtraders.com", "www.contoso.com", "dfssdfsd"];
                    await TaskClass.RunWhenAll(urls);
                    goto default;
                case "41":
                    await TaskClass.RunWhenAllDelay(10, 15);
                    goto default;
                case "42":
                    await TaskClass.RunFunc(100);
                    goto default;
                case "43":
                    await TaskClass.RunFactoryFunc(100);
                    goto default;
                case "44":
                    await TaskClass.RunWordCounter([$"{dir}/files/don-quijote-406", $"{dir}/files/little-prince-219"]);
                    goto default;
                case "45":
                    await TaskClass.RunFactoryDifferentTResult();
                    goto default;
                case "46":
                    await TaskClass.RunFuncContinueWithCancellationToken(20, 500);
                    goto default;
                default:
                    Console.WriteLine();
                    break;
            }
        }
    }

    static string GetCurrentFolder([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
}


