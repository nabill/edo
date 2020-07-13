using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;

namespace HappyTravel.Edo.UnitTests.Utility
{
    public class DbSetMockProvider
    {
        public static DbSet<T> GetDbSetMock<T>(IEnumerable<T> enumerable) where T : class => enumerable.AsQueryable().BuildMockDbSet().Object;
    }
}