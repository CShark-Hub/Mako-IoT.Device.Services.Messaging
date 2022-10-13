using System.Collections;

namespace MakoIoT.Device.Services.Messaging.Test
{
    public static class Extensions
    {
        public static bool Contains(this IEnumerable enumerable, object item)
        {
            foreach (var i in enumerable)
            {
                if (i.Equals(item))
                    return true;
            }

            return false;
        }
    }
}
