using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WurmSimulator.Lib
{
    public class SimulationResult
    {
        public bool Success { get; set; } = false;

        public int Shatter { get; set; } = 0;

        public int TotalCasts { get; set; } = 0;

        public int TotalDispels { get; set; } = 0;

        public double TotalFavor { get; set; } = 0;
    }
}
