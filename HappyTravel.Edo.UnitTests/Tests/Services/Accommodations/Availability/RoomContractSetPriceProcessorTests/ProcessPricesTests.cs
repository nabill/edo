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
        [Fact]
        public async Task Should_process_total_rate()
        {
            var roomContractSet = CreateRoomContractSet(Currencies.USD, contractSetTotalRate: (Gross: 100, Final: 50));

            var processed = await RoomContractSetPriceProcessor.ProcessPrices(roomContractSet, AddTenReturnUsd);
            
            Assert.Equal(120, processed.Rate.Gross.Amount);
            Assert.Equal(60, processed.Rate.FinalPrice.Amount);
            Assert.Equal(Currencies.USD, processed.Rate.FinalPrice.Currency);
            Assert.Equal(Currencies.USD, processed.Rate.Gross.Currency);
        }
        
        
        [Fact]
        public async Task Should_process_total_rate_for_list()
        {
            var roomContractSet = CreateRoomContractSet(Currencies.USD, contractSetTotalRate: (Gross: 100, Final: 50));
            var roomContractSets = new List<RoomContractSet> {roomContractSet};

            var processed = await RoomContractSetPriceProcessor.ProcessPrices(roomContractSets, AddTenReturnUsd);
            
            Assert.Equal(120, processed[0].Rate.Gross.Amount);
            Assert.Equal(60, processed[0].Rate.FinalPrice.Amount);
            Assert.Equal(Currencies.USD, processed[0].Rate.FinalPrice.Currency);
            Assert.Equal(Currencies.USD, processed[0].Rate.Gross.Currency);
        }
        
        
        [Fact]
        public async Task Should_process_room_rate()
        {
            var roomContractSet = CreateRoomContractSet(Currencies.USD, contractSetTotalRate: (Gross: 100, Final: 50), roomTotalRate: (Gross: 100, Final: 50));

            var processed = await RoomContractSetPriceProcessor.ProcessPrices(roomContractSet, AddTenReturnUsd);
            
            Assert.Equal(120, processed.RoomContracts[0].Rate.Gross.Amount);
            Assert.Equal(60, processed.RoomContracts[0].Rate.FinalPrice.Amount);
            Assert.Equal(Currencies.USD, processed.RoomContracts[0].Rate.Gross.Currency);
            Assert.Equal(Currencies.USD, processed.RoomContracts[0].Rate.FinalPrice.Currency);
        }
        
        
        [Fact]
        public async Task Should_process_room_rate_for_list()
        {
            var roomContractSet = CreateRoomContractSet(Currencies.USD, contractSetTotalRate: (Gross: 100, Final: 50), roomTotalRate: (Gross: 100, Final: 50));
            var roomContractSets = new List<RoomContractSet> {roomContractSet};

            var processed = await RoomContractSetPriceProcessor.ProcessPrices(roomContractSets, AddTenReturnUsd); 
            
            Assert.Equal(120, processed[0].RoomContracts[0].Rate.Gross.Amount);
            Assert.Equal(60, processed[0].RoomContracts[0].Rate.FinalPrice.Amount);
            Assert.Equal(Currencies.USD, processed[0].RoomContracts[0].Rate.Gross.Currency);
            Assert.Equal(Currencies.USD, processed[0].RoomContracts[0].Rate.FinalPrice.Currency);
        }


        [Fact]
        public async Task Should_process_daily_room_rates()
        {
            var roomContractSet = CreateRoomContractSet(Currencies.USD, contractSetTotalRate: (Gross: 100, Final: 50), roomDailyRate: (Gross: 100, Final: 50));

            var processed = await RoomContractSetPriceProcessor.ProcessPrices(roomContractSet, AddTenReturnUsd);
            
            Assert.Equal(120, processed.RoomContracts[0].DailyRoomRates[0].Gross.Amount);
            Assert.Equal(60, processed.RoomContracts[0].DailyRoomRates[0].FinalPrice.Amount);
            Assert.Equal(Currencies.USD, processed.RoomContracts[0].DailyRoomRates[0].Gross.Currency);
            Assert.Equal(Currencies.USD, processed.RoomContracts[0].DailyRoomRates[0].FinalPrice.Currency);
        }
        
        
        [Fact]
        public async Task Should_process_daily_room_rates_for_list()
        {
            var roomContractSet = CreateRoomContractSet(Currencies.USD, contractSetTotalRate: (Gross: 100, Final: 50), roomDailyRate: (Gross: 100, Final: 50));
            var roomContractSets = new List<RoomContractSet> {roomContractSet};

            var processed = await RoomContractSetPriceProcessor.ProcessPrices(roomContractSets, AddTenReturnUsd); 
            
            Assert.Equal(120, processed[0].RoomContracts[0].DailyRoomRates[0].Gross.Amount);
            Assert.Equal(60, processed[0].RoomContracts[0].DailyRoomRates[0].FinalPrice.Amount);
            Assert.Equal(Currencies.USD, processed[0].RoomContracts[0].DailyRoomRates[0].Gross.Currency);
            Assert.Equal(Currencies.USD, processed[0].RoomContracts[0].DailyRoomRates[0].FinalPrice.Currency);
        }
        
        [InlineData(100, 50, 1d)]
        [InlineData(200, 100, 1.5d)]
        [InlineData(300, 200, 2.5d)]
        [Theory]
        public async Task Should_be_change_rates_proportionally(decimal gross, decimal net, decimal ratio)
        {
            var roomContractSet = CreateRoomContractSet(Currencies.USD, contractSetTotalRate: (Gross: gross, Final: net), roomDailyRate: (Gross: gross, Final: net), roomTotalRate:  (Gross: gross, Final: net));
            var roomContractSets = new List<RoomContractSet> {roomContractSet};

            ValueTask<MoneyAmount> PriceProcessFunction(MoneyAmount price) => new ValueTask<MoneyAmount>(new MoneyAmount(price.Amount * ratio, Currencies.USD));

            var processed = await RoomContractSetPriceProcessor.ProcessPrices(roomContractSets, PriceProcessFunction); 
            
            Assert.Equal(gross * ratio, processed[0].RoomContracts[0].DailyRoomRates[0].Gross.Amount);
            Assert.Equal(net * ratio, processed[0].RoomContracts[0].DailyRoomRates[0].FinalPrice.Amount);
            Assert.Equal(gross * ratio, processed[0].RoomContracts[0].Rate.Gross.Amount);
            Assert.Equal(net * ratio, processed[0].RoomContracts[0].Rate.FinalPrice.Amount);
            Assert.Equal(Currencies.USD, processed[0].RoomContracts[0].DailyRoomRates[0].Gross.Currency);
            Assert.Equal(Currencies.USD, processed[0].RoomContracts[0].DailyRoomRates[0].FinalPrice.Currency);
        }
        
        
        private RoomContractSet CreateRoomContractSet(
            Currencies currency = Currencies.USD,
            (decimal Gross, decimal Final) contractSetTotalRate = default,
            (decimal Gross, decimal Final) roomDailyRate = default,
            (decimal Gross, decimal Final) roomTotalRate = default
        )
        {
            return CreateRoomContractSet(
                new Rate(new MoneyAmount(contractSetTotalRate.Final, currency), new MoneyAmount(contractSetTotalRate.Gross, currency)), 
                new List<RoomContract>
                {
                    CreateRoomContract(
                        CreateDailyRates(currency, roomDailyRate.Final, roomDailyRate.Gross), 
                        new Rate(new MoneyAmount(roomTotalRate.Final, currency), new MoneyAmount(roomTotalRate.Gross, currency))
                        )
                }); 
        }
        

        private RoomContractSet CreateRoomContractSet(Rate contractSetTotalRate, List<RoomContract> roomContracts)
            => new(default, contractSetTotalRate, default, roomContracts, default, default);
        

        private RoomContract CreateRoomContract(List<DailyRate> roomDailyRates, Rate roomTotalRate)
            => new(default, default, default, default, 
                default, default, default, roomDailyRates, roomTotalRate, default);
        

        private List<DailyRate> CreateDailyRates(Currencies currency, decimal final, decimal gross)
        {
            var fromDate = new DateTime();
            return new List<DailyRate>
            {
                new(fromDate,
                    fromDate.AddDays(1), // because of validation "toDate" should be bigger by exactly 1 day
                    new MoneyAmount(final, currency),
                    new MoneyAmount(gross, currency)
                )
            };
        }
       
        private readonly PriceProcessFunction AddTenReturnUsd = price => new ValueTask<MoneyAmount>(new MoneyAmount(price.Amount + 10, Currencies.USD)) ;
    }
}