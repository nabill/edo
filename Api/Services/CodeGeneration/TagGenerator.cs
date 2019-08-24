using System;
using System.Collections.Generic;
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


        public Task<string> GenerateReferenceCode(ServiceTypes serviceType, string destinationCode, string itineraryNumber)
        {
            //TODO: change temporary solution 
            var randomElement = new Random().Next(10000);

            return Task.FromResult(string.Join('-', serviceType, destinationCode, itineraryNumber, randomElement));
        }


        public async Task<string> GenerateItn()
        {
            var hash = string.Empty;
            var quotient = await _context.GetNextItineraryNumber();
            do
            {
                quotient = Math.DivRem(quotient, ItnNumeralSystemBase, out var remainder);
                hash += IthNumeralSystemTable[remainder];
            } while (quotient != 0);

            return hash.PadLeft(6, '0');
        }


        private const long ItnNumeralSystemBase = 36;

        private static readonly Dictionary<long, string> IthNumeralSystemTable = new Dictionary<long, string>
        {
            {0, "0"},
            {1, "1"},
            {2, "2"},
            {3, "3"},
            {4, "4"},
            {5, "5"},
            {6, "6"},
            {7, "7"},
            {8, "8"},
            {9, "9"},
            {10, "A"},
            {11, "B"},
            {12, "C"},
            {13, "D"},
            {14, "E"},
            {15, "F"},
            {16, "G"},
            {17, "H"},
            {18, "I"},
            {19, "J"},
            {20, "K"},
            {21, "L"},
            {22, "M"},
            {23, "N"},
            {24, "O"},
            {25, "P"},
            {26, "Q"},
            {27, "R"},
            {28, "S"},
            {29, "T"},
            {30, "U"},
            {31, "V"},
            {32, "W"},
            {33, "X"},
            {34, "Y"},
            {35, "Z"}
        };

        private readonly EdoContext _context;
    }
}