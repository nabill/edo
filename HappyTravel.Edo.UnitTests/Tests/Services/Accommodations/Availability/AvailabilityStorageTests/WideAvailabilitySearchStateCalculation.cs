using System;
using System.Collections.Generic;
using HappyTravel.Edo.Api.Models.Availabilities;
using HappyTravel.Edo.Api.Services.Accommodations.Availability;
using HappyTravel.Edo.Api.Services.Accommodations.Availability.Steps.WideAvailabilitySearch;
using HappyTravel.Edo.Common.Enums;
using Xunit;

namespace HappyTravel.Edo.UnitTests.Tests.Services.Accommodations.Availability.AvailabilityStorageTests
{
    public class WideAvailabilitySearchStateCalculation
    {
        [Fact]
        public void Search_state_should_contain_all_errors()
        {
            var searchId = new Guid("ae05b78f-4488-4845-9f7d-bad3d4cd177e");
            var errors = new[] {"Failed to connect", "Failed to fetch", "Server error"};
            var providerStates = new List<(Suppliers, SupplierAvailabilitySearchState)>
            {
                (Suppliers.Etg, SupplierAvailabilitySearchState.Failed(searchId, errors[0])),
                (Suppliers.Netstorming, SupplierAvailabilitySearchState.Failed(searchId, errors[1])),
                (Suppliers.Illusions, SupplierAvailabilitySearchState.Failed(searchId, errors[2]))
            };

            var wideAvailabilitySearchState = WideAvailabilitySearchState.FromProviderStates(searchId, providerStates);
        
            foreach (var error in errors)
                Assert.Contains(error, wideAvailabilitySearchState.Error);
        }
        
        
        [Fact]
        public void Provider_search_results_count_should_sum()
        {
            var searchId = new Guid("c273b8eb-5351-424a-a10b-910ed755f6d5");
            var providerStates = new List<(Suppliers, SupplierAvailabilitySearchState)>
            {
                (Suppliers.Etg, SupplierAvailabilitySearchState.Completed(searchId, new List<string>(),  10)),
                (Suppliers.Netstorming, SupplierAvailabilitySearchState.Completed(searchId, new List<string>(),15)),
                (Suppliers.Illusions, SupplierAvailabilitySearchState.Completed(searchId, new List<string>(),144))
            };
        
            var wideAvailabilitySearchState = WideAvailabilitySearchState.FromProviderStates(searchId, providerStates);
            
            Assert.Equal(169, wideAvailabilitySearchState.ResultCount);
        }

