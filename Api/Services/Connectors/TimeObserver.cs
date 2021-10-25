using System;
using System.Threading.Tasks;

namespace HappyTravel.Edo.Api.Services.Connectors
{
    public class TimeObserver
    {
        public static async Task<T> Execute<T>(Func<Task<T>> observedFunc, Func<Task> notifyFunc, TimeSpan notifyAfter)
        {
            if (observedFunc is null)
                throw new ArgumentNullException(nameof(observedFunc));

            if (notifyFunc is null)
                throw new ArgumentNullException(nameof(notifyFunc));

            if (notifyAfter < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(notifyAfter), "Delay has negative value");

            var startedTickCount = Environment.TickCount;
            var result = await observedFunc();

            if (Environment.TickCount - startedTickCount > notifyAfter.TotalMilliseconds)
                await notifyFunc();

            return result;
        }
    }
}