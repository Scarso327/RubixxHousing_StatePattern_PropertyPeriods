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

    public DateTime? NotifiedDate { get; private set; }

    public Guid OccupancyId { get; private set; }
    public virtual Occupancy Occupancy { get; set; }

    public bool CanBeCancelled => !EndDate.HasValue;
    public bool CanBeReinstated => EndDate.HasValue && !PeriodAfterThisOne!.EndDate.HasValue && PeriodAfterThisOne is VoidPropertyPeriod;

    public override bool CanSupercedePeriods => true;
    public override bool CanBeSuperceded => CanBeCancelled;

    // This check doesn't account for occupied period's not allowing you to force revise them from others as that is added via guard clause on the revise methods in this class already
    // The only time the period after or before really matters is when the dates are going to overlap
    public override bool CanReviseStartDate => !EndDate.HasValue && (PeriodBeforeThisOne is VoidPropertyPeriod || PeriodBeforeThisOne is OccupiedPropertyPeriod);

    // See comment above for why we allowed OccupiedPropertyPeriod's through
    public override bool CanReviseEndDate => PeriodAfterThisOne is VoidPropertyPeriod || PeriodAfterThisOne is OccupiedPropertyPeriod;

    public override void ReviseStartDate(DateTime newStartDate)
    {
        // Read comment above "CanReviseStartDate" for context
        if (PeriodBeforeThisOne is OccupiedPropertyPeriod occupiedPropertyPeriod && occupiedPropertyPeriod.EndDate >= newStartDate)
            throw new PropertyPeriodViolation(this, "You must revise the end date of the occupancy before this one first");

        base.ReviseStartDate(newStartDate);
    }

    public override void ReviseEndDate(DateTime newEndDate)
    {
        // Read comment above "CanReviseStartDate" for context
        if (PeriodAfterThisOne is OccupiedPropertyPeriod occupiedPropertyPeriod && occupiedPropertyPeriod.StartDate <= newEndDate)
            throw new PropertyPeriodViolation(this, "You must revise the start date of the occupancy after this one first");

        base.ReviseEndDate(newEndDate);
    }

    public void ReviseEndDate(DateTime newEndDate, DateTime newNotifiedDate)
    {
        NotifiedDate = newNotifiedDate;
        ReviseEndDate(newEndDate);
    }

    public void SupercedeOverlappingPeriod()
    {
        if (SupercededByPropertyPeriod is null)
            return;

        SupercededByPropertyPeriod.SupersedePeriod(this);

        // Unset Superceded Periods
        SupercededByPropertyPeriod = null;
        SupercededByPropertyPeriodId = null;
    }

    public override void DisposeProperty(DateTime disposalDate) => throw new PropertyPeriodViolation(this, "This property has an active occupancy so can't be disposed");

    public override void EndOccupancy(DateTime occupancyEndDate)
    {
        EndDate = occupancyEndDate;

        var voidPropertyPeriod = new VoidPropertyPeriod(Property, occupancyEndDate.AddDays(1));
        Property.AddPropertyPeriod(voidPropertyPeriod);
    }

    public override Occupancy StartOccupancy(DateTime occupancyStartDate, string uORN) => throw new PropertyPeriodViolation(this, "This property has an occupancy on the date you've specified");
}
