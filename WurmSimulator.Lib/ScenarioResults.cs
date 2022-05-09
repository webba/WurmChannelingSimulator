using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WurmSimulator.Lib
{
    public class ScenarioResults
    {
        public int Shatters { get; set; } = 0;

        public int Successes { get; set; } = 0;

        public ResultData? TotalFavor { get; set; }

        public ResultData? TotalCasts { get; set; }

        public ResultData? TotalDispels { get; set; }

    }
}
