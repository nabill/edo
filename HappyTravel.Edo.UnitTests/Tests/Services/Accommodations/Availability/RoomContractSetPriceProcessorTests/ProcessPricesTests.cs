using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.EdoContracts.Accommodations.Internals;
using HappyTravel.Edo.Api.Services.PriceProcessing;
using HappyTravel.EdoContracts.General;
using HappyTravel.Money.Models;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Availability.RoomContractSetPriceProcessorTests
{
    public class ProcessPricesTests
    {
        public ProcessPricesTests()
        {
            _priceProcessFunction = price => new ValueTask<MoneyAmount>(new MoneyAmount(price.Amount * 1.07m, price.Currency));
            
            // all the rates must be different to avoid false positives
            var roomDailyRates = GetDailyRates(80, 120); 
            var roomTotalRate = new Rate(new MoneyAmount(180, default), new MoneyAmount(220, default));
            var contractSetTotalRate = new Rate(new MoneyAmount(250, default), new MoneyAmount(300, default));

            _roomContractSet = GetRoomContractSet(
                contractSetTotalRate, 
                new List<RoomContract>
                {
                    GetRoomContract(roomDailyRates, roomTotalRate)
                });
            
            _roomContractSets = new List<RoomContractSet>
            {
                _roomContractSet
            };
        }
        

        [Fact]
        public async Task Check_process_prices_for_room_contract_set()
        {
            var processed = await RoomContractSetPriceProcessor.ProcessPrices(_roomContractSet, _priceProcessFunction);
            
            Assert.True(await PriceWasProcessed(_roomContractSet.Rate, processed.Rate));
            Assert.True(await PriceWasProcessed(_roomContractSet.RoomContracts[0].Rate, processed.RoomContracts[0].Rate));
            Assert.True(await PriceWasProcessed(_roomContractSet.RoomContracts[0].DailyRoomRates[0], processed.RoomContracts[0].DailyRoomRates[0]));
        }

        
        [Fact]
        public async Task Check_process_prices_for_list_of_room_contract_set()
        {
            var processed = await RoomContractSetPriceProcessor.ProcessPrices(_roomContractSets, _priceProcessFunction);
            
            Assert.True(await PriceWasProcessed(_roomContractSets[0].Rate, processed[0].Rate));
            Assert.True(await PriceWasProcessed(_roomContractSets[0].RoomContracts[0].Rate, processed[0].RoomContracts[0].Rate));
            Assert.True(await PriceWasProcessed(_roomContractSets[0].RoomContracts[0].DailyRoomRates[0], processed[0].RoomContracts[0].DailyRoomRates[0]));
        }


        private async Task<bool> PriceWasProcessed(Rate original, Rate processed)
        {
            return await PriceWasProcessed(original.Gross, processed.Gross) 
                && await PriceWasProcessed(original.FinalPrice, processed.FinalPrice);
        }


        private async Task<bool> PriceWasProcessed(DailyRate original, DailyRate processed)
        {
            return await PriceWasProcessed(original.Gross, processed.Gross) 
                && await PriceWasProcessed(original.FinalPrice, processed.FinalPrice);
        }


        private async Task<bool> PriceWasProcessed(MoneyAmount original, MoneyAmount processed)
        {
            var expected = await _priceProcessFunction(original);
            return expected.Amount == processed.Amount 
                && expected.Currency == processed.Currency;
        }
        

        private RoomContractSet GetRoomContractSet(Rate contractSetTotalRate, List<RoomContract> roomContracts)
            => new(default, contractSetTotalRate, default, roomContracts, default, default);
        

        private RoomContract GetRoomContract(List<DailyRate> roomDailyRates, Rate roomTotalRate)
            => new(default, default, default, default, 
                default, default, default, roomDailyRates, roomTotalRate, default);
        

        private List<DailyRate> GetDailyRates(decimal final, decimal gross)
        {
            var fromDate = new DateTime();
            return new List<DailyRate>
            {
                new(fromDate,
                    fromDate.AddDays(1), // because of validation "toDate" should be bigger by exactly 1 day
                    new MoneyAmount(final, default),
                    new MoneyAmount(gross, default)
                )
            };
        }
       
        private readonly PriceProcessFunction _priceProcessFunction;
        private readonly RoomContractSet _roomContractSet;
        private readonly List<RoomContractSet> _roomContractSets;
    }
}