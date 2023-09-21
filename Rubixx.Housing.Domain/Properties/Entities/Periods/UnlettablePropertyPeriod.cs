using Rubixx.Housing.Domain.Occupancies.Entities;
using Rubixx.Housing.Domain.Properties.Exceptions;

namespace Rubixx.Housing.Domain.Properties.Entities.Periods;

public class UnlettablePropertyPeriod : BasePropertyPeriod
{
    protected UnlettablePropertyPeriod() : base() { }

    public UnlettablePropertyPeriod(Property property, DateTime startDate, DateTime? endDate = null) : base(property, startDate, endDate) { }

    public override bool CanReviseStartDate => false;

    public override bool CanReviseEndDate => false;

    public override Occupancy StartOccupancy(DateTime occupancyStartDate, string uORN) => throw new PropertyPeriodViolation(this, "This property is unlettable");
}
