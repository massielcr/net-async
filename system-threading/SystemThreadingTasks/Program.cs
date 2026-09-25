using SystemThreadingTasks;
using SystemThreadingTasks.Enums;

class Program
{
    static async Task Main()
    {
        while(true)
        {            
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("[1] - Enums - ConfigureAwaitOptions");
            Console.WriteLine("[2] - Enums - TaskStatus");
            Console.WriteLine("[5] - Task  - instantiation");

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
                default:
                    Console.WriteLine();
                    break;
            }
        }
    }    
}


