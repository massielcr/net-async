using AsyncProgrammingScenarios.Models;

namespace AsyncProgrammingScenarios.Services
{
    public interface ICalculateCPU
    {
        DamageResult CalculateDamageDone(int counter);
    }
}
