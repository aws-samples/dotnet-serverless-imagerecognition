using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Microsoft.Extensions.Caching.Memory;

namespace Common
{
    public class CommunicationManager
    {
        private const string ConnectionIdField = "connectionId";
        private const string UsernameField = "username";
        private const string EndpointField = "endpoint";
        private const string LoginDateField = "logindate";

        private readonly MemoryCache _connectionsCache = new MemoryCache(new MemoryCacheOptions());
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
            return new CommunicationManager(awsCredentials, region, ddbTableName);
        }

        public static CommunicationManager CreateManager(string ddbTableName)
        {
            return CreateManager(FallbackCredentialsFactory.GetCredentials(), FallbackRegionFactory.GetRegionEndpoint(),
                ddbTableName);
        }

        public async Task SendMessage(MessageEvent evnt)
        {
            if (string.IsNullOrEmpty(_ddbTableName))
                return;

            var payload = JsonSerializer.Serialize(evnt);
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(payload));


            QueryResponse queryResponse;
            if (!_connectionsCache.TryGetValue(evnt.TargetUser, out ICacheEntry entry) ||
                entry.AbsoluteExpiration < DateTime.UtcNow)
            {
                var queryRequest = new QueryRequest
                {
                    TableName = _ddbTableName,
                    IndexName = "username",
                    KeyConditionExpression = $"{UsernameField} = :u",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {":u", new AttributeValue {S = evnt.TargetUser}}
                    }
                };
                queryResponse = await _ddbClient.QueryAsync(queryRequest);

                entry = _connectionsCache.CreateEntry(evnt.TargetUser);
                entry.AbsoluteExpiration = DateTime.UtcNow.AddSeconds(10);
                entry.SetValue(queryResponse);
                _connectionsCache.Set(evnt.TargetUser, entry);
            }
            else
            {
                queryResponse = entry.Value as QueryResponse;
            }

            AmazonApiGatewayManagementApiClient apiClient = null;
            try
            {
                var goneConnections = new List<Dictionary<string, AttributeValue>>();
                foreach (var item in queryResponse.Items)
                {
                    var endpoint = item[EndpointField].S;

                    if (apiClient == null || !apiClient.Config.ServiceURL.Equals(endpoint, StringComparison.Ordinal))
                    {
                        if (apiClient != null)
                        {
                            apiClient.Dispose();
                            apiClient = null;
                        }

                        apiClient = new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig
                        {
                            ServiceURL = endpoint
                        });
                    }

                    var connectionId = item[ConnectionIdField].S;

                    stream.Position = 0;
                    var postConnectionRequest = new PostToConnectionRequest
                    {
                        ConnectionId = connectionId,
                        Data = stream
                    };

                    try
                    {
                        await apiClient.PostToConnectionAsync(postConnectionRequest);
                    }
                    catch (GoneException)
                    {
                        goneConnections.Add(item);
                    }
                }

                // Remove connections from the cache that have disconnected.
                foreach (var goneConnectionItem in goneConnections) queryResponse.Items.Remove(goneConnectionItem);
            }
            catch
            {
                // Never stop rendering based on communication errors.
            }
            finally
            {
                apiClient?.Dispose();
            }
        }
    }
}