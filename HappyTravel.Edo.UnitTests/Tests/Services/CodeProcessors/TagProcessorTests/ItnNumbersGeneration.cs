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
            
            var tagProcessor = new TagProcessor(dbContext, Options.Create(new TagProcessingOptions()));
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


        [InlineData("DEV", "DEV-HTL-ABC-000001")]
        [InlineData(null, "HTL-ABC-000001")]
        [InlineData("STG", "STG-HTL-ABC-000001")]
        [Theory]
        public async Task Generated_reference_codes_should_be_valid(string prefix, string result)
        {
            var context = MockEdoContextFactory.Create();
            context.Setup(e => e.GenerateNextItnMember(It.IsAny<string>()))
                .Returns(() => Task.FromResult(It.IsAny<int>()));
            context.Setup(e => e.RegisterItn(It.IsAny<string>()))
                .Returns(() => Task.CompletedTask);
            
            var tagProcessor = new TagProcessor(context.Object, Options.Create(new TagProcessingOptions
            {
                ReferenceCodePrefix = prefix
            }));
            
            var itn = await tagProcessor.GenerateItn();

            var sequentialReferenceCode = await tagProcessor.GenerateReferenceCode(ServiceTypes.HTL, "ABC", itn);
            var nonSequentialReferenceCode = await tagProcessor.GenerateNonSequentialReferenceCode(ServiceTypes.HTL, "ABC");
            
            Assert.Equal($"{result}-00", sequentialReferenceCode);
            Assert.Equal(result, nonSequentialReferenceCode);

            Assert.True(tagProcessor.TryGetItnFromReferenceCode(sequentialReferenceCode, out var r));
            Assert.Equal(itn, r);
            
            Assert.True(tagProcessor.TryGetItnFromReferenceCode(nonSequentialReferenceCode, out r));
            Assert.Equal(itn, r);
        }
        
        
        private const int MaxItnNumbersCount = 100_000;
        private long _currentItn = 0L;
        private readonly HashSet<string> _registeredItnNumbers = new HashSet<string>();
    }
}