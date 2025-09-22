using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutdoorPlanner.Core
{
    internal class TimeWorker
    {
        public static List<(int Index, DateTime Start, DateTime End)> GetThreeHourIntervals(DateTime date)
        {
            var intervals = new List<(int, DateTime, DateTime)>();
            var startOfDay = date.Date;

            for (int i = 0; i < 8; i++)
            {
                DateTime start = startOfDay.AddHours(i * 3);
                DateTime end = start.AddHours(3);
                intervals.Add((i, start, end));
            }

            return intervals;
        }

        public static int? FindCurrentInterval()
        {
            DateTime currentTime = DateTime.Now;
            var intervals = GetThreeHourIntervals(currentTime.Date);

            foreach (var interval in intervals)
            {
                if (currentTime >= interval.Start && currentTime < interval.End)
                {
                    return interval.Index;
                }
            }

            return null;
        }

        public static (DateTime Start, DateTime End) FindCurrentInterval(int startHour, int endHour)
        {
            DateTime startTime = DateTime.MinValue, endTime = DateTime.MinValue;
            var intervals = GetThreeHourIntervals(DateTime.Now.Date);

            foreach (var interval in intervals)
            {
                if (startHour >= interval.Start.Hour && startHour < interval.End.Hour)
                {
                    startTime = interval.Start;
                }
                if (endHour > interval.Start.Hour && endHour <= interval.End.Hour)
                {
                    endTime = interval.End;
                }
                if (endHour == 22 || endHour == 23 || endHour == 24)
                {
                    endTime = intervals.Last().End;
                }
            }

            return (startTime, endTime);
        }

        public static (int startHour, int endHour) ParseEventTime(string timeSpan)
        {
            if (string.IsNullOrWhiteSpace(timeSpan))
            {
                throw new ArgumentException("Строка не может быть пустой");
            }
            string[] parts = timeSpan.Split('-');

            if (parts.Length != 2)
            {
                throw new FormatException("Неверный формат. Ожидается: чч-чч");
            }

            if (!int.TryParse(parts[0], out int startHour) || !int.TryParse(parts[1], out int endHour))
            {
                throw new FormatException("Неверный формат. Ожидаются числовые значения");
            }

            if (startHour < 0 || startHour > 23 || endHour < 0 || endHour > 23)
            {
                throw new ArgumentException("Часы должны быть в диапазоне от 0 до 23");
            }

            if (startHour > endHour)
            {
                throw new ArgumentException("Время начала должно быть раньше времени окончания");
            }

            return (startHour, endHour);
        }
    }
}