        [Fact]
        public void Provider_search_results_with_duplicates_count_should_sum()
        {
            var searchId = new Guid("c273b8eb-5351-424a-a10b-910ed755f6d5");
            var providerStates = new List<(Suppliers, SupplierAvailabilitySearchState)>
            {
                (Suppliers.Etg, SupplierAvailabilitySearchState.Completed(searchId, new List<string>{"1", "2", "6", null},  10)),
                (Suppliers.Netstorming, SupplierAvailabilitySearchState.Completed(searchId, new List<string>{"1", "3", null},15)),
                (Suppliers.Illusions, SupplierAvailabilitySearchState.Completed(searchId, new List<string>{"1", "4", "6"},144))
            };

            var wideAvailabilitySearchState = WideAvailabilitySearchState.FromProviderStates(searchId, providerStates);

            Assert.Equal(166, wideAvailabilitySearchState.ResultCount);
        }
        
        
        [Theory]
        [InlineData(Suppliers.Illusions, AvailabilitySearchTaskState.Completed)]
        [InlineData(Suppliers.Netstorming, AvailabilitySearchTaskState.Failed)]
        [InlineData(Suppliers.Etg, AvailabilitySearchTaskState.Pending)]
        public void One_provider_search_should_return_provider_state(Suppliers supplier, AvailabilitySearchTaskState providerTaskState)
        {
            var searchId = new Guid("45a364fb-33be-4115-97fe-c94090d86452");
            var providerSearchState = CreateProviderAvailabilitySearchState(searchId, providerTaskState);

            var wideSearchState = WideAvailabilitySearchState.FromProviderStates(searchId, new[] {(supplier, providerSearchState)});
            
            Assert.Equal(providerTaskState, wideSearchState.TaskState);
        }

        
        [Theory]
        [InlineData(AvailabilitySearchTaskState.Completed)]
        [InlineData(AvailabilitySearchTaskState.Failed)]
        [InlineData(AvailabilitySearchTaskState.Pending)]
        public void Should_return_same_state_when_provider_states_equal(AvailabilitySearchTaskState searchTaskState)
        {
            var searchId = new Guid("1929875f-275f-46ec-84b7-d32f6a4f30d8");
            var providerSearchStates = new List<(Suppliers, SupplierAvailabilitySearchState)>
            {
                (Suppliers.Etg, CreateProviderAvailabilitySearchState(searchId, searchTaskState)),
                (Suppliers.Illusions, CreateProviderAvailabilitySearchState(searchId, searchTaskState)),
                (Suppliers.Netstorming, CreateProviderAvailabilitySearchState(searchId, searchTaskState))
            };
        
            var wideSearchState = WideAvailabilitySearchState.FromProviderStates(searchId, providerSearchStates);
            
            Assert.Equal(searchTaskState, wideSearchState.TaskState);
        }
        
        
        [Fact]
        public void Should_return_completed_when_all_searches_finished_or_failed()
        {
            var searchId = new Guid("91c56a8a-cba1-4832-8251-030ac51aee77");
            var providerSearchStates = new List<(Suppliers, SupplierAvailabilitySearchState)>
            {
                (Suppliers.Etg, CreateProviderAvailabilitySearchState(searchId, AvailabilitySearchTaskState.Completed)),
                (Suppliers.Illusions, CreateProviderAvailabilitySearchState(searchId, AvailabilitySearchTaskState.Failed)),
                (Suppliers.Netstorming, CreateProviderAvailabilitySearchState(searchId, AvailabilitySearchTaskState.Failed))
            };
        
            var wideSearchState = WideAvailabilitySearchState.FromProviderStates(searchId, providerSearchStates);
            
            Assert.Equal(AvailabilitySearchTaskState.Completed, wideSearchState.TaskState);
        }
        
        
        [Fact]
        public void Should_return_partially_completed_when_one_connector_is_pending()
        {
            var searchId = new Guid("815379cb-419f-465b-b671-e081c73876a8");
            var providerSearchStates = new List<(Suppliers, SupplierAvailabilitySearchState)>
            {
                (Suppliers.Etg, CreateProviderAvailabilitySearchState(searchId, AvailabilitySearchTaskState.Completed)),
                (Suppliers.Illusions, CreateProviderAvailabilitySearchState(searchId, AvailabilitySearchTaskState.Pending)),
                (Suppliers.Netstorming, CreateProviderAvailabilitySearchState(searchId, AvailabilitySearchTaskState.Failed))
            };
        
            var wideSearchState = WideAvailabilitySearchState.FromProviderStates(searchId, providerSearchStates);
            
            Assert.Equal(AvailabilitySearchTaskState.PartiallyCompleted, wideSearchState.TaskState);
        }
        
        
        private static SupplierAvailabilitySearchState CreateProviderAvailabilitySearchState(Guid searchId, AvailabilitySearchTaskState providerSearchState)
        {
            switch (providerSearchState)
            {
                case AvailabilitySearchTaskState.Completed:
                    return SupplierAvailabilitySearchState.Completed(searchId, new List<string>(),10, string.Empty);
                case AvailabilitySearchTaskState.Failed:
                    return SupplierAvailabilitySearchState.Failed(searchId, string.Empty);
                case AvailabilitySearchTaskState.Pending:
                    return SupplierAvailabilitySearchState.Pending(searchId);
                default: throw new ArgumentException("Incomplete test data");
            }
        }
    }
}