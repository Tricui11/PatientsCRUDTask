using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

class Program
{
    static readonly Random _random = new();

    static async Task Main()
    {
        IConfiguration config = LoadConfiguration();
        var baseUrl = Environment.GetEnvironmentVariable("ApiSettings__BaseUrl") ?? config["ApiSettings:BaseUrl"];
        string apiUrl = $"{baseUrl}/api/Patients";

        using HttpClient client = new();
        while (true)
        {
            Console.WriteLine("Хотите добавить 100 записей? Нажмите Enter для добавления, или любую другую клавишу для выхода...");
            string input = Console.ReadLine();

            if (string.IsNullOrEmpty(input))
            {
                for (int i = 0; i < 100; i++)
                {
                    var patient = GeneratePatient();
                    string json = JsonConvert.SerializeObject(patient, Formatting.Indented);
                    StringContent content = new(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);
                    Console.WriteLine($"[{i + 1}/100] Status: {response.StatusCode}");
                }

                Console.WriteLine("Добавление 100 пациентов завершено.");
            }
            else
            {
                Console.WriteLine("Завершаем программу.");
                break;
            }
        }
    }

    static IConfiguration LoadConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    static object GeneratePatient()
    {
        string[] maleFirstNames = { "Иван", "Алексей", "Петр", "Василий", "Сергей" };
        string[] femaleFirstNames = { "Анна", "Мария", "Ольга", "Екатерина", "Наталья" };
        string[] maleMiddleNames = { "Иванович", "Алексеевич", "Петрович", "Васильевич", "Сергеевич" };
        string[] femaleMiddleNames = { "Ивановна", "Алексеевна", "Петровна", "Васильевна", "Сергеевна" };
        string[] lastNames = { "Иванов", "Петров", "Сидоров", "Кузнецов", "Смирнов" };

        bool isMale = _random.Next(2) == 0;
        string firstName = isMale ? maleFirstNames[_random.Next(maleFirstNames.Length)] : femaleFirstNames[_random.Next(femaleFirstNames.Length)];
        string middleName = isMale ? maleMiddleNames[_random.Next(maleMiddleNames.Length)] : femaleMiddleNames[_random.Next(femaleMiddleNames.Length)];
        string lastName = lastNames[_random.Next(lastNames.Length)];
        string gender = isMale ? "male" : "female";

        return new
        {
            name = new
            {
                id = Guid.NewGuid(),
                use = "official",
                family = lastName,
                given = new[] { firstName, middleName }
            },
            gender,
            birthDate = DateTime.UtcNow
                .AddYears(-_random.Next(1, 20))
                .AddMonths(-_random.Next(1, 12))
                .AddDays(-_random.Next(1, 31))
                .AddHours(-_random.Next(1, 24))
                .AddMinutes(-_random.Next(1, 60))
                .AddSeconds(-_random.Next(1, 60))
                .ToString("yyyy-MM-ddTHH:mm:ss"),
            active = _random.Next(2) == 0
        };
    }
}
