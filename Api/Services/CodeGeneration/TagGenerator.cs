using System;
using System.Threading.Tasks;
using HappyTravel.Edo.Common.Enums;
using HappyTravel.Edo.Data;

namespace HappyTravel.Edo.Api.Services.CodeGeneration
{
    internal class TagGenerator : ITagGenerator
    {
        public TagGenerator(EdoContext context)
        {
            _context = context;
        }

        public Task<string> GenerateReferenceCode(ServiceTypes serviceType, string destinationCode, long itineraryNumber)
        {
            //TODO: change temporary solution 
            var randomElement = new Random().Next(10000);

            return Task.FromResult(string.Join('-', serviceType, destinationCode, itineraryNumber, randomElement));
        }

        public Task<long> GenerateItn() => _context.GetNextItineraryNumber();

        private readonly EdoContext _context;
    }
}