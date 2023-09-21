using Rubixx.Housing.Domain.Occupancies.Entities;
using Rubixx.Housing.Domain.Properties.Exceptions;

namespace Rubixx.Housing.Domain.Properties.Entities.Periods;

public class VoidPropertyPeriod : BasePropertyPeriod
{
    protected VoidPropertyPeriod() : base() { }

    public VoidPropertyPeriod(Property property, DateTime startDate, DateTime? endDate = null) : base(property, startDate, endDate) { }

    public override bool CanReviseStartDate => true;

    public override bool CanReviseEndDate => true;

    public Guid? OccupiedPropertyPeriodId { get; private set; }
    public virtual OccupiedPropertyPeriod? OccupiedPropertyPeriod { get; set; }

    public void SupersedeVoid(OccupiedPropertyPeriod occupiedPropertyPeriod)
    {
        if (OccupiedPropertyPeriodId.HasValue)
            throw new PropertyPeriodViolation(this, "Unable to supersede a void after it's already been superseded");

        OccupiedPropertyPeriodId = occupiedPropertyPeriod.Id;
        OccupiedPropertyPeriod = occupiedPropertyPeriod;
    }

    public override DateTime StartDate {
        get => OccupiedPropertyPeriod?.StartDate ?? base.StartDate;
        protected set
        {
            if (OccupiedPropertyPeriodId.HasValue)
                throw new PropertyPeriodViolation(this, "Unable to set start date of superceded void directly");

            base.StartDate = value;
        }
    }

    public override DateTime? EndDate
    {
        get => OccupiedPropertyPeriod?.StartDate ?? base.EndDate;
        protected set
        {
            if (OccupiedPropertyPeriodId.HasValue)
                throw new PropertyPeriodViolation(this, "Unable to set end date of superceded void directly");

            base.EndDate = value;
        }
    }

    public override Occupancy StartOccupancy(DateTime occupancyStartDate, string uORN)
    {
        if (OccupiedPropertyPeriodId.HasValue)
            throw new PropertyPeriodViolation(this, "Superceded Voids can't be used to start occupancies");

        // Void Periods only end if a property is let or disposed which if that happens we can't let an occupancy before it.
        if (EndDate.HasValue)
            throw new PropertyPeriodViolation(this, "Can't start an occupancy over a void period that has ended as it'll have future occupied or disposed periods");

        EndDate = occupancyStartDate.AddDays(-1);

        var occupiedPropertyPeriod = new OccupiedPropertyPeriod(Property, occupancyStartDate, uORN);

        // Instead of deleting void peridos that have been completely overridden we make them into a "superceded void"
        // so the data can be accessed but it won't ever be used to handle future periods
        if (occupancyStartDate <= StartDate)
            SupersedeVoid(occupiedPropertyPeriod);

        Property.AddPropertyPeriod(occupiedPropertyPeriod);

        return occupiedPropertyPeriod.Occupancy;
    }
}
