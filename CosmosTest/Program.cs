using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CosmosTest
{
    class Program
    {
        static readonly string AccountUri = "https://localhost:8081";
        static readonly string AccountKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

        static readonly string Database = "testDatabase";
        static readonly string Collection = "testCollection";
        static readonly bool CreateTestDocument = true; // Set this to false if you don't want to create a test document

        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            using (var client = new DocumentClient(new Uri(AccountUri), AccountKey, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() }))
            {
                var databaseUri = UriFactory.CreateDatabaseUri(Database);
                var collectionUri = UriFactory.CreateDocumentCollectionUri(Database, Collection);

                #region Setup

                await client.CreateDatabaseIfNotExistsAsync(new Database { Id = Database });

                await client.CreateDocumentCollectionIfNotExistsAsync(databaseUri, new DocumentCollection
                {
                    Id = Collection,
                    PartitionKey = new PartitionKeyDefinition
                    {
                        Paths = new System.Collections.ObjectModel.Collection<string> { "/partitionKey" }
                    }
                });


                if (CreateTestDocument)
                {
                    var doc = new TestModel
                    {
                        Name = "Test",
                        PartitionKey = "Default",
                        Value = 123
                    };

                    var result = await client.CreateDocumentAsync(collectionUri, doc);
                }

                #endregion


                // Basic linq query without type constraints

                Console.WriteLine("Basic query:");

                var basicQuery = client.CreateDocumentQuery<TestModel>(collectionUri, new FeedOptions { PartitionKey = new PartitionKey("Default") })
                    .Where(x => x.Name == "Test")
                    .ToList();

                foreach (var item in basicQuery)
                    Console.WriteLine(item.ToString());

                if (basicQuery.Count == 0)
                    Console.WriteLine("No results.");


                // Generic method query with type constraints

                Console.WriteLine();
                Console.WriteLine("Generic method query:");

                var genericMethodQuery = QueryStuff<TestModel>(client, collectionUri, "Default", "Test");

                foreach (var item in genericMethodQuery)
                    Console.WriteLine(item.ToString());

                if (genericMethodQuery.Count == 0)
                    Console.WriteLine("No results.");


                // Generic class query with type constraints

                Console.WriteLine();
                Console.WriteLine("Generic class query:");

                var service = new TestService<TestModel>(client);
                var genericClassQuery = service.QueryStuff(collectionUri, "Default", "Test");

                foreach (var item in genericClassQuery)
                    Console.WriteLine(item.ToString());

                if (genericClassQuery.Count == 0)
                    Console.WriteLine("No results.");
            }


            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadLine();
        }

        private static List<T> QueryStuff<T>(DocumentClient client, Uri collectionUri, string partitionKey, string name) where T : ITestInterface
        {
            var queryResults = client.CreateDocumentQuery<T>(collectionUri, new FeedOptions { PartitionKey = new PartitionKey(partitionKey) })
                .Where(x => x.Name == name)
                .ToList();

            return queryResults;
        }
    }
}
