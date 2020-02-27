using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;

namespace HappyTravel.Edo.UnitTests.Infrastructure.DbSetMocks
{
    public class DbSetMockProvider
    {
        public static DbSet<T> GetDbSetMock<T>(IEnumerable<T> enumerable) where T : class => enumerable.AsQueryable().BuildMockDbSet().Object;
    }
}