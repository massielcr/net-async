using Breakfast.CL.Models;

namespace Breakfast.CL.Services
{
    public interface IToastService
    {
        Toast ToastBread(int slices);

        Task<Toast> ToastBreadAsync(int slices);

        void ApplyButter(Toast toast);

        void ApplyJam(Toast toast);
    }
}
