using OutdoorPlanner.Core;
using OutdoorPlanner.Configuration;
using OutdoorPlanner.Database;
using System.Net.Mail;
using System.Text;

namespace OutdoorPlanner.Email
{
    internal class EmailSender
    {
        public static void RegisterEvent(Event newEvent, EmailConfiguration configuration)
        {
            var toAddress = new MailAddress(newEvent.Mail);

            using (MailMessage mailMessage = new MailMessage(configuration.fromMailAddress, toAddress))
            {
                mailMessage.Subject = $"Мероприятие \"{newEvent.Name}\" зарегистрировано";

                mailMessage.Body = $@"
                    🎉 ВАШЕ МЕРОПРИЯТИЕ УСПЕШНО ЗАРЕГИСТРИРОВАНО!

                    Дорогой организатор,

                    Мы рады сообщить, что ваше мероприятие успешно добавлено в систему Outdoor Planner.

                    📋 ДЕТАЛИ МЕРОПРИЯТИЯ:
                    ───────────────────────────────────
                    🏷️ Название: {newEvent.Name}
                    📍 Место проведения: {newEvent.Location}
                    📅 Дата и время: {newEvent.DateTime:dd.MM.yyyy HH:mm}
                    ───────────────────────────────────

                    🌤️  ПРОГНОЗ ПОГОДЫ:
                    На момент мероприятия ожидается {newEvent.WeatherDesc.ToLower()} 
                    с температурой около {newEvent.Temp}°C.

                    💡 ЧТО ДАЛЬШЕ?
                    • Мы сохранили все данные о вашем мероприятии
                    • Вы получите напоминание при открытии программы менее чем за 24 часа
                    • При изменении погодных условий мы вас предупредим
                    ───────────────────────────────────
                    Это письмо отправлено автоматически. Пожалуйста, не отвечайте на него.";

                configuration.smtpClient.Send(mailMessage);
                Logger.Info("Сообщение о регистрации мероприятия отправлено");
            }

        }

        public static void SendNotification(EventContext db, EmailConfiguration configuration)
        {
            if (db.Events.Any())
            {
                List<Event> upcomingEvents = db.Events
                    .AsEnumerable()
                    .Where(x => DateTime.Parse(x.DateTime) <= DateTime.Now.AddDays(1) &&
                       DateTime.Parse(x.DateTime) >= DateTime.Now)
                    .ToList();

                foreach (Event e in upcomingEvents)
                {
                    if (!e.IsNotificated)
                    {
                        var newWeather = WeatherForecast.GetForecastForNotification(e);
                        var toAddress = new MailAddress(e.Mail);
                        e.Weather = newWeather.Weather[0].Main;
                        e.WeatherDesc = newWeather.Weather[0].Description;
                        e.Temp = newWeather.Main.Temp;

                        using (MailMessage mailMessage = new MailMessage(configuration.fromMailAddress, toAddress))
                        {
                            mailMessage.Subject = $"НАПОМИНАНИЕ: мероприятие \"{e.Name}\" скоро начнется!";

                            mailMessage.Body = $@"
                            ⏰ ВАШЕ МЕРОПРИЯТИЕ СКОРО НАЧНЕТСЯ!

                            Уважаемый организатор,

                            Напоминаем, что ваше мероприятие состоится уже совсем скоро!

                            📋 ДЕТАЛИ МЕРОПРИЯТИЯ:
                            ───────────────────────────────────
                            🏷️ Название: {e.Name}
                            📍 Место проведения: {e.Location}
                            📅 Дата и время: {DateTime.Parse(e.DateTime):dd.MM.yyyy HH:mm} по МСК
                            ⏳ Осталось времени: {GetTimeRemaining(DateTime.Parse(e.DateTime))}
                            ───────────────────────────────────

                            🌤️  ТЕКУЩИЙ ПРОГНОЗ ПОГОДЫ:
                            На момент мероприятия ожидается {e.WeatherDesc.ToLower()} 
                            с температурой около {e.Temp}°C.

                            📝 ПОСЛЕДНИЕ ПОДГОТОВКИ:
                            • Проверьте все необходимое оборудование
                            • Убедитесь в доступности места проведения
                            • Подготовьте запасной план на случай изменения погоды
                            • Возьмите с собой воду и средства защиты от солнца/дождя

                            🎯 СОВЕТЫ ПО ПОГОДЕ:
                            {GetWeatherAdvice(e.Weather, e.Temp)}

                            💫 ЖЕЛАЕМ УСПЕШНОГО ПРОВЕДЕНИЯ МЕРОПРИЯТИЯ!
                            Пусть все пройдет гладко и участники останутся довольны!

                            ───────────────────────────────────
                            Это письмо отправлено автоматически. Пожалуйста, не отвечайте на него.";

                            configuration.smtpClient.Send(mailMessage);
                        }
                        e.IsNotificated = true;
                        Logger.Info($"Уведомление о мерприятии {e.Name} отправлено");
                    }
                }
                db.SaveChanges();
            }
        }

        private static string GetTimeRemaining(DateTime eventTime)
        {
            TimeSpan remaining = eventTime - DateTime.Now;

            if (remaining.TotalHours >= 1)
            {
                return $"{remaining.Hours} часов и {remaining.Minutes} минут";
            }
            else if (remaining.TotalMinutes >= 1)
            {
                return $"{remaining.Minutes} минут";
            }
            else return "менее минуты";
        }

        private static string GetWeatherAdvice(string weather, double temperature)
        {
            var advice = new StringBuilder();

            if (weather.Contains("дождь") || weather.Contains("rain"))
            {
                advice.AppendLine("• 🌧️  Возьмите зонт и водонепроницаемую одежду");
                advice.AppendLine("• 🏕️  Подготовьте навес или палатку для укрытия");
                advice.AppendLine("• ⚡  Будьте осторожны при грозе");
            }
            else if (weather.Contains("ясно") || weather.Contains("clear"))
            {
                advice.AppendLine("• ☀️  Используйте солнцезащитный крем");
                advice.AppendLine("• 💧  Возьмите достаточное количество воды");
                advice.AppendLine("• 🕶️  Не забудьте головной убор и солнечные очки");
            }
            else if (weather.Contains("облачно") || weather.Contains("cloud"))
            {
                advice.AppendLine("• ⛅  Идеальные условия для outdoor-мероприятия");
                advice.AppendLine("• 🌡️  Температура комфортная для пребывания на улице");
            }
            else if (weather.Contains("снег") || weather.Contains("snow"))
            {
                advice.AppendLine("• ❄️  Одевайтесь теплее, берите термос с горячим напитком");
                advice.AppendLine("• 🧤  Не забудьте перчатки и теплую обувь");
            }

            if (temperature > 25)
            {
                advice.AppendLine("• 🔥  Высокая температура - обеспечьте тень и прохладительные напитки");
            }
            else if (temperature < 5)
            {
                advice.AppendLine("• 🧊  Низкая температура - предусмотрите обогрев и теплую одежду");
            }

            return advice.ToString();
        }
    }
}
