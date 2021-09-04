using System.Collections.Generic;
using System.Threading.Tasks;
using LogProxy.Messages;

namespace LogProxy.Services
{
    public interface ILogProxyService
    {
        Task<IEnumerable<EnrichedTitleAndText>> GetFromThirdPartyAsync();
        Task TransferToThirdPartyAsync(IEnumerable<TitleAndText> titleAndTextCollection);
    }

    public class LogProxyService : ILogProxyService
    {
        private readonly IAirTableAccess _airTableAccess;
        private readonly IMessageConverter<IEnumerable<AirTableResponse>, IEnumerable<EnrichedTitleAndText>> _toEnrichedTitlesAndTexts;
        private readonly IMessageConverter<IEnumerable<TitleAndText>, AirTableRequest> _toAirTableRequest;

        public LogProxyService(IAirTableAccess airTableAccess, 
            IMessageConverter<IEnumerable<AirTableResponse>, IEnumerable<EnrichedTitleAndText>> toEnrichedTitlesAndTexts, 
            IMessageConverter<IEnumerable<TitleAndText>, AirTableRequest> toAirTableRequest)
        {
            _airTableAccess = airTableAccess;
            _toEnrichedTitlesAndTexts = toEnrichedTitlesAndTexts;
            _toAirTableRequest = toAirTableRequest;
        }

        public async Task<IEnumerable<EnrichedTitleAndText>> GetFromThirdPartyAsync()
        {
            var airTableResponses = await _airTableAccess.GetAllAsync();
            var enrichedTitleAndText = _toEnrichedTitlesAndTexts.Convert(airTableResponses);
            return enrichedTitleAndText;
        }

        public async Task TransferToThirdPartyAsync(IEnumerable<TitleAndText> titleAndTextCollection)
        {
            var converted = _toAirTableRequest.Convert(titleAndTextCollection);
            if (converted == null)
                return;

            await _airTableAccess.PostAsync(converted);
        }
    }
}