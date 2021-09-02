using System;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LogProxy.Messages;
using LogProxy.Services;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Record = LogProxy.Messages.Record;

namespace LogProxy.IntegrationTests
{
    public class AirTableAccessTests
    {
        [Fact(Skip = "will only work if api key is set")]
        public async Task GetAllAsync_GetResultsFromAirTable_NotNull()
        {
            var airTableAccess = GetAirTableAccess();

            var result = await airTableAccess.GetAllAsync();

            Assert.NotNull(result);
        }

        [Fact(Skip = "will only work if api key is set")]
        public async Task GetAllAsync_GetResultsFromAirTable_ResponsesNotEmpty()
        {
            var airTableAccess = GetAirTableAccess();

            var result = (await airTableAccess.GetAllAsync()).ToList();

            Assert.NotEmpty(result);
            Assert.NotEmpty(result.First().Records);
        }

        [Fact(Skip = "will only work if api key is set")]
        public async Task PostAsync_PostOneThing_VerifyNewElementPresent()
        {
            var airTableAccess = GetAirTableAccess();
            var id = Guid.NewGuid().ToString();
            var messageAndSummary = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);

            await airTableAccess.PostAsync(GetRequest(new Fields
            {
                Id = id,
                Message = messageAndSummary,
                Summary = messageAndSummary,
                ReceivedAt = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
            }));

            var allResponses = await airTableAccess.GetAllAsync();
            Assert.Contains(allResponses.SelectMany(response => response.Records).Select(record => record.Fields),
                field => field.Id == id && field.Message == messageAndSummary && field.Summary == messageAndSummary);
        }

        [Fact(Skip = "will only work if api key is set")]
        public async Task PostAsync_PostThreeThings_VerifyAllPresent()
        {
            var airTableAccess = GetAirTableAccess();
            var id = Guid.NewGuid().ToString();
            var messageAndSummary = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture);
            var field = new Fields
            {
                Id = id,
                Message = messageAndSummary,
                Summary = messageAndSummary,
                ReceivedAt = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
            };

            await airTableAccess.PostAsync(GetRequest(field, field, field));

            var allResponses = await airTableAccess.GetAllAsync();
            Assert.Equal(3,
                allResponses.SelectMany(response => response.Records).Select(record => record.Fields).Count(f =>
                    f.Id == id && f.Message == messageAndSummary && f.Summary == messageAndSummary));
        }

        private static IAirTableAccess GetAirTableAccess()
        {
            var configMock = new Mock<IOptions<AirTableAccessConfig>>();
            configMock.Setup(x => x.Value).Returns(new AirTableAccessConfig
            {
                ApiKey = "set_me",
                AirTableUrl = "https://api.airtable.com/v0/appD1b1YjWoXkUJwR/Messages"
            });
            return new AirTableAccess(new HttpClient(), configMock.Object);
        }

        private static AirTableRequest GetRequest(params Fields[] fromFields)
        {
            return new AirTableRequest
            {
                Records = fromFields.Select(field => new Record {Fields = field})
            };
        }
    }
}