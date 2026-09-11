namespace SystemThreadingTasksEnums
{
    internal static class EnumConfigureAwaitOptions
    {
        internal static async Task Run()
        {
            Console.WriteLine("No Options:");
            await Method1();
            Console.WriteLine();

            Console.WriteLine("Options: None");
            await Method1(ConfigureAwaitOptions.None);
            Console.WriteLine();

            Console.WriteLine("Options: ContinueOnCapturedContext - Exception");
            await Method1(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Console.WriteLine();

            Console.WriteLine("Options: SuppressThrowing");
            await Method1(ConfigureAwaitOptions.SuppressThrowing);
            Console.WriteLine();

            Console.WriteLine("Options: ForceYielding");
            await Method1(ConfigureAwaitOptions.ForceYielding);
            Console.WriteLine();

            Console.WriteLine("Options: None - Exception");
            await Method1Exception(ConfigureAwaitOptions.None);
            Console.WriteLine();

            Console.WriteLine("Options: ContinueOnCapturedContext - Exception");
            await Method1Exception(ConfigureAwaitOptions.ContinueOnCapturedContext);
            Console.WriteLine();

            Console.WriteLine("Options: SuppressThrowing - Exception");
            await Method1Exception(ConfigureAwaitOptions.SuppressThrowing);
            Console.WriteLine();

            Console.WriteLine("Options: ForceYielding - Exception");
            await Method1Exception(ConfigureAwaitOptions.ForceYielding);
            Console.WriteLine();
        }


        internal static async Task Method1()
        {
            int threadId = Environment.CurrentManagedThreadId;

            Logger.Log($"Started. - Current Thread ID: {threadId}");

            try
            {
                await Method2();
            }
            catch (Exception ex)
            {
                Logger.Log($"Exception. - Current Thread ID: {threadId}");
            }            

            Logger.Log($"Completed. - Current Thread ID: {threadId}");
        }

        internal static async Task Method1(ConfigureAwaitOptions option)
        {
            int threadId = Environment.CurrentManagedThreadId;

            Logger.Log($"Started. - Current Thread ID: {threadId}");

            try
            {
                await Method2().ConfigureAwait(option);
            }
            catch (Exception ex)
            {
                Logger.Log($"Exception. - Current Thread ID: {threadId}");
            }            

            Logger.Log($"Completed. - Current Thread ID: {threadId}");
        }

        internal static async Task Method1Exception(ConfigureAwaitOptions option)
        {
            int threadId = Environment.CurrentManagedThreadId;

            Logger.Log($"Started. - Current Thread ID: {threadId}");

            try
            {
                await Method2Exception().ConfigureAwait(option);
            }
            catch (Exception ex)
            {
                Logger.Log($"Exception. - Current Thread ID: {threadId}");
            }            

            Logger.Log($"Completed. - Current Thread ID: {threadId}");
        }


        internal static async Task Method2()
        {
            int threadId = Environment.CurrentManagedThreadId;

            Logger.Log($"Started. - Current Thread ID: {threadId}");

            await Task.Delay(500);
            await Task.CompletedTask;

            Logger.Log($"Completed. - Current Thread ID: {threadId}");
        }        

        internal static async Task Method2Exception()
        {
            int threadId = Environment.CurrentManagedThreadId;

            await Task.Delay(500);
            Logger.Log($"Started. - Current Thread ID: {threadId}");

            throw new Exception("Errorrrrr");
        }
    }
}
