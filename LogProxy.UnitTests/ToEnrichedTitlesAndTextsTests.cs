using System.Collections.Generic;
using System.Linq;
using LogProxy.Messages;
using LogProxy.Services;
using Xunit;

namespace LogProxy.UnitTests
{
    public class ToEnrichedTitlesAndTextsTests
    {
        [Fact]
        public void Convert_FromNull_ReturnsEmptyList()
        {
            var converter = new ToEnrichedTitlesAndTexts();

            var result = converter.Convert(null);

            Assert.Empty(result);
        }

        [Fact]
        public void Convert_FromEmptyResponses_ReturnsEmptyList()
        {
            var converter = new ToEnrichedTitlesAndTexts();

            var result = converter.Convert(new List<AirTableResponse>());

            Assert.Empty(result);
        }

        [Fact]
        public void Convert_FromResponseWithNullRecords_ReturnsEmptyList()
        {
            var converter = new ToEnrichedTitlesAndTexts();

            var result = converter.Convert(new List<AirTableResponse>
            {
                new AirTableResponse
                {
                    Records = null
                }
            });

            Assert.Empty(result);
        }

        [Fact]
        public void Convert_FromResponseWithEmptyRecords_ReturnsEmptyList()
        {
            var converter = new ToEnrichedTitlesAndTexts();

            var result = converter.Convert(new List<AirTableResponse>
            {
                new AirTableResponse
                {
                    Records = new List<EnrichedRecord>()
                }
            });

            Assert.Empty(result);
        }

        [Fact]
        public void Convert_FromSingleResponseWithRecords_ReturnsExpectedTitlesAndTexts()
        {
            var converter = new ToEnrichedTitlesAndTexts();
            var records = GetRecords(5, 0).ToList();
            var airTableResponse = GetAirTableResponses(records);

            var result = converter.Convert(airTableResponse);

            Assert.True(result.All(enrichedTitleAndText =>
                records.SingleOrDefault(record =>
                    record.Fields.Id == enrichedTitleAndText.Id &&
                    record.Fields.ReceivedAt == enrichedTitleAndText.ReceivedAt &&
                    record.Fields.Summary == enrichedTitleAndText.Title &&
                    record.Fields.Message == enrichedTitleAndText.Text) != null));
        }

        [Fact]
        public void Convert_FromResponsesWithRecords_ReturnsExpectedTitlesAndTexts()
        {
            var converter = new ToEnrichedTitlesAndTexts();
            var recordsForFirstResponse = GetRecords(5, 0).ToList();
            var recordsForSecondResponse = GetRecords(5, 5).ToList();
            var airTableResponses = GetAirTableResponses(recordsForFirstResponse, recordsForSecondResponse);

            var result = converter.Convert(airTableResponses);

            var allRecords = recordsForFirstResponse.Concat(recordsForSecondResponse).ToList();
            Assert.True(result.All(enrichedTitleAndText =>
                allRecords.SingleOrDefault(record =>
                    record.Fields.Id == enrichedTitleAndText.Id &&
                    record.Fields.ReceivedAt == enrichedTitleAndText.ReceivedAt &&
                    record.Fields.Summary == enrichedTitleAndText.Title &&
                    record.Fields.Message == enrichedTitleAndText.Text) != null));
        }

        private static IEnumerable<AirTableResponse> GetAirTableResponses(
            params IEnumerable<EnrichedRecord>[] enrichedRecordsArray)
        {
            return enrichedRecordsArray.Select(recordArray => new AirTableResponse
            {
                Records = recordArray
            });
        }

        private static IEnumerable<EnrichedRecord> GetRecords(int numberOfRecords, int idOffset)
        {
            var records = new List<EnrichedRecord>();
            for (var i = 0; i < numberOfRecords; i++)
            {
                records.Add(new EnrichedRecord
                {
                    Id = "i_dont_care",
                    Fields = new Fields
                    {
                        Id = $"id{idOffset}",
                        Summary = $"summary{idOffset}",
                        Message = $"message{idOffset}",
                        ReceivedAt = $"receivedAt{idOffset}",
                    },
                    CreatedTime = "i_dont_care"
                });
                idOffset++;
            }

            return records;
        }
    }
}