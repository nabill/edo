using HappyTravel.Edo.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HappyTravel.Edo.UnitTests.Utility
{
    public static class MockEdoContextFactory
    {
        public static Mock<EdoContext> Create()
        {
            return new Mock<EdoContext>(new DbContextOptions<EdoContext>());
        }
    }
}