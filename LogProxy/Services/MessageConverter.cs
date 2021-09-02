using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LogProxy.Messages;

namespace LogProxy.Services
{
    public interface IMessageConverter<in TFrom, out TTo>
    {
        public TTo Convert(TFrom message);
    }

    public class ToEnrichedTitlesAndTexts : IMessageConverter<IEnumerable<AirTableResponse>, IEnumerable<EnrichedTitleAndText>>
    {
        public IEnumerable<EnrichedTitleAndText> Convert(IEnumerable<AirTableResponse> airTableResponses)
        {
            if (airTableResponses == null)
                return new List<EnrichedTitleAndText>();

            var responseList = airTableResponses.ToList();
            if (!responseList.Any())
                return new List<EnrichedTitleAndText>();

            return responseList.Where(response => response.Records != null && response.Records.Any())
                .SelectMany(response => response.Records).Select(record => new EnrichedTitleAndText
            {
                Id = record.Fields.Id,
                Title = record.Fields.Summary,
                Text = record.Fields.Message,
                ReceivedAt = record.Fields.ReceivedAt
            });
        }
    }

    public class ToAirTableRequest : IMessageConverter<IEnumerable<TitleAndText>, AirTableRequest>
    {
        public AirTableRequest Convert(IEnumerable<TitleAndText> message)
        {
            if (message == null)
                return null;
            var titlesAndTexts = message.ToList();
            if (!titlesAndTexts.Any())
                return null;
            return new AirTableRequest
            {
                Records = titlesAndTexts.Select(titleAndText => new Record
                {
                    Fields = new Fields
                    {
                        Id = Guid.NewGuid().ToString(),
                        Summary = titleAndText.Title,
                        Message = titleAndText.Text,
                        ReceivedAt = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
                    }
                })
            };
        }
    }
}