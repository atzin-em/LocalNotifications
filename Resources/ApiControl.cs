using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace LocalNotifications.Resources
{
    internal class ApiControl
    {
        private string ApiUrl = "";
        public ApiControl(string url) 
        { 
            this.SetUrl(url);
        }
        
        public string GetUrl()
        {
            return ApiUrl;
        }

        public void SetUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                this.ApiUrl = url;
            }
        }

        public async Task<string> GetApiResponseAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Make a GET request to the API endpoint
                    HttpResponseMessage response = await client.GetAsync(ApiUrl);

                    // Ensure the response is successful
                    response.EnsureSuccessStatusCode();

                    // Read the response content as string
                    string responseContent = await response.Content.ReadAsStringAsync();

                    return responseContent;
                }
                catch (Exception ex)
                {
                    // Handle any exceptions that occurred during the request
                    Console.WriteLine($"Error: {ex.Message}");
                    return null;
                }
            }
        }
    }
}