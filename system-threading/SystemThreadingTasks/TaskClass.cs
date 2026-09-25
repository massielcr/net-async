namespace SystemThreadingTasks
{
    internal class TaskClass
    {
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
    }
}
