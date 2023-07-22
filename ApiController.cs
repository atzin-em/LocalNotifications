using System.Threading.Tasks;
using Refit;
using System.Collections.Generic;
using System.Net.Http;

namespace LocalNotifications.Resources
{
    public class ApiController
    {
        // Private fields to store API related data
        private string apiUrl;
        private string apiKeyHeader;
        private string apiKey;

        // Public properties with custom logic for API related data
        public string Api
        {
            get { return apiUrl; }
            set { apiUrl = value; }
        }

        public string KeyHeader
        {
            get { return apiKeyHeader; }
            set { apiKeyHeader = value; }
        }

        public string Key
        {
            get { return apiKey.Length == 0 ? "UNSET" : "SET"; }
            set { apiKey = value; }
        }

        // Constructor for the ApiController class
        public ApiController(string Url = "", string ApiKeyHeader = "", string ApiKey = "")
        {
            this.Api = Url;
            this.KeyHeader = ApiKeyHeader;
            this.Key = ApiKey;
        }

        // Refit interface defining API endpoints and methods
        public interface IApiService
        {
            [Get("/{endpoint}")]
            Task<string> GetEndpointData(string endpoint);

            [Get("/{endpoint}")]
            Task<string> GetEndpointData(string endpoint, [HeaderCollection] IDictionary<string, string> headers);
        }

        // Method to perform a GET request to the API endpoint
        public async Task<string> GetRequest()
        {
            // Check if the API URL is set
            if (this.Api != "")
            {
                // Create an instance of the Refit interface for the API service
                var apiService = RestService.For<IApiService>(Api);
                try
                {
                    if (this.Key == "SET")
                    {
                        // If API key is set, add it as a header and make the request
                        return await apiService.GetEndpointData("/", new Dictionary<string, string>() { { this.apiKeyHeader, this.apiKey } });
                    }
                    else
                    {
                        // If API key is not set, make the request without additional headers
                        return await apiService.GetEndpointData("/");
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Handle any HTTP request errors and return an error message
                    return $"ERROR: {ex.Message}";
                }
            }
            // Return an error message if API URL is not set
            return "ERROR: API URL NOT SET";
        }
    }
}
