using Newtonsoft.Json;
using OutdoorPlanner;
using OutdoorPlanner.Core;
using OutdoorPlanner.ClassesForWeather;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

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
                throw new ArgumentNullException("Такого города не существует!");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return weatherResponse;
        }

        public static WeatherInfoForTimespan GetForecastForNotification(Event e)
        {
            WeatherResponse weatherResponse;
            string apiKey = Program._openWeatherApiKey;
            string urlWeather = $"https://api.openweathermap.org/data/2.5/forecast?q={e.Location}&cnt=9&appid={apiKey}&units=metric&lang=ru";

            try
            {
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
                throw new ArgumentNullException("Такого города не существует!");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
