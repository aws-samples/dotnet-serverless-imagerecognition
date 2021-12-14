using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Caching.Memory;

namespace ImageRecognition.Communication.Functions
{
    public class CommunicationManager
    {
        private const string ConnectionIdField = "connectionId";
        private const string UsernameField = "username";
        private const string EndpointField = "endpoint";
        private const string LoginDateField = "logindate";

        private MemoryCache _connectionsCache = new(new MemoryCacheOptions());
        private readonly IAmazonDynamoDB _ddbClient;


        private readonly string _ddbTableName;

        private CommunicationManager(AWSCredentials awsCredentials, RegionEndpoint region, string ddbTableName)
        {
            _ddbTableName = ddbTableName;
            _ddbClient = new AmazonDynamoDBClient(awsCredentials, region);
        }


        public static CommunicationManager CreateManager(AWSCredentials awsCredentials, RegionEndpoint region,
            string ddbTableName)
        {
            return new(awsCredentials, region, ddbTableName);
        }

        public static CommunicationManager CreateManager(string ddbTableName)
        {
            return CreateManager(FallbackCredentialsFactory.GetCredentials(), FallbackRegionFactory.GetRegionEndpoint(),
                ddbTableName);
        }


        public async Task LoginAsync(string connectionId, string endpoint, string username)
        {
            if (string.IsNullOrEmpty(_ddbTableName))
                return;

            var putRequest = new PutItemRequest
            {
                TableName = _ddbTableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {ConnectionIdField, new AttributeValue {S = connectionId}},
                    {EndpointField, new AttributeValue {S = endpoint}},
                    {UsernameField, new AttributeValue {S = username}},
                    {LoginDateField, new AttributeValue {S = DateTime.UtcNow.ToString()}}
                }
            };

            await _ddbClient.PutItemAsync(putRequest);
        }

        public async Task LogoffAsync(string connectionId)
        {
            if (string.IsNullOrEmpty(_ddbTableName))
                return;

            var deleteRequest = new DeleteItemRequest
            {
                TableName = _ddbTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    {ConnectionIdField, new AttributeValue {S = connectionId}}
                }
            };

            await _ddbClient.DeleteItemAsync(deleteRequest);
        }
    }
}