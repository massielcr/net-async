using System.Runtime.CompilerServices;

namespace SystemThreadingTasksEnums
{
    internal static class Logger
    {
        public static void Log(string message, [CallerMemberName] string methodName = "")
        {
            Console.WriteLine($"{methodName} {message}");
        }
    }
}
