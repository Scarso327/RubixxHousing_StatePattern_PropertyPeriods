using Rubixx.Housing.Domain.Properties.Exceptions;

namespace Rubixx.Housing.Domain.Properties.Entities.Periods;

public class VoidPropertyPeriod : BasePropertyPeriod
{
    protected VoidPropertyPeriod() : base() { }

    public VoidPropertyPeriod(Property property, DateTime startDate, DateTime? endDate = null) : base(property, startDate, endDate) { }

    public Guid? OccupiedPropertyPeriodId { get; private set; }
    public virtual OccupiedPropertyPeriod? OccupiedPropertyPeriod { get; set; }

    public void SupersedeVoid(OccupiedPropertyPeriod occupiedPropertyPeriod)
    {
        if (OccupiedPropertyPeriodId.HasValue)
            throw new PropertyPeriodViolation(this, "Unable to supersede a void after it's already been superseded");

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

    public override void DisposeProperty()
    {
        throw new NotImplementedException();
    }

    public override void EndOccupancy(DateTime occupancyEndDate) => throw new PropertyPeriodViolation(this, "A void property doesn't have an occupancy to end");

    public override void StartOccupancy(DateTime occupancyStartDate, string uORN)
    {
        if (OccupiedPropertyPeriodId.HasValue)
            throw new PropertyPeriodViolation(this, "Superceded Voids can't be used to start occupancies");

        EndDate = occupancyStartDate.AddDays(-1);

        var occupiedPropertyPeriod = new OccupiedPropertyPeriod(Property, occupancyStartDate, uORN);

        if (occupancyStartDate <= StartDate)
            SupersedeVoid(occupiedPropertyPeriod);

        Property.AddPropertyPeriod(occupiedPropertyPeriod);
    }
}
