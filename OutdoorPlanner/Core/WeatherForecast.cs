using Newtonsoft.Json;
using OutdoorPlanner.ClassesForWeather;
using System.Net;
using OutdoorPlanner.Configuration;

namespace OutdoorPlanner.Core
{
    internal class WeatherForecast
    {
        public static WeatherResponse GetWeatherForecast(string city, int daysCount)
        {
            WeatherResponse weatherResponse;
            string apiKey = Program._openWeatherApiKey;
            int? timeSpanCount = daysCount * 8 + (8 - TimeWorker.FindCurrentInterval());// Получение каждого интервала для каждого дня
            string urlWeather = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&cnt={timeSpanCount}&appid={apiKey}&units=metric&lang=ru";

            try
            {
                Logger.Info("Подключение к api");
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(urlWeather);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                string response;
                using (StreamReader sr = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    response = sr.ReadToEnd();
                }

                weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);
            }
            catch (WebException)
            {
                Logger.Error("Используется несуществующий город");
                throw new ArgumentNullException("Такого города не существует!");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка при подключении к api");
                throw ex;
            }
            return weatherResponse;
        }

        public static WeatherInfoForTimespan GetForecastForNotification(Event e)
        {
            WeatherResponse weatherResponse;
            string apiKey = Program._openWeatherApiKey;
            string urlWeather = $"https://api.openweathermap.org/data/2.5/forecast?q={e.Location}&cnt=8&appid={apiKey}&units=metric&lang=ru";

            try
            {
                Logger.Info("Подключение к api");
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(urlWeather);
                HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                string response;
                using (StreamReader sr = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    response = sr.ReadToEnd();
                }

                weatherResponse = JsonConvert.DeserializeObject<WeatherResponse>(response);

                var newWeatherInfo = weatherResponse.List
                    .Where(x => x.Dt_txt == DateTime.Parse(e.DateTime))
                    .First();

                return newWeatherInfo;
            }
            catch (WebException)
            {
                Logger.Error("Используется несуществующий город");
                throw new ArgumentNullException("Такого города не существует!");
            }
            catch (Exception ex)
            {
                Logger.Error("Ошибка при подключении к api");
                throw ex;
            }
        }
    }
}
