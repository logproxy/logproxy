using System;
using System.Collections.Generic;
using System.Linq;
using LogProxy.Messages;
using LogProxy.Services;
using Xunit;

namespace LogProxy.UnitTests
{
    public class ToAirTableRequestTests
    {
        [Fact]
        public void Convert_Null_ReturnNull()
        {
            var converter = new ToAirTableRequest();

            var result = converter.Convert(null);

            Assert.Null(result);
        }

        [Fact]
        public void Convert_EmptyList_ReturnsNull()
        {
            var converter = new ToAirTableRequest();

            var result = converter.Convert(new List<TitleAndText>());

            Assert.Null(result);
        }

        [Theory]
        [InlineData("title", "text")]
        [InlineData("some_other_title", "some_other_text")]
        public void Convert_WithOneElement_AssertAirTableMessageHasExpectedRecord(string title, string text)
        {
            var converter = new ToAirTableRequest();

            var result = converter.Convert(GetTitleAndTexts((title, text)));

            var record = result.Records.Single();
            Assert.True(!string.IsNullOrWhiteSpace(record.Fields.Id) && record.Fields.Summary == title &&
                        record.Fields.Message == text && IsQuiteUpToDateTimestamp(record.Fields.ReceivedAt));
        }

        [Fact]
        public void Convert_WithMoreElements_AssertAirTableMessageHasExpectedRecords()
        {
            var converter = new ToAirTableRequest();
            var titleAndTexts = GetTitleAndTexts(("title1", "text1"),
                ("title2", "text2"), ("title3", "text3"), ("title4", "text4")).ToList();

            var result = converter.Convert(titleAndTexts);

            Assert.True(result.Records.Select(record => record.Fields).All(field =>
                !string.IsNullOrWhiteSpace(field.Id) &&
                titleAndTexts.SingleOrDefault(titleAndText =>
                    titleAndText.Title == field.Summary && titleAndText.Text == field.Message) != null &&
                IsQuiteUpToDateTimestamp(field.ReceivedAt)));
        }
        
        [Fact]
        public void Convert_WithMoreElements_AssertGeneratedIdsAreDistinctGuids()
        {
            var converter = new ToAirTableRequest();
            var titleAndTexts = GetTitleAndTexts(("title1", "text1"),
                ("title2", "text2"), ("title3", "text3"), ("title4", "text4")).ToList();

            var result = converter.Convert(titleAndTexts);

            var generatedIdCount = result.Records.Select(record => record.Fields.Id).Distinct().Count();
            Assert.Equal(titleAndTexts.Count, generatedIdCount);
            Assert.All(result.Records, record => Guid.TryParse(record.Fields.Id, out var _));
        }
        
        private bool IsQuiteUpToDateTimestamp(string receivedAt)
        {
            var utcNow = DateTime.UtcNow;
            var receivedAtDateTime = DateTime.Parse(receivedAt);
            return utcNow.Subtract(receivedAtDateTime).TotalSeconds < 2;
        }

        private IEnumerable<TitleAndText> GetTitleAndTexts(params (string title, string text)[] valueTuples)
        {
            return valueTuples.Select(tuple => new TitleAndText {Text = tuple.text, Title = tuple.title});
        }
    }
}

