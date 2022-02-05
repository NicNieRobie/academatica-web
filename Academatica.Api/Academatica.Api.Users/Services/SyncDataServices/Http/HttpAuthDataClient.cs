using Academatica.Api.Users.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Academatica.Api.Users.Services.SyncDataServices.Http
{
    public class HttpAuthDataClient: IAuthDataClient
    {
        public readonly HttpClient _httpClient;

        public HttpAuthDataClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendEmailConfirmation(SendConfirmationEmailRequestDto requestDto)
        {
            var content = new StringContent(JsonSerializer.Serialize(requestDto), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://localhost:5001/connect/send-confirmation", content);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Sync POST successful.");
            } else
            {
                Console.WriteLine("Sync POST unsuccessful.");
            }
        }
    }
}
