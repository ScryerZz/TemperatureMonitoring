using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemperatureMonitoring
{
    public class TemperatureViolation
    {
        public DateTime ViolationTime { get; set; }
        public int RequiredTemperature { get; set; }
        public int ActualTemperature { get; set; }
        public int Deviation { get; set; }
    }
}