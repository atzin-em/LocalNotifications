using Refit;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace LocalNotifications.Resources
{
    public class ApiController
    {
        private string apiUrl;
        public string Api
        {
            get { return apiUrl; }
            set { apiUrl = value; }
        }
        private string apiKeyHeader;
        public string KeyHeader
        {
            get { return apiKeyHeader; }
            set { apiKeyHeader = value; }
        }

        private string apiKey;
        public string Key
        {
            get { return apiKey.Length == 0 ? "UNSET" : "SET"; }
            set { apiKey = value; }
        }

        public ApiController(string Url = "", string ApiKeyHeader = "", string ApiKey = "")
        {
            this.Api = Url;
            this.KeyHeader = ApiKeyHeader;
            this.Key = ApiKey;
        }

        public interface IApiService
        {
            [Get("/{endpoint}")]
            Task<string> GetEndpointData(string endpoint);
            Task<string> GetEndpointData(string endpoint, [HeaderCollection] IDictionary<string, string> headers);

        }

        public async Task<string> GetRequest()
        {
            if (this.Api != "")
            {
                var apiService = RestService.For<IApiService>(Api);
                try
                {
                    if (this.Key == "SET")
                    {
                        return await apiService.GetEndpointData("/", new Dictionary<string, string>() { { this.apiKeyHeader, this.apiKey } });
                    }
                    else
                    {
                        return await apiService.GetEndpointData("/");
                    }
                }
                catch (HttpRequestException ex)
                {
                    return $"ERROR: {ex.Message}";
                }
            }
            return "ERROR: API URL NOT SET";
        }
    }
}