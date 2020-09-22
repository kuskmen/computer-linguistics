using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

namespace computer_linguistics
{
    public class Crawler
    {
        private readonly ILogger logger;
        private readonly CrawlerOptions crawlerOptions;
        private readonly IMongoClient mongoClient;
        private readonly IMongoDatabase database;
        private readonly IMongoCollection<BsonDocument> collection;
        private readonly Regex urlRegex =
            new Regex("(http|https):\\/\\/([\\w_-]+(?:(?:\\.[\\w_-]+)+))([\\w.,@?^=%&:/~+#-]*[\\w@?^=%&/~+#-])?",
                      RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10));

        public Crawler(ILogger logger, CrawlerOptions crawlerOptions)
        {
            this.crawlerOptions = crawlerOptions;
            this.logger = logger;

            // TODO: hardcode, ow yeah.
            this.mongoClient = new MongoClient("mongodb://localhost:27017");
            this.database = this.mongoClient.GetDatabase("crawler-stuff");
            this.collection = this.database.GetCollection<BsonDocument>("matches");
        }

        public async Task Start(string url)
        {
            using (var webclient = new WebClient())
            {
                this.logger.LogDebug($"Downloading content for url : {url} ...");
                var websiteContent = await webclient.DownloadStringTaskAsync(url);
                await SaveFinds(websiteContent, url);

                var urls = this.urlRegex.Matches(websiteContent);
                var tasks = new Task[urls.Count];

                for (var i = 0; i < urls.Count; ++i)
                {
                    tasks[i] = Start(urls[i].Value);
                }

                var processed = 0;
                while (processed < urls.Count)
                {
                    var bulk = tasks.Skip(processed).Take(Environment.ProcessorCount - 1).ToArray();
                    await Task.WhenAll(bulk);
                    processed += bulk.Length;
                }
            }
        }

        private async Task SaveFinds(string websiteContent, string url)
        {
            if (string.IsNullOrEmpty(websiteContent))
            {
                this.logger.LogDebug($"Website content for {url} is empty or null.");
                return;
            }

            var matches = this.crawlerOptions.SearchPattern.Matches(websiteContent);
            if (matches.Count != 0)
            {
                var matchesArray = new Match[matches.Count];
                matches.CopyTo(matchesArray, 0);

                var documents = new List<BsonDocument>(matches.Count);
                foreach (var match in matchesArray)
                {
                    documents.Add(
                        new BsonDocument
                        {
                            { "url",   url },
                            { "match", match.Value }
                        }
                    );
                    logger.LogDebug($"Created document url: {url}, match: {match.Value}");
                }

                if (documents.Count != 0)
                {
                    await this.collection.InsertManyAsync(documents);
                }
            }
        }
    }
    public class CrawlerOptions
    {
        public Uri StartingPoint { get; set; }

        public Regex SearchPattern { get; set; }

        public static CrawlerOptions Invalid { get; } = new CrawlerOptions { SearchPattern = null, StartingPoint = null };

        public bool IsValid => SearchPattern != null && StartingPoint != null;
    }
}