using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WurmSimulator
{
    public static class WurmSkillUtil
    {
        private static readonly Random random = new();
        private static readonly Normal normalDist = new();

        public static readonly double DispelFavorCost = 10;
        public static readonly double DispelDifficulty = 20;
        public static readonly double CocDifficulty = 60;
        public static readonly double CocFavorCost = 50;
        public static readonly double LtFavorCost = 100;

        public static double SkillCheck(double skill, double difficulty,  double bonus)
        {
            double effective = EffectiveSkill(skill, bonus);
            return RollGaussian((float)effective, (float)difficulty);
        }

        public static double EffectiveSkill(double skill, double bonus)
        {
            if (bonus > 70)
            {
                bonus = 70;
            }
            double linearMax = (100 + skill) / 2;
            double diffToMaxChange = Math.Min(skill, linearMax - skill);
            double newBon = diffToMaxChange * bonus / 100;
            skill += newBon;
            return skill;
        }

        public static float RollGaussian(float skill, float difficulty)
        {
            float slide = (skill * skill * skill - difficulty * difficulty * difficulty) / 50000.0F + skill - difficulty;
            float w = 30 - Math.Abs(skill - difficulty) / 4;

            int attempts = 0;
            float result = 0;
            while (true)
            {
                result =  (float)normalDist.Sample() * (w + Math.Abs(slide) / 6) + slide;
                float rejectCutoff = (float)normalDist.Sample() * (w - Math.Abs(slide) / 6) + slide;
                if (slide > 0)
                {
                    if (result > rejectCutoff + Math.Max(100 - slide, 0))
                    {
                        result = -1000;
                    }
                }
                else if (result < rejectCutoff - Math.Max(100 + slide, 0))
                {
                    result = -1000;
                }
                attempts += 1;
                if (attempts == 100)
                {
                    if (result > 100)
                    {
                        return 90 + (float)random.NextDouble() * 5;
                    }
                    if (result < -100)
                    {
                        return -90 - (float)random.NextDouble() * 5;
                    }
                }

                if (result > -100 && result < 100)
                {
                    break;
                }
            }
            return result;
        }


        public static bool DispelCheck(double power, double skill, double targetDifficulty, double bonus, bool bene)
        {
            int limit = random.Next((int)power + (int)targetDifficulty);
            double check = SkillCheck(skill, DispelDifficulty, bonus) + (bene ? 5 : 0);
            return check > limit;
        }


        public static bool IsShatter(double power, double ql)
        {
            return (power < (-1 * ql)) || (power < 0 && random.NextDouble() <= 0.01);
        }

        public static float GetFinalPower(float currentPower, float newPower, bool bene)
        {
            float mod = 5.0F * (1.0F - currentPower / (bene ? 105.0F : 100.0F));
            return mod + newPower;
        }
    }
}
