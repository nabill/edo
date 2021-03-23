using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Infrastructure.Options;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.UnitTests.Utility;
using Microsoft.Extensions.Options;
using Moq;
using NetTopologySuite.Utilities;
using Xunit;
using Assert = Xunit.Assert;

namespace HappyTravel.Edo.UnitTests.Tests.Services.CodeProcessors.TagProcessorTests
{
    public class ItnNumbersGeneration
    {
        [Fact]
        public async Task Generated_itn_numbers_should_be_unique()
        {
            var dbContext = CreateDbContextWithDuplicateItnValidation();
            
            var tagProcessor = new TagProcessor(dbContext, Options.Create(new BookingOptions()));
            while (_currentItn < MaxItnNumbersCount)
            {
                await tagProcessor.GenerateItn();
            }


            EdoContext CreateDbContextWithDuplicateItnValidation()
            {
                var context = MockEdoContextFactory.Create();
                
                context.Setup(e => e.GetNextItineraryNumber())
                    .Returns(() => Task.FromResult(++_currentItn));

                context.Setup(e => e.RegisterItn(It.IsAny<string>()))
                    .Returns<string>(itn =>
                    {
                        if (_registeredItnNumbers.Contains(itn))
                            throw new AssertionFailedException($"ITN '{itn}' is duplicated after {_registeredItnNumbers.Count} generations");

                        _registeredItnNumbers.Add(itn);
                        return Task.CompletedTask;
                    });

                return context.Object;
            }
        }


        [InlineData("DEV", ServiceTypes.HTL, "ABC", "DEV-HTL-ABC-000001-00")]
        [InlineData(null, ServiceTypes.HTL, "BCA", "HTL-BCA-000001-00")]
        [InlineData("STG", ServiceTypes.HTL, "CBA", "STG-HTL-CBA-000001-00")]
        [Theory]
        public async Task Generated_reference_codes_should_equals(string prefix, ServiceTypes serviceType, string destinationCode, string result)
        {
            var context = MockEdoContextFactory.Create();
            context.Setup(e => e.GenerateNextItnMember(It.IsAny<string>()))
                .Returns(() => Task.FromResult(It.IsAny<int>()));
            context.Setup(e => e.RegisterItn(It.IsAny<string>()))
                .Returns(() => Task.CompletedTask);
            
            var tagProcessor = new TagProcessor(context.Object, Options.Create(new BookingOptions
            {
                ReferenceCodePrefix = prefix
            }));
            
            var itn = await tagProcessor.GenerateItn();

            var referenceCode = await tagProcessor.GenerateReferenceCode(serviceType, destinationCode, itn);
            Assert.Equal(referenceCode, result);
        }

        [InlineData("DEV-HTL-ABC-000001-00", "DEV")]
        [InlineData("HTL-BCA-000001-00", null)]
        [InlineData("STG-HTL-CBA-000001-00", "STG")]
        [Theory]
        public void Get_itn_from_reference_code_should_success(string referenceCode, string prefix)
        {
            var context = MockEdoContextFactory.Create();
            var tagProcessor = new TagProcessor(context.Object, Options.Create(new BookingOptions
            {
                ReferenceCodePrefix = prefix
            }));
            
            Assert.True(tagProcessor.TryGetItnFromReferenceCode(referenceCode, out var result));
            Assert.Equal("000001", result);
        }
        
        
        private const int MaxItnNumbersCount = 100_000;
        private long _currentItn = 0L;
        private readonly HashSet<string> _registeredItnNumbers = new HashSet<string>();
    }
}