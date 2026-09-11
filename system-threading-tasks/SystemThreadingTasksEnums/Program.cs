using SystemThreadingTasksEnums;

class Program
{
    static async Task Main()
    {
        while(true)
        {
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine("[1] - ConfigureAwaitOptions");
            Console.WriteLine("[2] - TaskStatus");

            string? key = Console.ReadLine();

            switch (key)
            {
                case "1":
                    await EnumConfigureAwaitOptions.Run();
                    break;
                case "2":
                    await EnumTaskStatus.Run();
                    break;
                default:
                    break;
            }
        }
    }    
}


