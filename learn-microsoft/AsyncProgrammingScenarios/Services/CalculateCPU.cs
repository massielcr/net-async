using AsyncProgrammingScenarios.Models;

namespace AsyncProgrammingScenarios.Services
{
    public class CalculateCPU : ICalculateCPU
    {
        public DamageResult CalculateDamageDone(int counter)
        {
            int total = 0;
            var rnd = new Random();

            for (int i = 0; i < counter; i++)
            {
                total += rnd.Next(1, 7);
            }


            return new DamageResult()
            {
                Damage = total
            };
        }
    }
}
