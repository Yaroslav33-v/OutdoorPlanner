namespace OutdoorPlanner.ClassesForWeather
{
    internal class WeatherInfoForTimespan
    {
        public int? SuitableIndex { get; set; } = null;
        public WeatherInfo Main { get; set; }
        public List<WeatherConditionsInfo> Weather { get; set; }
        public WindInfo Wind { get; set; }
        public DateTime Dt_txt { get; set; }
    }
}
