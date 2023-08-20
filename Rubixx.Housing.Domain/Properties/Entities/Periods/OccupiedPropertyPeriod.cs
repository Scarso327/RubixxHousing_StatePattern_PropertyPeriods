using Rubixx.Housing.Domain.Occupancies.Entities;
using Rubixx.Housing.Domain.Properties.Exceptions;

namespace Rubixx.Housing.Domain.Properties.Entities.Periods;

public class OccupiedPropertyPeriod : BasePropertyPeriod
{
    protected OccupiedPropertyPeriod() : base() { }

    public OccupiedPropertyPeriod(Property property, DateTime startDate, string uORN, DateTime? endDate = null) : base(property, startDate, endDate)
    {
        Occupancy = new Occupancy(this, uORN);
    }

    public DateTime? NofitiedDate { get; private set; }

    public Guid OccupancyId { get; private set; }
    public virtual Occupancy Occupancy { get; set; }

    public Guid? SupercededVoidPropertyPeriodId { get; private set; }
    public virtual VoidPropertyPeriod? SupercededVoidPropertyPeriod { get; set; }

    public void ReviseStartDate(DateTime newStartDate)
    {
        
    }

    public void ReviseEndDate(DateTime newEndDate, DateTime newNotifiedDate)
    {
        if (!EndDate.HasValue)
            throw new PropertyPeriodViolation(this, "You can't revise the end date of an unended occupancy");
    }

    public override void DisposeProperty() => throw new PropertyPeriodViolation(this, "This property has an active occupancy so can't be disposed");

    public override void EndOccupancy(DateTime occupancyEndDate)
    {
        EndDate = occupancyEndDate;

        var voidPropertyPeriod = new VoidPropertyPeriod(Property, occupancyEndDate.AddDays(1));
        Property.AddPropertyPeriod(voidPropertyPeriod);
    }

    public override void StartOccupancy(DateTime occupancyStartDate, string uORN) => throw new PropertyPeriodViolation(this, "This property has an occupancy on the date you've specified");
}
