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


        [InlineData("DEV", "DEV-HTL-ABC-000001-00")]
        [InlineData(null, "HTL-ABC-000001-00")]
        [InlineData("STG", "STG-HTL-ABC-000001-00")]
        [Theory]
        public async Task Generated_sequential_reference_codes_should_be_success(string prefix, string expectedReferenceCode)
        {
            var tagProcessor = MockTagProcessor(prefix);
            var itn = await tagProcessor.GenerateItn();
            var referenceCode = await tagProcessor.GenerateReferenceCode(ServiceTypes.HTL, "ABC", itn);
            
            Assert.Equal(expectedReferenceCode, referenceCode);
        }
        
        
        [InlineData("DEV", "DEV-HTL-ABC-000001")]
        [InlineData(null, "HTL-ABC-000001")]
        [InlineData("STG", "STG-HTL-ABC-000001")]
        [Theory]
        public async Task Generated_non_sequential_reference_codes_should_be_success(string prefix, string expectedReferenceCode)
        {
            var tagProcessor = MockTagProcessor(prefix);
            var referenceCode = await tagProcessor.GenerateNonSequentialReferenceCode(ServiceTypes.HTL, "ABC");
            
            Assert.Equal(expectedReferenceCode, referenceCode);
        }
        
        
        [InlineData("DEV", "000002", "DEV-HTL-ABC-000002")]
        [InlineData("DEV", "000073", "DEV-HTL-ABC-000073-00")]
        [InlineData(null, "243456", "HTL-ABC-243456")]
        [InlineData(null, "987654", "HTL-ABC-987654-00")]
        [InlineData("STG", "000001", "STG-HTL-ABC-000001")]
        [InlineData("STG", "000103", "STG-HTL-ABC-000103-00")]
        [Theory]
        public void Get_int_from_reference_code_should_be_success(string prefix, string itn, string referenceCode)
        {
            var tagProcessor = MockTagProcessor(prefix);

            Assert.True(tagProcessor.TryGetItnFromReferenceCode(referenceCode, out var result));
            Assert.Equal(itn, result);
        }


        private static TagProcessor MockTagProcessor(string prefix)
        {
            var context = MockEdoContextFactory.Create();
            context.Setup(e => e.GenerateNextItnMember(It.IsAny<string>()))
                .Returns(() => Task.FromResult(It.IsAny<int>()));
            context.Setup(e => e.RegisterItn(It.IsAny<string>()))
                .Returns(() => Task.CompletedTask);
            
            return new TagProcessor(context.Object, Options.Create(new TagProcessingOptions
            {
                ReferenceCodePrefix = prefix
            }));
        }
        
        
        private const int MaxItnNumbersCount = 100_000;
        private long _currentItn = 0L;
        private readonly HashSet<string> _registeredItnNumbers = new HashSet<string>();
    }
}