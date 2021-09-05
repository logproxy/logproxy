using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LogProxy.Messages;
using Microsoft.Extensions.Options;

namespace LogProxy.Services
{
    public interface IAirTableAccess
    {
        Task<IEnumerable<AirTableResponse>> GetAllAsync();
        Task PostAsync(AirTableRequest requestMessage);
    }

    public class AirTableAccess : IAirTableAccess
    {
        private readonly HttpClient _httpClient;
        private readonly AirTableAccessConfig _config;

        public AirTableAccess(HttpClient httpClient, IOptions<AirTableAccessConfig> options)
        {
            _httpClient = httpClient;
            _config = options.Value;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _config.ApiKey);
        }

        public async Task<IEnumerable<AirTableResponse>> GetAllAsync()
        {
            var responses = new List<AirTableResponse>();
            string offset = null;
            do
            {
                var url = _config.AirTableUrl;
                if (offset != null)
                    url = $"{_config.AirTableUrl}?offset={offset}";
                var response = await _httpClient.GetFromJsonAsync<AirTableResponse>(url,
                    new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
                responses.Add(response);
                offset = response?.Offset;
            } while (!string.IsNullOrWhiteSpace(offset));

            return responses;
        }

        public async Task PostAsync(AirTableRequest requestMessage)
        {
            var response = await _httpClient.PostAsync(_config.AirTableUrl,
                new StringContent(JsonSerializer.Serialize(requestMessage), Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var responseMessage = await response.Content.ReadFromJsonAsync<AirTableResponse>();

            var elementsInRequestButNotInResponse = Diff(requestMessage.Records.Select(record => record.Fields),
                responseMessage?.Records.Select(record => record.Fields)).ToList();
            if (elementsInRequestButNotInResponse.Any())
            {
                var jsonString = JsonSerializer.Serialize(elementsInRequestButNotInResponse);
                throw new AirTablePostException($"failed to post: {jsonString}");
            }
        }

        private IEnumerable<Fields> Diff(IEnumerable<Fields> requested, IEnumerable<Fields> responded)
        {
            return requested.Where(requestField =>
                !responded.Any(responseField =>
                    responseField.Message == requestField.Message &&
                    responseField.Summary == requestField.Summary));
        }
    }

    public class AirTableAccessConfig
    {
        public string AirTableUrl { get; set; }
        public string ApiKey { get; set; }
    }

    public class AirTablePostException : Exception
    {
        public AirTablePostException(string message) : base(message)
        {
        }
    }
}