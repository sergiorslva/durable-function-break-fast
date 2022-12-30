using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace breakfast_function
{
    public static class ParallelFunction
    {
        [FunctionName("Function_Breakfast_FanOutFanIn")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context,
            ILogger log)
        {            
            var parallelTasks = new List<Task<string>>();

            parallelTasks.Add(context.CallActivityAsync<string>("FanOutFanInBreakFast", "Coffee is ready"));
            parallelTasks.Add(context.CallActivityAsync<string>("FanOutFanInBreakFast", "Eggs are ready"));
            parallelTasks.Add(context.CallActivityAsync<string>("FanOutFanInBreakFast", "Bacon is ready"));
            parallelTasks.Add(context.CallActivityAsync<string>("FanOutFanInBreakFast", "Toast is ready"));

            var outputs = await Task.WhenAll(parallelTasks);
            return outputs.ToList();
        }

        [FunctionName("FanOutFanInBreakFast")]
        public static async Task<string> FanOutFanInBreakFast([ActivityTrigger] string step, ILogger log)
        {            
            await Task.Delay(Random.Shared.Next(5000, 10000));
            log.LogInformation(step);
            return step;
        }        

        [FunctionName("Function_Start_FanOutFanIn")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function_Breakfast_FanOutFanIn", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}