using Microsoft.Extensions.Configuration;
using OutdoorPlanner.Email;

namespace OutdoorPlanner.Configuration
{
    public class ConfigurationLoader
    {
        public static void LoadConfiguration()
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(GetProjectRootPath())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                
                Program._openWeatherApiKey = configuration["OpenWeather:ApiKey"]
                     ?? throw new ArgumentNullException("OpenWeather:ApiKey не найден в конфигурации");

                Program._connectionString = configuration["ConnectionStrings:DefaultConnection"]
                     ?? throw new ArgumentNullException("ConnectionStrings:DefaultConnection не найден в конфигурации");

                Program._emailConfiguration = new EmailConfiguration(
                   configuration["EmailSettings:EmailFrom"]
                   ?? throw new ArgumentNullException("EmailSettings:EmailFrom не найден в конфигурации"),

                   configuration["EmailSettings:EmailPassword"]
                   ?? throw new ArgumentNullException("EmailSettings:EmailPassword не найден в конфигурации"),

                   configuration["EmailSettings:SmtpServer"]
                   ?? throw new ArgumentNullException("EmailSettings:SmtpServer не найден в конфигурации"),

                   int.Parse(configuration["EmailSettings:SmtpPort"]
                   ?? throw new ArgumentNullException("EmailSettings:SmtpPort не найден в конфигурации")
                   )
               );
            }
            catch (Exception ex)
            {
                Logger.Error($"Ошибка загрузки конфигурации: {ex.Message}");
                throw new Exception($"Ошибка загрузки конфигурации: {ex.Message}");
            }
        }

        static string GetProjectRootPath() => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, @"..\..\"));
    }
}
