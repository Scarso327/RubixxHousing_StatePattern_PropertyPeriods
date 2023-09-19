using Rubixx.Housing.Domain.Occupancies.Entities;
using Rubixx.Housing.Domain.Properties.Exceptions;

namespace Rubixx.Housing.Domain.Properties.Entities.Periods;

public class UnlettablePropertyPeriod : BasePropertyPeriod
{
    protected UnlettablePropertyPeriod() : base() { }

    public UnlettablePropertyPeriod(Property property, DateTime startDate, DateTime? endDate = null) : base(property, startDate, endDate) { }

    public override void EndOccupancy(DateTime occupancyEndDate) => throw new PropertyPeriodViolation(this, "An unlettable property can't have an occupancy to end");

    public override Occupancy StartOccupancy(DateTime occupancyStartDate, string uORN) => throw new PropertyPeriodViolation(this, "This property is unlettable");
}
