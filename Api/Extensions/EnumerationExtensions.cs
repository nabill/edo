namespace Api.Extensions
{
    public static class EnumerationExtensions
    {
        public static bool Has<T>(this System.Enum? type, T? value)
        {
            try
            {
                if (value is null || type is null)
                    return false;

                return (((int)(object)type & (int)(object)value) == (int)(object)value);
            }
            catch
            {
                return false;
            }
        }
    }
}