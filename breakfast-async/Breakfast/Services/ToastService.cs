using Breakfast.CL.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

        public async Task<Toast> ToastBreadAsync(int slices)
        {
            for (int slice = 0; slice < slices; slice++)
            {
                Console.WriteLine("Putting a slice of bread in the toaster");
            }
            Console.WriteLine("Start toasting...");
            await Task.Delay(2000);

            if (slices <= 0)
            {
                Console.WriteLine("Fire! Toast is ruined!");
                throw new InvalidOperationException("The toaster is on fire");
            }                

            Task.Delay(1000).Wait();
            Console.WriteLine("Remove toast from toaster");

            return new Toast();
        }

        public async Task<Toast> MakeToastWithButterAndJamAsync(int slices)
        {
            var toast = await ToastBreadAsync(slices);
            ApplyButter(toast);
            ApplyJam(toast);

            return toast;
        }
    }
}
