using Breakfast.CL.Models;

namespace Breakfast.CL.Services
{
    public class ToastService : IToastService
    {
        public void ApplyButter(Toast toast) => Console.WriteLine("Putting butter on the toast");

        public void ApplyJam(Toast toast) => Console.WriteLine("Putting jam on the toast");

        public Toast ToastBread(int slices)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                Console.WriteLine("Putting a slice of bread in the toaster");
            }
            Console.WriteLine("Start toasting...");
            Task.Delay(3000).Wait();
            Console.WriteLine("Remove toast from toaster");

            return new Toast();
        }

        public Task<Toast> ToastBreadAsync(int slices)
        {
            throw new NotImplementedException();
        }
    }
}
