using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.EdoContracts.General;
using HappyTravel.Money.Enums;
using HappyTravel.Money.Models;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Availability.RoomContractSetPriceProcessorTests
{
    public class ProcessPricesTests
    {
        public ProcessPricesTests()
        {
            _priceProcessFunction = price => new ValueTask<MoneyAmount>(new MoneyAmount(price.Amount * 1.07m, price.Currency));
            
            _totalFinal = 250;
            _totalFinalExpected = GetExpected(_totalFinal);

            _totalGross = 300;
            _totalGrossExpected = GetExpected(_totalGross);

            _roomGross = 220;
            _roomGrossExpected = GetExpected(_roomGross);

            _roomFinal = 180;
            _roomFinalExpected = GetExpected(_roomFinal);

            _dailyGross = 120;
            _dailyGrossExpected = GetExpected(_dailyGross);

            _dailyFinal = 80;
            _dailyFinalExpected = GetExpected(_dailyFinal);
            
            _roomContractSet = GetRoomContractSet();
            _roomContractSets = new List<RoomContractSet>
            {
                _roomContractSet
            };
        }
        

        [Fact]
        public async Task Check_process_prices_for_room_contract_set()
        {
            var processed = await RoomContractSetPriceProcessor.ProcessPrices(_roomContractSet, _priceProcessFunction);

            Assert.Equal(_totalGrossExpected, processed.Rate.Gross.Amount);
            Assert.Equal(_totalFinalExpected, processed.Rate.FinalPrice.Amount);
            Assert.Equal(_roomGrossExpected, processed.RoomContracts[0].Rate.Gross.Amount);
            Assert.Equal(_roomFinalExpected, processed.RoomContracts[0].Rate.FinalPrice.Amount);
            Assert.Equal(_dailyGrossExpected, processed.RoomContracts[0].DailyRoomRates[0].Gross.Amount);
            Assert.Equal(_dailyFinalExpected, processed.RoomContracts[0].DailyRoomRates[0].FinalPrice.Amount);
        }


        [Fact]
        public async Task Check_process_prices_for_list_of_room_contract_set()
        {
            var processed = await RoomContractSetPriceProcessor.ProcessPrices(_roomContractSets, _priceProcessFunction);
            
            Assert.Equal(_totalGrossExpected, processed[0].Rate.Gross.Amount);
            Assert.Equal(_totalFinalExpected, processed[0].Rate.FinalPrice.Amount);
            Assert.Equal(_roomGrossExpected, processed[0].RoomContracts[0].Rate.Gross.Amount);
            Assert.Equal(_roomFinalExpected, processed[0].RoomContracts[0].Rate.FinalPrice.Amount);
            Assert.Equal(_dailyGrossExpected, processed[0].RoomContracts[0].DailyRoomRates[0].Gross.Amount);
            Assert.Equal(_dailyFinalExpected, processed[0].RoomContracts[0].DailyRoomRates[0].FinalPrice.Amount);
        }

        private decimal GetExpected(decimal amount)
        {
            // do not use await because the function is used inside the constructor
            var expected = _priceProcessFunction(new MoneyAmount(amount, Currencies.NotSpecified));
            return expected.Result.Amount; 
        }


        private RoomContractSet GetRoomContractSet()
        {
            return new(default,
                new Rate(new MoneyAmount(_totalFinal, default), new MoneyAmount(_totalGross, default)),
                default,
                new List<RoomContract> {GetRoomContract()},
                default,
                default);
        }


        private RoomContract GetRoomContract()
        {
            return new(default, default, default, default, default, default, default,
                GetDailyRates(),
                new Rate(new MoneyAmount(_roomFinal, default), new MoneyAmount(_roomGross, default)),
                default);
        }


        private List<DailyRate> GetDailyRates()
        {
            var fromDate = new DateTime();
            return new List<DailyRate>
            {
                new(
                    fromDate,
                    fromDate.AddDays(1), // because of validation "toDate" should be bigger by exactly 1 day
                    new MoneyAmount(_dailyFinal, default),
                    new MoneyAmount(_dailyGross, default)
                )
            };
        }


        private readonly PriceProcessFunction _priceProcessFunction;

        private readonly RoomContractSet _roomContractSet;
        private readonly List<RoomContractSet> _roomContractSets;

        private readonly decimal _totalGross;
        private readonly decimal _totalGrossExpected;
        
        private readonly decimal _totalFinal;
        private readonly decimal _totalFinalExpected; 
        
        private readonly decimal _roomGross;
        private readonly decimal _roomGrossExpected;
        
        private readonly decimal _roomFinal;
        private readonly decimal _roomFinalExpected;

        private readonly decimal _dailyGross;
        private readonly decimal _dailyGrossExpected;

        private readonly decimal _dailyFinal;
        private readonly decimal _dailyFinalExpected;
    }
}