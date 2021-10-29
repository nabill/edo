using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using HappyTravel.CurrencyConverter;
using HappyTravel.CurrencyConverter.Infrastructure;
using HappyTravel.Edo.Api.Models.Agents;
using HappyTravel.Edo.Api.Services.CurrencyConversion;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Moq;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.CurrencyConversion.CurrencyConverterServiceTests
{
    public class ConvertingPrices
    {
        [Fact]
        public async Task Conversion_should_returns_success_when_a_current_currency_is_null()
        {
            var getCurrencyFunc = new Func<object, Currencies?>(o => null);
            var service = new CurrencyConverterService(null, null);

            var result = await service.ConvertPricesInData(new object(), null, getCurrencyFunc);

            Assert.True(result.IsSuccess);
        }


        [Fact]
        public async Task Conversion_should_returns_success_when_a_current_currency_equals_a_target_currency()
        {
            var getCurrencyFunc = new Func<object, Currencies?>(o => CurrencyConverterService.TargetCurrency);
            var service = new CurrencyConverterService(null, null);

            var result = await service.ConvertPricesInData(new object(), null, getCurrencyFunc);

            Assert.True(result.IsSuccess);
        }


        [Fact]
        public async Task Conversion_should_returns_failure_when_a_current_currency_is_not_specified()
        {
            var getCurrencyFunc = new Func<object, Currencies?>(o => Currencies.NotSpecified);
            var service = new CurrencyConverterService(null, null);

            var result = await service.ConvertPricesInData(new object(), null, getCurrencyFunc);

            Assert.True(result.IsFailure);
        }


        [Fact]
        public async Task Conversion_should_returns_failure_when_a_rate_currency_service_returns_failure()
        {
            var rateServiceMock = new Mock<ICurrencyRateService>();
            rateServiceMock.Setup(s => s.Get(It.IsAny<Currencies>(), It.IsAny<Currencies>()))
                .Returns(new ValueTask<Result<decimal>>(Result.Failure<decimal>("error")));
            var service = new CurrencyConverterService(rateServiceMock.Object, null);

            var result = await service.ConvertPricesInData(new object(), null, GetCurrencyFunc);

            Assert.True(result.IsFailure);
        }


        [Fact(Skip = "Can't test, because of ICurrencyConverterFactory design")]
        public void Conversion_should_returns_success()
        { }
        
        
        [Fact]
        public async Task Conversion_should_use_rate_to_convert_prices()
        {
            var rateService = CreateAedToUsdRateService(aedToUsdRate: (decimal) 0.274);
            var converterFactory = CreateConverterFactoryWithZeroBuffers();
            var currencyConverter = new CurrencyConverterService(rateService, converterFactory);
            var originalPrice = new MoneyAmount
            {
                Amount = (decimal) 3761.62,
                Currency = Currencies.AED
            };

            var (_, _, resultingPrice, _) = await currencyConverter.ConvertPricesInData(originalPrice,
                (amount, function) => function(amount), 
                d => d.Currency);


            Assert.Equal((decimal) 1030.68, resultingPrice.Amount);
            Assert.Equal(Currencies.USD, resultingPrice.Currency);
            
            
            static ICurrencyRateService CreateAedToUsdRateService(decimal aedToUsdRate)
            {
                var rt = new Mock<ICurrencyRateService>();
                rt.Setup(cr => cr.Get(Currencies.AED, Currencies.USD))
                    .ReturnsAsync(() => Result.Success(aedToUsdRate));
                
                return rt.Object;
            }

            
            static ICurrencyConverterFactory CreateConverterFactoryWithZeroBuffers()
                => new CurrencyConverterFactory(new List<BufferPair>
                {
                    new()
                    {
                        BufferValue = decimal.Zero,
                        SourceCurrency = Currencies.AED,
                        TargetCurrency = Currencies.USD
                    },
                    new()
                    {
                        BufferValue = decimal.Zero,
                        SourceCurrency = Currencies.USD,
                        TargetCurrency = Currencies.AED
                    }
                });
        }


        private static Func<object, Currencies?> GetCurrencyFunc => _ => Currencies.EUR;
    }
}
