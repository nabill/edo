using System.Linq;
using System.Text.Json;
using CSharpFunctionalExtensions;
using HappyTravel.Edo.Api.Models.Company;
using HappyTravel.Edo.Data;
using System.Threading.Tasks;
using HappyTravel.Edo.Data.StaticDatas;
using Microsoft.EntityFrameworkCore;

namespace HappyTravel.Edo.Api.Services.Company
{
    public class CompanyService : ICompanyService
    {
        public CompanyService(EdoContext context)
        {
            _context = context;
        }


        public async ValueTask<Result<CompanyInfo>> Get()
        {
            if (_companyInfo != default)
                return Result.Ok(_companyInfo);

            var companyInfo = await _context.StaticData.SingleOrDefaultAsync(sd => sd.Type == StaticDataTypes.CompanyInfo);
            if (companyInfo == default)
                return Result.Fail<CompanyInfo>("Could not find company information");

            _companyInfo = JsonSerializer.Deserialize<CompanyInfo>(companyInfo.Data.RootElement.ToString());
            return Result.Ok(_companyInfo);
        }


        private readonly EdoContext _context;
        private static CompanyInfo _companyInfo;
    }
}