using System.Collections.Generic;
using System.Threading.Tasks;
using HappyTravel.Edo.Api.Services.CodeProcessors;
using HappyTravel.Edo.Data;
using HappyTravel.Edo.UnitTests.Infrastructure;
using Moq;
using NetTopologySuite.Utilities;
using Xunit;

namespace HappyTravel.Edo.UnitTests.TagGeneration
{
    public class ItnNumbersGeneration
    {
        [Fact]
        public async Task Generated_itn_numbers_should_be_unique()
        {
            var dbContext = CreateDbContextWithDuplicateItnValidation();
            
            var tagProcessor = new TagProcessor(dbContext);
            while (_currentItn < MaxItnNumbersCount)
            {
                await tagProcessor.GenerateItn();
            }


            EdoContext CreateDbContextWithDuplicateItnValidation()
            {
                var context = MockEdoContext.Create();
                
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
        
        private const int MaxItnNumbersCount = 100_000;
        private long _currentItn = 0L;
        private readonly HashSet<string> _registeredItnNumbers = new HashSet<string>();
    }
}