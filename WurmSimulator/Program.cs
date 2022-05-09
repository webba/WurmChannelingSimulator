// See https://aka.ms/new-console-template for more information
using MathNet.Numerics.Statistics;
using WurmSimulator.Lib;

Console.WriteLine("Hello, World!");


var shatterMode = "Restart";

List<Scenario> scenarioList = new List<Scenario>()
{
    new Scenario()
    {
        TargetPower = 109,
        DispellThreshold = 20
    },
};

foreach (Scenario scenario in scenarioList)
{

    double channelingSkill = scenario.ChannelingSkill;
    bool bene = scenario.Bene;
    double bonus = scenario.Bonus;
    double targetPower = scenario.TargetPower;
    double dispellThreshold = scenario.DispellThreshold;
    double itemQL = scenario.ItemQL;

    int simulations = 10000;

    List<double> totalFavorList = new();
    List<double> totalSkillCastsList = new();
    List<double> totalDispelCastsList = new();
    int totalShatters = 0;


    for (int i = 0; i < simulations; i++)
    {

        int shatters = 0;
        int skillCasts = 0;
        int dispelCasts = 0;
        double totalFavor = 0;

        bool isCurrentItem = false;
        double currentCastPower = 0;

        bool completed = false;

        while (!completed)
        {
            if (isCurrentItem && currentCastPower > dispellThreshold)
            {
                totalFavor += WurmSkillUtil.DispelFavorCost;
                dispelCasts++;
                bool dispelled = WurmSkillUtil.DispelCheck(currentCastPower, channelingSkill, WurmSkillUtil.CocDifficulty, bonus, bene);
                if (dispelled)
                {
                    isCurrentItem = false;
                    currentCastPower = 0;
                }
            }
            else
            {
                totalFavor += WurmSkillUtil.CocFavorCost;
                skillCasts++;
                double power = WurmSkillUtil.SkillCheck(channelingSkill, WurmSkillUtil.CocDifficulty, bonus) + (bene ? 5 : 0);

                if (power >= currentCastPower)
                {
                    if (isCurrentItem)
                    {
                        currentCastPower = WurmSkillUtil.GetFinalPower((float)currentCastPower, (float)power, bene);
                    }
                    else
                    {
                        currentCastPower = power;
                        isCurrentItem = true;
                    }

                    if (currentCastPower >= targetPower)
                    {
                        completed = true;
                    }
                }
                else if (power < 0)
                {
                    if (WurmSkillUtil.IsShatter(power, itemQL))
                    {
                        if (shatterMode == "Restart")
                        {
                            isCurrentItem = false;
                            currentCastPower = 0;
                            shatters += 1;
                            totalShatters += 1;
                        }
                        else
                        {
                            isCurrentItem = false;
                            currentCastPower = 0;
                            shatters += 1;
                            totalShatters += 1;
                            completed = true;

                        }
                    }
                }
            }
        }

        totalFavorList.Add(totalFavor);
        totalSkillCastsList.Add(skillCasts);
        totalDispelCastsList.Add(dispelCasts);
    }
    Console.WriteLine("{0};{1};{2};{3};{4};{5};{6};{7};{8}",
        scenario.GetDescriptor(),
        simulations,
        totalFavorList.Average(),
        Statistics.StandardDeviation(totalFavorList),
        totalSkillCastsList.Average(),
        Statistics.StandardDeviation(totalSkillCastsList),
        totalDispelCastsList.Average(),
        Statistics.StandardDeviation(totalDispelCastsList),
        (double)totalShatters / (double)simulations);
}