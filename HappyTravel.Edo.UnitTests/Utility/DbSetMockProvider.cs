using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace HappyTravel.Edo.UnitTests.Utility
{
    public class DbSetMockProvider
    {
        public static DbSet<T> GetDbSetMock<T>(IEnumerable<T> enumerable) where T : class
        {
            var list = enumerable is List<T> enumerableList ? enumerableList : enumerable.ToList();
            var mock = list.AsQueryable().BuildMockDbSet();

            mock.Setup(d => d.Add(It.IsAny<T>())).Callback<T>(list.Add);

            return mock.Object;
        }
    }
}