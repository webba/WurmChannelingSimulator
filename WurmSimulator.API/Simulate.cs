using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WurmSimulator.Lib;

namespace WurmSimulator.API
{
    public static class Simulate
    {
        [FunctionName("Example")]
        public static async Task<IActionResult> Example(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)][FromBody] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# Running Example.");

            return new OkObjectResult(new Scenario());
        }
        [FunctionName("Simulate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)][FromBody] Scenario scenario,
            ILogger log)
        {
            log.LogInformation("C# Running Simulation.");

            if(scenario == null)
            {
                return new BadRequestResult();
            }

            int maxSims = 100_000;
            if (scenario.Simulations > maxSims)
            {
                scenario.Simulations = maxSims;
            }
            
            var results = WurmSkillUtil.RunScenario(scenario);

            return new OkObjectResult(results);
        }
    }
}
