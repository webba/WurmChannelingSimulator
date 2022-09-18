using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WurmSimulator.Lib
{
    public class WurmSkillUtil
    {
        private static readonly Random random = new();
        private static readonly Normal normalDist = new();

        public static readonly double DispelFavorCost = 10;
        public static readonly double DispelDifficulty = 20;
        public static readonly double CocDifficulty = 60;
        public static readonly double CocFavorCost = 50;
        public static readonly double LtFavorCost = 100;

        private double targetSims = 0;
        private ConcurrentBag<int> completedSimulations = new ConcurrentBag<int>();

        public static double SkillCheck(double skill, double difficulty, double bonus)
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
                result = (float)normalDist.Sample() * (w + Math.Abs(slide) / 6) + slide;
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

        public async Task<ScenarioResults> RunScenarioAsync(Scenario scenario, Action<double> callback)
        {
            await Task.Yield();

            completedSimulations.Clear();
            targetSims = (int)scenario.Simulations;

            var tasks = Enumerable.Range(0, (int)scenario.Simulations)
                .Select(s => RunScenarioSimulationAsync(scenario, s, callback))
                .ToList();

            var resultsList = await Task.WhenAll(tasks);

            return new()
            {
                Shatters = resultsList.Sum(x => x.Shatter),
                Successes = resultsList.Count(x => x.Success),
                TotalFavor = new()
                {
                    Average = resultsList.Average(x => x.TotalFavor),
                    StandardDeviation = Statistics.StandardDeviation(resultsList.Select(x => x.TotalFavor))
                },
                TotalCasts = new()
                {
                    Average = resultsList.Average(x => x.TotalCasts),
                    StandardDeviation = Statistics.StandardDeviation(resultsList.Select(x => (double)x.TotalCasts))
                },
                TotalDispels = new()
                {
                    Average = resultsList.Average(x => x.TotalDispels),
                    StandardDeviation = Statistics.StandardDeviation(resultsList.Select(x => (double)x.TotalDispels))
                }
            };
        }

        public async Task<ScenarioResults> RunScenarioAsyncCallback(Scenario scenario, Action<double> callback)
        {
            await Task.Yield();

            completedSimulations.Clear();
            targetSims = (int)scenario.Simulations;

            var tasks = Enumerable.Range(0, (int)scenario.Simulations)
                .Select(s => RunScenarioSimulationAsync(scenario, s, callback))
                .ToList();

            var resultsList = await Task.WhenAll(tasks);

            return new()
            {
                Shatters = resultsList.Sum(x => x.Shatter),
                Successes = resultsList.Count(x => x.Success),
                TotalFavor = new()
                {
                    Average = resultsList.Average(x => x.TotalFavor),
                    StandardDeviation = Statistics.StandardDeviation(resultsList.Select(x => x.TotalFavor))
                },
                TotalCasts = new()
                {
                    Average = resultsList.Average(x => x.TotalCasts),
                    StandardDeviation = Statistics.StandardDeviation(resultsList.Select(x => (double)x.TotalCasts))
                },
                TotalDispels = new()
                {
                    Average = resultsList.Average(x => x.TotalDispels),
                    StandardDeviation = Statistics.StandardDeviation(resultsList.Select(x => (double)x.TotalDispels))
                }
            };
        }

        public static ScenarioResults RunScenario(Scenario scenario)
        {
            var results = new ConcurrentBag<SimulationResult>();
            Parallel.For(0, (int)scenario.Simulations, (sim) => results.Add(RunScenarioSimulation(scenario)));
            var resultsList = results.ToArray();

            return new()
            {
                Shatters = resultsList.Sum(x => x.Shatter),
                Successes = resultsList.Count(x => x.Success),
                TotalFavor = new()
                {
                    Average = resultsList.Average(x => x.TotalFavor),
                    StandardDeviation = Statistics.StandardDeviation(resultsList.Select(x => x.TotalFavor))
                },
                TotalCasts = new()
                {
                    Average = resultsList.Average(x => x.TotalCasts),
                    StandardDeviation = Statistics.StandardDeviation(resultsList.Select(x => (double)x.TotalCasts))
                },
                TotalDispels = new()
                {
                    Average = resultsList.Average(x => x.TotalDispels),
                    StandardDeviation = Statistics.StandardDeviation(resultsList.Select(x => (double)x.TotalDispels))
                }
            };
        }

        public double GetProgress()
        {
            return (double)completedSimulations.Count() / targetSims;
        }

        public async Task<SimulationResult> RunScenarioSimulationAsync(Scenario scenario, int simulation, Action<double> callback)
        {
            await Task.Yield();
            SimulationResult result = RunScenarioSimulation(scenario);
            completedSimulations.Add(simulation);
            callback(GetProgress());
            return result;
        }

        public static SimulationResult RunScenarioSimulation(Scenario scenario)
        {
            SimulationResult result = new();

            bool isCurrentItem = false;
            double currentCastPower = 0;

            bool completed = false;

            while (!completed)
            {
                if (isCurrentItem && currentCastPower > scenario.DispellThreshold)
                {
                    result.TotalFavor += WurmSkillUtil.DispelFavorCost;
                    result.TotalDispels++;
                    bool dispelled = WurmSkillUtil.DispelCheck(currentCastPower, scenario.ChannelingSkill, scenario.SpellDifficulty, scenario.Bonus, scenario.Bene);
                    if (dispelled)
                    {
                        isCurrentItem = false;
                        currentCastPower = 0;
                    }
                }
                else
                {
                    result.TotalFavor += scenario.SpellFavor;
                    result.TotalCasts++;
                    double power = WurmSkillUtil.SkillCheck(scenario.ChannelingSkill, scenario.SpellDifficulty, scenario.Bonus) + (scenario.Bene ? 5 : 0);

                    if (power >= currentCastPower)
                    {
                        if (isCurrentItem)
                        {
                            currentCastPower = WurmSkillUtil.GetFinalPower((float)currentCastPower, (float)power, scenario.Bene);
                        }
                        else
                        {
                            currentCastPower = power;
                            isCurrentItem = true;
                        }

                        if (currentCastPower >= scenario.TargetPower)
                        {
                            completed = true;
                            result.Success = true;
                        }
                    }
                    else if (power < 0)
                    {
                        if (WurmSkillUtil.IsShatter(power, scenario.ItemQL))
                        {
                            if (scenario.ShatterType == ShatterType.Replace)
                            {
                                isCurrentItem = false;
                                currentCastPower = 0;
                                result.Shatter += 1;
                            }
                            else if (scenario.ShatterType != ShatterType.Seryll)
                            {
                                result.Shatter += 1;
                                completed = true;

                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
