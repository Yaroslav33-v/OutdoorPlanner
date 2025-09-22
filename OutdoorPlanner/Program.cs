using OutdoorPlanner.Configuration;
using OutdoorPlanner.Email;
using OutdoorPlanner.Core;
using OutdoorPlanner.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutdoorPlanner
{
    internal class Program
    {
        internal static string _openWeatherApiKey;
        internal static string _connectionString;
        internal static EmailConfiguration _emailConfiguration;
        static void Main()
        {
            try
            {
                ConfigurationLoader.LoadConfiguration();

                using (EventContext db = new EventContext())
                {
                    EmailSender.SendNotification(db, _emailConfiguration);
                    DbWorker.DeleteByDate(db);

                    bool restartNeeded;
                    do
                    {
                        restartNeeded = false;
                        try
                        {
                            Console.WriteLine("Программа OutdoorPlanner приветствует вас!" +
                                "\n1. Просмотреть запланированные события" +
                                "\n2. Зарегистрировать новое мероприятие");

                            if (!int.TryParse(Console.ReadLine(), out int choice))
                                throw new ArgumentNullException("Выберите корректное действие!");

                            if (choice == 1)
                            {
                                var idMap = DbWorker.PrintData(db);
                                Console.WriteLine("\nВыберите дальнешее действие:" +
                                    "\n1. Зарегистрировать новое мероприятие" +
                                    "\n2. Удалить мероприятие" +
                                    "\n3. Выйти из программы");

                                if (!int.TryParse(Console.ReadLine(), out int choice1))
                                    throw new ArgumentNullException("Выберите корректное действие!");

                                if (choice1 == 1) choice = 2;
                                else if (choice1 == 2)
                                {
                                    Console.WriteLine("Введите id мероприятия для удаления");
                                    if (!int.TryParse(Console.ReadLine(), out int id))
                                        throw new ArgumentNullException("Введите корректный id!");
                                    DbWorker.DeleteById(db, idMap, id);
                                }
                                else if (choice1 == 3) return;
                                if (choice1 < 0 || choice1 > 3)
                                    throw new ArgumentNullException("Выберите корректное действие!");
                            }
                            if (choice == 2)
                            {
                                RunApplication(db);
                            }
                            if (choice < 1 || choice > 2)
                                throw new ArgumentNullException("Выберите корректное действие!");
                            restartNeeded = AskForRestart("Желаете перезапустить приложение? (y/n)");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Произошла ошибка: {ex.Message}");
                            restartNeeded = AskForRestart("Желаете перезапустить приложение? (y/n)");
                        }
                    } while (restartNeeded);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static bool AskForRestart(string question)
        {
            Console.WriteLine(question);
            var response = Console.ReadLine()?.ToLower();

            return response == "y" || response == "yes" || response == "д" || response == "да";
        }

        public static void RunApplication(EventContext db)
        {
            try
            {
                int daysCount = 4;
                Event newEvent = new Event();

                Console.WriteLine("=== Служба планировщика мероприятий запущена ===");
                Console.WriteLine(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));

                Console.WriteLine("Введите название мероприятия:");
                newEvent.Name = ValidateNotEmpty(Console.ReadLine(), "Название не может быть пустым!");

                Console.WriteLine("Введите название города: ");
                newEvent.Location = ValidateNotEmpty(Console.ReadLine(), "Название города не может быть пустым!");

                Console.WriteLine("Выберите время начала мероприятия (чч-чч):");
                string eventTimeSpan = ValidateNotEmpty(Console.ReadLine(), "Заполните часы проведения мероприятия!");

                (int startHour, int endHour) = TimeWorker.ParseEventTime(eventTimeSpan);
                (DateTime eventStart, DateTime eventEnd) = TimeWorker.FindCurrentInterval(startHour, endHour);

                Console.WriteLine("Погода в следующие дни: ");

                var weatherResponse = WeatherForecast.GetWeatherForecast(newEvent.Location, daysCount);
                int i = 1;

                foreach (var weather in weatherResponse.List)
                {
                    bool isInTimeInterval = weather.Dt_txt.TimeOfDay >= eventStart.TimeOfDay &&
                                           weather.Dt_txt.TimeOfDay <= eventEnd.TimeOfDay;

                    bool isOvernightEvent = eventEnd.TimeOfDay == TimeSpan.Zero &&
                                           weather.Dt_txt.TimeOfDay >= eventStart.TimeOfDay;

                    if ((isInTimeInterval || isOvernightEvent) && endHour >= weather.Dt_txt.Hour)
                    {
                        Console.WriteLine($"{i}. {weather.Dt_txt} " +
                            $"({GetRussianDayOfWeek(weather.Dt_txt.DayOfWeek)}) " +
                            $"Погода: {FirstLetterToUpper(weather.Weather[0].Description)} " +
                            $"Температура: {weather.Main.Temp} °C");
                        weather.SuitableIndex = i;
                        i++;
                    }
                }

                Console.WriteLine("Выберите подходящую дату и введите её индекс: ");
                if (!int.TryParse(Console.ReadLine(), out int chosenIndex))
                    throw new ArgumentNullException("Выберите дату и время!");
                if (chosenIndex < 0 || chosenIndex > weatherResponse.List.Max(x => x.SuitableIndex))
                {
                    throw new ArgumentOutOfRangeException("Введите корректный индекс!");
                }

                var weatherInChosenDay = weatherResponse.List.Where(x => x.SuitableIndex == chosenIndex).First();

                newEvent.DateTime = weatherInChosenDay.Dt_txt.ToString();

                if (weatherInChosenDay.Dt_txt <= DateTime.Now.AddDays(1))
                {
                    newEvent.IsNotificated = true;
                }

                newEvent.Weather = weatherInChosenDay.Weather[0].Main;
                newEvent.WeatherDesc = weatherInChosenDay.Weather[0].Description;
                newEvent.Temp = weatherInChosenDay.Main.Temp;

                Console.WriteLine("Введите вашу почту:");
                string email = ValidateNotEmpty(Console.ReadLine(), "Заполните почту!");

                if (IsValidEmail(email)) newEvent.Mail = email;
                else throw new ArgumentNullException("Введите настоящую почту!");

                db.Events.Add(newEvent);
                db.SaveChanges();

                EmailSender.RegisterEvent(newEvent, _emailConfiguration);

                Console.WriteLine("Мероприятие успешно зарегистрировано!");
            }
            catch
            {
                throw;
            }
        }

        public static string FirstLetterToUpper(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            char[] letters = str.ToCharArray();
            letters[0] = char.ToUpper(letters[0]);
            return new string(letters);
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private static string ValidateNotEmpty(string value, string exMessage)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(exMessage);
            }
            return value;
        }

        private static string GetRussianDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Понедельник",
                DayOfWeek.Tuesday => "Вторник",
                DayOfWeek.Wednesday => "Среда",
                DayOfWeek.Thursday => "Четверг",
                DayOfWeek.Friday => "Пятница",
                DayOfWeek.Saturday => "Суббота",
                DayOfWeek.Sunday => "Воскресенье",
                _ => dayOfWeek.ToString()
            };
        }
    }
}
