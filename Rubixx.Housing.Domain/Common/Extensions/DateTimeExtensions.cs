using Rubixx.Housing.Domain.Common.Exceptions;

namespace Rubixx.Housing.Domain.Common.Extensions;

public static class DateTimeExtensions
{
    public static void ThrowIfInPast(this DateTime dateTime)
    {
        if (dateTime.Date < DateTime.Today)
            throw new DateInPast(dateTime);
    }
}
