using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Employees1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            List<Employee> employees = new List<Employee>();
            for (int i = 0; i < 1000; i++)
            {
                employees.Add(new Employee { Id = i, Name = $"Employee_{i}", Age = new Random().Next(20, 60) });
            }

            Console.WriteLine($"Поток {Environment.CurrentManagedThreadId}: Начало работы");

            StreamService<Employee> streamService = new StreamService<Employee>();

            var stream = new MemoryStream();
            var progress = new Progress<string>(msg => Console.WriteLine(msg));

            Task task1 = Task.Run(async () =>
            {
                await streamService.WriteToStreamAsync(stream, employees, progress);
            });

            await Task.Delay(200);

            Task task2 = Task.Run(async () =>
            {
                await streamService.CopyFromStreamAsync(stream, "employees.dat", progress);
            });

            await Task.WhenAll(task1, task2);

            int count = await streamService.GetStatisticsAsync("employees.dat", e => e.Age > 35);

            Console.WriteLine($"Количество сотрудников старше 35 лет: {count}");

            Console.ReadKey();
        }
    }
}
