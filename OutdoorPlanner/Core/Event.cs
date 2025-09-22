using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutdoorPlanner.Core
{
    internal class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string DateTime { get; set; }
        public string Weather { get; set; }
        public string WeatherDesc { get; set; }
        public float Temp { get; set; }
        public string Mail { get; set; }
        public bool IsNotificated { get; set; } = false;
    }
}
