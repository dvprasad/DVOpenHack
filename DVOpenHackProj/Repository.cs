
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace DVOpenHack
{
    internal class Repository
    {
        private const string EndpointUri = "https://wkopenhack.documents.azure.com:443/";
        private const string PrimaryKey = "aFUhvbv4MX2wgJYwo9FiAxFNddAcmsiYZVointIJftDAg4KF1MLuJmFBG7CG0TCbw7XaLY6oCeoy9QoIl5aRVw==";
        private DocumentClient client;
        private string dbName = "OpenHack";
        private string ratingsCollectionName = "Ratings";

        public Repository()
        {
            this.client = new DocumentClient(new Uri(EndpointUri), PrimaryKey);
        }

        public async Task CreateDb()
        {
            try
            {
                await this.client.CreateDatabaseIfNotExistsAsync(new Database { Id = dbName });
                await this.client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(dbName), new DocumentCollection { Id = ratingsCollectionName });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        public async Task AddRating(RatingResponse rating)
        {
            var input = new RatingCosmosResponse { Id = rating.Id, LocationName = rating.LocationName, ProductId = rating.ProductId, Rating = rating.Rating, TimeStamp = rating.TimeStamp, UserId = rating.UserId, UserNotes = rating.UserNotes };
            await this.client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(dbName, ratingsCollectionName), input);
        }
        public async Task<RatingResponse> GetRating(Guid ratingId)
        {
            return await client.ReadDocumentAsync<RatingResponse>(UriFactory.CreateDocumentUri(dbName, ratingsCollectionName, ratingId.ToString()));

            //////return (CreateRating.Rating)(dynamic)response.Resource;

            ///// FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };

            //FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
            //IQueryable<RatingResponse> RatingQuery = this.client.CreateDocumentQuery<RatingResponse>(
            //    UriFactory.CreateDocumentCollectionUri(dbName, ratingsCollectionName), queryOptions).Where(r => r.Id == ratingId);
            //return RatingQuery.ToList().FirstOrDefault();
        }

        public IEnumerable<RatingResponse> GetRatingsForUser(Guid userId)
        {
            try
            {
                FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true };
                IQueryable<RatingResponse> ratingQuery = this.client.CreateDocumentQuery<RatingResponse>(
                    UriFactory.CreateDocumentCollectionUri(dbName, ratingsCollectionName), queryOptions).Where(r => r.UserId == userId);
                return ratingQuery.ToList();
            }
            catch (DocumentClientException de)
            {
                return new List<RatingResponse>();
            }

        }

        public class RatingCosmosResponse
        {
            [JsonProperty(PropertyName = "id")]
            public Guid Id { get; set; }
            public Guid UserId { get; set; }
            public Guid ProductId { get; set; }
            public string LocationName { get; set; }
            public int Rating { get; set; }
            public string UserNotes { get; set; }
            public DateTime TimeStamp { get; set; }
        }

    }
}
