namespace Rubixx.Housing.Domain.Common.Exceptions;

public class DateInPast : Exception
{
    public DateInPast(DateTime date) : base($"{date.ToShortDateString()} is in the past") { }
}
