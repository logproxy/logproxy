using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogProxy.Messages;
using LogProxy.Services;
using Moq;
using Xunit;

namespace LogProxy.UnitTests
{
    public class LogProxyServiceTests
    {
        [Fact]
        public async Task GetFromThirdPartyAsync_Verify_CallsAirTable()
        {
            var airTableAccessMock = new Mock<IAirTableAccess>();
            var logProxy = GetLogProxy(airTableAccessMock.Object);

            await logProxy.GetFromThirdPartyAsync();

            airTableAccessMock.Verify(x => x.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetFromThirdPartyAsync_AirTableThrows_Throw()
        {
            var airTableAccessMock = new Mock<IAirTableAccess>();
            var exception = new Exception();
            airTableAccessMock.Setup(x => x.GetAllAsync()).Throws(exception);
            var logProxy = GetLogProxy(airTableAccessMock.Object);

            var actual = await Assert.ThrowsAnyAsync<Exception>(async () => await logProxy.GetFromThirdPartyAsync());
            
            Assert.Equal(exception, actual);
        }

        [Fact]
        public async Task GetFromThirdPartyAsync_Verify_DontPostAnything()
        {
            var airTableAccessMock = new Mock<IAirTableAccess>();
            var logProxy = GetLogProxy(airTableAccessMock.Object);

            await logProxy.GetFromThirdPartyAsync();

            airTableAccessMock.Verify(x => x.PostAsync(It.IsAny<AirTableRequest>()), Times.Never);
        }

        [Fact]
        public async Task GetFromThirdPartyAsync_Verify_AirTableResultIsConverted()
        {
            var airTableAccessMock = new Mock<IAirTableAccess>();
            var allTheAirTableResponses = new List<AirTableResponse> {new AirTableResponse()};
            airTableAccessMock.Setup(x => x.GetAllAsync()).ReturnsAsync(allTheAirTableResponses);
            var toEnrichedTextsAndTitlesMock = new
                Mock<IMessageConverter<IEnumerable<AirTableResponse>, IEnumerable<EnrichedTitleAndText>>>();
            var logProxy = GetLogProxy(airTableAccessMock.Object, toEnrichedTextsAndTitlesMock.Object);

            await logProxy.GetFromThirdPartyAsync();

            toEnrichedTextsAndTitlesMock.Verify(x => x.Convert(allTheAirTableResponses), Times.Once);
        }

        [Fact]
        public async Task GetFromThirdPartyAsync_Verify_ConvertedIsReturned()
        {
            var airTableAccessMock = new Mock<IAirTableAccess>();
            var allTheAirTableResponses = new List<AirTableResponse> {new AirTableResponse()};
            airTableAccessMock.Setup(x => x.GetAllAsync()).ReturnsAsync(allTheAirTableResponses);
            var toEnrichedTextsAndTitlesMock = new
                Mock<IMessageConverter<IEnumerable<AirTableResponse>, IEnumerable<EnrichedTitleAndText>>>();
            var converted = new List<EnrichedTitleAndText> {new EnrichedTitleAndText()};
            toEnrichedTextsAndTitlesMock.Setup(x => x.Convert(allTheAirTableResponses)).Returns(converted);
            var logProxy = GetLogProxy(airTableAccessMock.Object, toEnrichedTextsAndTitlesMock.Object);

            var result = await logProxy.GetFromThirdPartyAsync();

            Assert.Equal(converted, result);
        }
        
        [Fact]
        public async Task TransferToThirdPartyAsync_Verify_ConverterIsCalled()
        {
            var converterMock = new Mock<IMessageConverter<IEnumerable<TitleAndText>, AirTableRequest>>();
            var logProxy = GetLogProxy(toAirTableRequest: converterMock.Object);
            var titlesAndTexts = new List<TitleAndText> {new TitleAndText()};

            await logProxy.TransferToThirdPartyAsync(titlesAndTexts);

            converterMock.Verify(x => x.Convert(titlesAndTexts), Times.Once);
        }
        
        [Fact]
        public async Task TransferToThirdPartyAsync_ConverterReturnsNull_DontPostOrGet()
        {
            var airTableAccessMock = new Mock<IAirTableAccess>();
            var logProxy = GetLogProxy(airTableAccessMock.Object);
            var titlesAndTexts = new List<TitleAndText> {new TitleAndText()};

            await logProxy.TransferToThirdPartyAsync(titlesAndTexts);

            airTableAccessMock.Verify(x=> x.PostAsync(It.IsAny<AirTableRequest>()), Times.Never);
            airTableAccessMock.Verify(x=> x.GetAllAsync(), Times.Never);
        }
        
        [Fact]
        public async Task TransferToThirdPartyAsync_Verify_ConvertedIsPostedAndDontDoAnyGet()
        {
            var airTableAccessMock = new Mock<IAirTableAccess>();
            var titlesAndTexts = new List<TitleAndText> {new TitleAndText()};
            var converterMock = new Mock<IMessageConverter<IEnumerable<TitleAndText>, AirTableRequest>>();
            var converted = new AirTableRequest();
            converterMock.Setup(x => x.Convert(titlesAndTexts)).Returns(converted);
            var logProxy = GetLogProxy(airTableAccessMock.Object, toAirTableRequest: converterMock.Object);

            await logProxy.TransferToThirdPartyAsync(titlesAndTexts);

            airTableAccessMock.Verify(x=> x.PostAsync(converted),Times.Once);
            airTableAccessMock.Verify(x=> x.GetAllAsync(), Times.Never);
        }
        
        [Fact]
        public async Task TransferToThirdPartyAsync_AirTableThrows_Throw()
        {
            var airTableAccessMock = new Mock<IAirTableAccess>();
            var titlesAndTexts = new List<TitleAndText> {new TitleAndText()};
            var converterMock = new Mock<IMessageConverter<IEnumerable<TitleAndText>, AirTableRequest>>();
            var converted = new AirTableRequest();
            var exception = new AirTablePostException("");
            airTableAccessMock.Setup(x => x.PostAsync(converted)).ThrowsAsync(exception);
            converterMock.Setup(x => x.Convert(titlesAndTexts)).Returns(converted);
            var logProxy = GetLogProxy(airTableAccessMock.Object, toAirTableRequest: converterMock.Object);

            var actual = await Assert.ThrowsAsync<AirTablePostException>(async () => await logProxy.TransferToThirdPartyAsync(titlesAndTexts));
            
            Assert.Equal(exception, actual);
        }


        private static ILogProxyService GetLogProxy(IAirTableAccess airTableAccess = null,
            IMessageConverter<IEnumerable<AirTableResponse>, IEnumerable<EnrichedTitleAndText>> toEnrichedTitlesAndTexts = null,
            IMessageConverter<IEnumerable<TitleAndText>, AirTableRequest> toAirTableRequest = null)
        {
            airTableAccess ??= new Mock<IAirTableAccess>().Object;
            toEnrichedTitlesAndTexts ??=
                new Mock<IMessageConverter<IEnumerable<AirTableResponse>, IEnumerable<EnrichedTitleAndText>>>().Object;
            toAirTableRequest ??= new Mock<IMessageConverter<IEnumerable<TitleAndText>, AirTableRequest>>().Object;
            return new LogProxyService(airTableAccess, toEnrichedTitlesAndTexts, toAirTableRequest);
        }
    }
}