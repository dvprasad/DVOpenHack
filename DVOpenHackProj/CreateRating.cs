using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;

namespace DVOpenHack
{
    public static class CreateRating
    {



        public const string BaseUri = "https://serverlessohproduct.trafficmanager.net/api/GetProduct";

        private static string GetProductUri(Guid productId) =>
            $"GetProduct?productId={productId}";

        [FunctionName("CreateRating")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //log.LogInformation("C# HTTP trigger function processed a request.");

            //string name = req.Query["name"];

            //string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            //dynamic data = JsonConvert.DeserializeObject(requestBody);
            //name = name ?? data?.name;

            //return name != null
            //    ? (ActionResult)new OkObjectResult($"Hello, {name}")
            //    : new BadRequestObjectResult("Please pass a name on the query string or in the request body");



            log.LogInformation("C# HTTP trigger function processed a request.");
            //var request = await req.Content.ReadAsAsync<PostRatingRequest>();
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            PostRatingRequest request = JsonConvert.DeserializeObject<PostRatingRequest>(requestBody);
            //name = name ?? data?.name;


            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(BaseUri);

            //validate the product exists
            var getProductResponse = await httpClient.GetAsync(GetProductUri(request.ProductId));
            if (!getProductResponse.IsSuccessStatusCode)
            {
                return new BadRequestObjectResult(new ArgumentException("the provided product id is invalid or does not exist", nameof(request.ProductId)));
            }

            //if (!IsRatingValid(request)) return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Rating must be between 0 and 5");
            //if (request.UserId == default(Guid)) return req.CreateErrorResponse(HttpStatusCode.BadRequest, "UserId must be provided");
            //if (!await IsValidUser(request)) return req.CreateErrorResponse(HttpStatusCode.BadRequest, "User is not valid");
            var rating = new RatingResponse
            {
                Id = Guid.NewGuid(),
                LocationName = request.LocationName,
                ProductId = request.ProductId,
                Rating = request.Rating,
                UserId = request.UserId,
                UserNotes = request.UserNotes,
                // TODO set this to the timestamp in the data store
                TimeStamp = DateTime.UtcNow
            };

            await new Repository().AddRating(rating);

            return (ActionResult)new OkObjectResult(rating);
        }
        private static async Task<bool> IsValidUser(PostRatingRequest request)
        {
            var client = new HttpClient { BaseAddress = new Uri("https://serverlessohuser.trafficmanager.net/api/") };
            return (await client.GetAsync($"GetUser?userId={request.UserId}")).StatusCode == HttpStatusCode.OK;
        }
        private static bool IsRatingValid(PostRatingRequest request) => request.Rating > 0 && request.Rating <= 5;
    }
}
