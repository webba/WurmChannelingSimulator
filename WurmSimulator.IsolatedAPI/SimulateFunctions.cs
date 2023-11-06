using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using WurmSimulator.Lib;

namespace WurmSimulator.IsolatedAPI
{
    public class SimulateFunctions
    {
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _serializerOptions;

        public SimulateFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SimulateFunctions>();
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        [Function("Example")]
        public HttpResponseData Example([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# Running Example.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            response.WriteString(JsonSerializer.Serialize(new Scenario(), _serializerOptions));
            return response;
        }

        [Function("Simulate")]
        public async Task<HttpResponseData> Simulate([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# Running Simulate."); 

            var scenario = await JsonSerializer.DeserializeAsync<Scenario>(req.Body, _serializerOptions);

            if (scenario == null)
            {
                var bresponse = req.CreateResponse(HttpStatusCode.BadRequest);
                return bresponse;
            }

            int maxSims = 100_000;
            if (scenario.Simulations > maxSims)
            {
                scenario.Simulations = maxSims;
            }

            var results = WurmSkillUtil.RunScenario(scenario);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");


            response.WriteString(JsonSerializer.Serialize(results));
            return response;
        }
    }
}
