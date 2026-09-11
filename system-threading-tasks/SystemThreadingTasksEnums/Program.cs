using SystemThreadingTasksEnums;

class Program
{
    static async Task Main()
    {
        await RunConfigureAwaitOptions();

        Console.ReadLine();
    }

    static async Task RunConfigureAwaitOptions()
    {
        Console.WriteLine("No Options:");
        await EnumConfigureAwaitOptions.Method1();
        Console.WriteLine();

        Console.WriteLine("Options: None");
        await EnumConfigureAwaitOptions.Method1(ConfigureAwaitOptions.None);
        Console.WriteLine();

        Console.WriteLine("Options: ContinueOnCapturedContext - Exception");
        await EnumConfigureAwaitOptions.Method1(ConfigureAwaitOptions.ContinueOnCapturedContext);
        Console.WriteLine();

        Console.WriteLine("Options: SuppressThrowing");
        await EnumConfigureAwaitOptions.Method1(ConfigureAwaitOptions.SuppressThrowing);
        Console.WriteLine();

        Console.WriteLine("Options: ForceYielding");
        await EnumConfigureAwaitOptions.Method1(ConfigureAwaitOptions.ForceYielding);
        Console.WriteLine();

        Console.WriteLine("Options: None - Exception");
        await EnumConfigureAwaitOptions.Method1Exception(ConfigureAwaitOptions.None);
        Console.WriteLine();

        Console.WriteLine("Options: ContinueOnCapturedContext - Exception");
        await EnumConfigureAwaitOptions.Method1Exception(ConfigureAwaitOptions.ContinueOnCapturedContext);
        Console.WriteLine();

        Console.WriteLine("Options: SuppressThrowing - Exception");
        await EnumConfigureAwaitOptions.Method1Exception(ConfigureAwaitOptions.SuppressThrowing);
        Console.WriteLine();

        Console.WriteLine("Options: ForceYielding - Exception");
        await EnumConfigureAwaitOptions.Method1Exception(ConfigureAwaitOptions.ForceYielding);
        Console.WriteLine();
    }
}


