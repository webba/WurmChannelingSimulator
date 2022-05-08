using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WurmSimulator
{
    public class Scenario
    {
        public double ChannelingSkill = 98.80;
        public bool Bene = true;
        public double Bonus = 70;
        public double TargetPower = 100;
        public double DispellThreshold = 120;
        public double ItemQL = 90;

        public string GetDescriptor()
        {
            return string.Format("{0};{1};{2};{3};{4};{5}", ChannelingSkill, Bene, DispellThreshold, Bonus, TargetPower, ItemQL);
        }
    }
}
