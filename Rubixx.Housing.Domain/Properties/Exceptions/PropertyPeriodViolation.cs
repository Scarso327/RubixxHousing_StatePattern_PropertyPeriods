using Rubixx.Housing.Domain.Properties.Entities.Periods;

namespace Rubixx.Housing.Domain.Properties.Exceptions;

public class PropertyPeriodViolation : Exception
{
    private readonly BasePropertyPeriod _propertyPeriod;

    public PropertyPeriodViolation(BasePropertyPeriod propertyPeriod, string message) : base(message)
        => _propertyPeriod = propertyPeriod;
}
