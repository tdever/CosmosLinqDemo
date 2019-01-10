using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosTest
{
    public class TestService<T> where T : ITestInterface
    {
        public TestService(DocumentClient client)
        {
            Client = client;
        }

        public DocumentClient Client { get; set; }

        public List<T> QueryStuff(Uri collectionUri, string partitionKey, string name)
        {
            var queryResults = Client.CreateDocumentQuery<T>(collectionUri, new FeedOptions { PartitionKey = new PartitionKey(partitionKey) })
                .Where(x => x.Name == name)
                .ToList();

            return queryResults;
        }
    }
}
