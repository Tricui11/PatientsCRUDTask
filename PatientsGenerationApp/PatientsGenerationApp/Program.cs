﻿using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

class Program
{
    static readonly Random _random = new();

    static async Task Main()
    {
        IConfiguration config = LoadConfiguration();
        string baseUrl = config["ApiSettings:BaseUrl"];
        string apiUrl = $"{baseUrl}/api/Patients";

        using HttpClient client = new();
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
        string[] genders = { "male", "female", "other", "unknown" };

        bool isMale = _random.Next(2) == 0;
        string firstName = isMale ? maleFirstNames[_random.Next(maleFirstNames.Length)] : femaleFirstNames[_random.Next(femaleFirstNames.Length)];
        string middleName = isMale ? maleMiddleNames[_random.Next(maleMiddleNames.Length)] : femaleMiddleNames[_random.Next(femaleMiddleNames.Length)];
        string lastName = lastNames[_random.Next(lastNames.Length)];
        string gender = genders[_random.Next(genders.Length)];

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
            birthDate = DateTime.UtcNow.AddYears(-_random.Next(1, 100)).ToString("yyyy-MM-ddTHH:mm:ss"),
            active = _random.Next(2) == 0
        };
    }
}
