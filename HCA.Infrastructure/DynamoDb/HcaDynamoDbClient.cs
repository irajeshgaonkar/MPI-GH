using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

namespace HCA.Infrastructure.DynamoDb
{
    public class HcaDynamoDbClient
    {
        private readonly DynamoDBContext _dbContex;

        private readonly AmazonDynamoDBClient _dynamoDBClient;

        public HcaDynamoDbClient()
        {
            AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();
            _dynamoDBClient = new AmazonDynamoDBClient(clientConfig);
            _dbContex = new DynamoDBContext(_dynamoDBClient);
        }


        public async Task<T> CreateItem<T>(T item)
        {
            //await _dbContex.SaveAsync(item);
            return item;
        }

        public async Task<List<T>> Query<T>(string? hashKey)
        {
            var operation = _dbContex.QueryAsync<T>(hashKey);
            var result =  await operation.GetRemainingAsync();
            return result;
        }

        public async Task<(List<Dictionary<string, AttributeValue>>, Dictionary<string, AttributeValue>)> ScanAsync(ScanRequest scanRequest)
        {
            var searchResult = await _dynamoDBClient.ScanAsync(scanRequest);
            return (searchResult.Items, searchResult.LastEvaluatedKey);
        }
    }
}
