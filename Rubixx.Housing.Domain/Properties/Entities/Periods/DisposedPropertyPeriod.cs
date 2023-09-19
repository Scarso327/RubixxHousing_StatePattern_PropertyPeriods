using Rubixx.Housing.Domain.Occupancies.Entities;
using Rubixx.Housing.Domain.Properties.Exceptions;

namespace Rubixx.Housing.Domain.Properties.Entities.Periods;

public class DisposedPropertyPeriod : BasePropertyPeriod
{
    protected DisposedPropertyPeriod() : base() { }

    public DisposedPropertyPeriod(Property property, DateTime startDate, DateTime? endDate = null) : base(property, startDate, endDate) { }

    public override bool CanReviseStartDate => false;

    public override bool CanReviseEndDate => false;

    public override void DisposeProperty(DateTime disposalDate) => throw new PropertyPeriodViolation(this, "This property has already been disposed");

    public override void EndOccupancy(DateTime occupancyEndDate) => throw new PropertyPeriodViolation(this, "A disposed property can't have an occupancy to end");

    public override Occupancy StartOccupancy(DateTime occupancyStartDate, string uORN) => throw new PropertyPeriodViolation(this, "An occupancy cannot be started once a property has been disposed");
}
