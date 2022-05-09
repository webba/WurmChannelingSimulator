using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WurmSimulator.Lib
{
    public class Scenario
    {
        public double ChannelingSkill { get; set; } = 100;

        public bool Bene { get; set; } = true;

        public double Bonus { get; set; } = 70;

        public double TargetPower { get; set; } = 90;

        public double DispellThreshold { get; set; } = 120;

        public double Simulations { get; set; } = 100;

        public double ItemQL { get; set; } = 90;

        public double SpellFavor { get; set; } = 50;

        public double SpellDifficulty { get; set; } = 60;

        public ShatterType ShatterType { get; set; } = ShatterType.NoReplace;

        public string GetDescriptor()
        {
            return string.Format("{0};{1};{2};{3};{4};{5}", ChannelingSkill, Bene, DispellThreshold, Bonus, TargetPower, ItemQL);
        }
    }
}
