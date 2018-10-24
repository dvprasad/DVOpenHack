using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;

namespace DVOpenHack
{
    public static class GetRatings
    {
        private static string BaseUrl = @"https://serverlessohuser.trafficmanager.net/api/GetUser";
        private static string GetUserUri(Guid userId) => $"GetUser?userId={userId}";

        [FunctionName("GetRatings")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string userId = req.Query["userId"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            userId = userId ?? data?.userId;

            //return userId != null
            //    ? (ActionResult)new OkObjectResult($"Hello, {userId}")
            //    : new BadRequestObjectResult("Please pass a userId on the query string or in the request body");


            Guid userguid = Guid.Parse(userId);

            if (!await ValidateUser(userguid)) return new BadRequestObjectResult("UserId not found");

            var result = (new Repository()).GetRatingsForUser(userguid);
            return (ActionResult)new OkObjectResult(result);
        }


        private static Uri GetUserUrl(Guid userId)
        {
            return new Uri(BaseUrl + "?userId=" + userId);
        }

        private static async Task<bool> ValidateUser(Guid userId)
        {
            bool valid = false;
            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(BaseUrl);
            HttpResponseMessage response = await httpClient.GetAsync(GetUserUri(userId));
            if (response.IsSuccessStatusCode)
            {
                GetProductsUser user = await response.Content.ReadAsAsync<GetProductsUser>();
                valid = user != null;
            }

            return valid;
        }


    }


    public class GetProductsUser
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
    }
}
