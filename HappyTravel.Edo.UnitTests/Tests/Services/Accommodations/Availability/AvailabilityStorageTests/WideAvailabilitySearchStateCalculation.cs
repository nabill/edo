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
            var supplierStates = new List<(int, SupplierAvailabilitySearchState)>
            {
                (1, SupplierAvailabilitySearchState.Failed(searchId, errors[0])),
                (2, SupplierAvailabilitySearchState.Failed(searchId, errors[1])),
                (3, SupplierAvailabilitySearchState.Failed(searchId, errors[2]))
            };

            var wideAvailabilitySearchState = WideAvailabilitySearchState.FromSupplierStates(searchId, supplierStates);
        
            foreach (var error in errors)
                Assert.Contains(error, wideAvailabilitySearchState.Error);
        }
        
        
        [Fact]
        public void Provider_search_results_count_should_sum()
        {
            var searchId = new Guid("c273b8eb-5351-424a-a10b-910ed755f6d5");
            var supplierStates = new List<(int, SupplierAvailabilitySearchState)>
            {
                (1, SupplierAvailabilitySearchState.Completed(searchId, new List<string>(),  10)),
                (2, SupplierAvailabilitySearchState.Completed(searchId, new List<string>(),15)),
                (3, SupplierAvailabilitySearchState.Completed(searchId, new List<string>(),144))
            };
        
            var wideAvailabilitySearchState = WideAvailabilitySearchState.FromSupplierStates(searchId, supplierStates);
            
            Assert.Equal(169, wideAvailabilitySearchState.ResultCount);
        }

        [Fact]
        public void Provider_search_results_with_duplicates_should_sum_unique_results_count()
        {
            var searchId = new Guid("c273b8eb-5351-424a-a10b-910ed755f6d5");
            var supplierStates = new List<(int, SupplierAvailabilitySearchState)>
            {
                (1, SupplierAvailabilitySearchState.Completed(searchId, new List<string?>{"1", "2", "6", null},  10)),
                (2, SupplierAvailabilitySearchState.Completed(searchId, new List<string?>{"1", "3", null},15)),
                (3, SupplierAvailabilitySearchState.Completed(searchId, new List<string?>{"1", "4", "6"},144))
            };

            var wideAvailabilitySearchState = WideAvailabilitySearchState.FromSupplierStates(searchId, supplierStates);

            Assert.Equal(166, wideAvailabilitySearchState.ResultCount);
        }
        
        
        [Theory]
        [InlineData(1, AvailabilitySearchTaskState.Completed)]
        [InlineData(2, AvailabilitySearchTaskState.Failed)]
        [InlineData(3, AvailabilitySearchTaskState.Pending)]
        public void One_supplier_search_should_return_supplier_state(int supplierId, AvailabilitySearchTaskState supplierTaskState)
        {
            var searchId = new Guid("45a364fb-33be-4115-97fe-c94090d86452");
            var supplierSearchState = CreateSupplierAvailabilitySearchState(searchId, supplierTaskState);

            var wideSearchState = WideAvailabilitySearchState.FromSupplierStates(searchId, new[] {(supplierId, supplierSearchState: supplierSearchState)});
            
            Assert.Equal(supplierTaskState, wideSearchState.TaskState);
        }

        
        [Theory]
        [InlineData(AvailabilitySearchTaskState.Completed)]
        [InlineData(AvailabilitySearchTaskState.Failed)]
        [InlineData(AvailabilitySearchTaskState.Pending)]
        public void Should_return_same_state_when_supplier_states_equal(AvailabilitySearchTaskState searchTaskState)
        {
            var searchId = new Guid("1929875f-275f-46ec-84b7-d32f6a4f30d8");
            var supplierSearchStates = new List<(int, SupplierAvailabilitySearchState)>
            {
                (1, CreateSupplierAvailabilitySearchState(searchId, searchTaskState)),
                (2, CreateSupplierAvailabilitySearchState(searchId, searchTaskState)),
                (3, CreateSupplierAvailabilitySearchState(searchId, searchTaskState))
            };
        
            var wideSearchState = WideAvailabilitySearchState.FromSupplierStates(searchId, supplierSearchStates);
            
            Assert.Equal(searchTaskState, wideSearchState.TaskState);
        }
        
        
        [Fact]
        public void Should_return_completed_when_all_searches_finished_or_failed()
        {
            var searchId = new Guid("91c56a8a-cba1-4832-8251-030ac51aee77");
            var supplierSearchStates = new List<(int, SupplierAvailabilitySearchState)>
            {
                (1, CreateSupplierAvailabilitySearchState(searchId, AvailabilitySearchTaskState.Completed)),
                (2, CreateSupplierAvailabilitySearchState(searchId, AvailabilitySearchTaskState.Failed)),
                (3, CreateSupplierAvailabilitySearchState(searchId, AvailabilitySearchTaskState.Failed))
            };
        
            var wideSearchState = WideAvailabilitySearchState.FromSupplierStates(searchId, supplierSearchStates);
            
            Assert.Equal(AvailabilitySearchTaskState.Completed, wideSearchState.TaskState);
        }
        
        
        [Fact]
        public void Should_return_partially_completed_when_one_connector_is_pending()
        {
            var searchId = new Guid("815379cb-419f-465b-b671-e081c73876a8");
            var supplierSearchStates = new List<(int, SupplierAvailabilitySearchState)>
            {
                (1, CreateSupplierAvailabilitySearchState(searchId, AvailabilitySearchTaskState.Completed)),
                (2, CreateSupplierAvailabilitySearchState(searchId, AvailabilitySearchTaskState.Pending)),
                (3, CreateSupplierAvailabilitySearchState(searchId, AvailabilitySearchTaskState.Failed))
            };
        
            var wideSearchState = WideAvailabilitySearchState.FromSupplierStates(searchId, supplierSearchStates);
            
            Assert.Equal(AvailabilitySearchTaskState.PartiallyCompleted, wideSearchState.TaskState);
        }
        
        
        private static SupplierAvailabilitySearchState CreateSupplierAvailabilitySearchState(Guid searchId, AvailabilitySearchTaskState supplierSearchState)
        {
            switch (supplierSearchState)
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