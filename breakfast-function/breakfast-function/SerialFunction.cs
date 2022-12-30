using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace breakfast_function
{
    public static class SerialFunction
    {
        [FunctionName("Function_Breakfast_Chaining")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();
            
            outputs.Add(await context.CallActivityAsync<string>("ChainingBreakFast", "Coffee is ready"));
            outputs.Add(await context.CallActivityAsync<string>("ChainingBreakFast", "Eggs are ready"));
            outputs.Add(await context.CallActivityAsync<string>("ChainingBreakFast", "Bacon is ready"));
            outputs.Add(await context.CallActivityAsync<string>("ChainingBreakFast", "Toast is ready"));            

            return outputs;
        }

        [FunctionName("ChainingBreakFast")]
        public static string Function_PourCoffee_Serial([ActivityTrigger] string step, ILogger log)
        {
            log.LogInformation(step);
            return step;
        }       

        [FunctionName("Function_Start_Chaining")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("Function_Breakfast_Chaining", null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}