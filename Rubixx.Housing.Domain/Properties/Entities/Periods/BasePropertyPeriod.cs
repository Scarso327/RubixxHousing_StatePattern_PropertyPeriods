using Rubixx.Housing.Domain.Occupancies.Entities;
using Rubixx.Housing.Domain.Properties.Exceptions;
using RubixxExtensibility.SharedLibrary.Common.Interfaces;

namespace Rubixx.Housing.Domain.Properties.Entities.Periods;

public abstract class BasePropertyPeriod : IEntity
{
    protected BasePropertyPeriod() { }

    protected BasePropertyPeriod(Property property, DateTime startDate, DateTime? endDate = null)
    {
        Property = property;
        _startDate = startDate;
        _endDate = endDate;
    }

    public Guid Id { get; set; }

    public Guid? SupercededByPropertyPeriodId { get; protected set; }
    public virtual BasePropertyPeriod? SupercededByPropertyPeriod { get; set; }

    public virtual bool CanSupercedePeriods => false;
    public virtual bool CanBeSuperceded => false;

    public abstract bool CanReviseStartDate { get; }
    public abstract bool CanReviseEndDate { get; }

    private DateTime _startDate;
    public virtual DateTime StartDate {
        get => SupercededByPropertyPeriod?.StartDate ?? _startDate;
        protected set
        {
            if (SupercededByPropertyPeriodId.HasValue)
                throw new PropertyPeriodViolation(this, "Unable to set start date of superceded property period directly");

            _startDate = value;
        }
    }

    private DateTime? _endDate;
    public virtual DateTime? EndDate
    {
        get => SupercededByPropertyPeriod?.StartDate ?? _endDate;
        protected set
        {
            if (SupercededByPropertyPeriodId.HasValue)
                throw new PropertyPeriodViolation(this, "Unable to set end date of superceded property period directly");

            _endDate = value;
        }
    }

    public Guid PropertyId { get; private set; }
    public virtual Property Property { get; set; }

    public BasePropertyPeriod? PeriodBeforeThisOne => Property.GetPropertyPeriodAtDate(StartDate.AddDays(-1));
    public BasePropertyPeriod? PeriodAfterThisOne => EndDate.HasValue ? Property.GetPropertyPeriodAtDate(EndDate.Value.AddDays(1)) : null;

    public virtual void ReviseStartDate(DateTime newStartDate)
    {
        if (!CanReviseStartDate)
            throw new PropertyPeriodViolation(this, "You can't revise the start date of this property period");

        if (StartDate.Date == newStartDate.Date)
            throw new PropertyPeriodViolation(this, "You can't revise the start date to be equal to the existing start date");

        var periodBeforeThisOne = PeriodBeforeThisOne;

        StartDate = newStartDate;

        var periodBeforeDesiredEndDate = newStartDate.AddDays(-1);

        // Handle revising of the adjacent periods
        if (periodBeforeThisOne is not null && periodBeforeThisOne.EndDate != periodBeforeDesiredEndDate)
        {
            // If we're an occupied period and our start date is going to equal the start date of the period before us and it's void then we supercede it.
            var isSuperceding = periodBeforeThisOne.CanBeSuperceded && newStartDate.Date == periodBeforeThisOne.StartDate && CanSupercedePeriods;

            if (newStartDate.Date <= periodBeforeThisOne.StartDate && !isSuperceding)
                throw new PropertyPeriodViolation(this, "You can't revise the start date beyond the start date of the previous property period");

            if (isSuperceding)
            {
                periodBeforeThisOne.SupersedePeriod(this);
                return;
            }

            // Similar to comment in Revise End Date just the opposite
            if (periodBeforeThisOne is OccupiedPropertyPeriod)
            {
                var gapFillingVoidPropertyPeriod = new VoidPropertyPeriod(Property, startDate: periodBeforeThisOne.EndDate!.Value.AddDays(1), endDate: StartDate.AddDays(-1));
                Property.AddPropertyPeriod(gapFillingVoidPropertyPeriod);

                return; // We don't want to revise the start date of the occupied period
            }

            periodBeforeThisOne.ReviseEndDate(periodBeforeDesiredEndDate);
        }
    }

    public virtual void ReviseEndDate(DateTime newEndDate)
    {
        if (!CanReviseEndDate || !EndDate.HasValue)
            throw new PropertyPeriodViolation(this, "You can't revise the end date of this property period");

        if (EndDate.HasValue && EndDate?.Date == newEndDate.Date)
            throw new PropertyPeriodViolation(this, "You can't revise the end date to be equal to the existing end date");

        var periodAfterThisOne = PeriodAfterThisOne;

        EndDate = newEndDate;

        var periodBeforeDesiredStartDate = newEndDate.AddDays(1);

        // Handle revising of the adjacent periods
        if (periodAfterThisOne is not null && periodAfterThisOne.StartDate != periodBeforeDesiredStartDate)
        {
            // If we're an occupied period and our end date is going to equal the end date of the period before us and it's void then we supercede it.
            var isSuperceding = periodAfterThisOne.CanBeSuperceded && newEndDate.Date == periodAfterThisOne.EndDate && CanSupercedePeriods;

            if (newEndDate.Date >= periodAfterThisOne.EndDate && !isSuperceding)
                throw new PropertyPeriodViolation(this, "You can't revise the end date beyond the end date of the previous property period");

            if (isSuperceding)
            {
                periodAfterThisOne.SupersedePeriod(this);
                return;
            }

            // If it's an occupied property period and we're at this stage it means we're revising the end date backwards so it doesn't overlap
            // We need to create a void property period between the period we're revising backwards and the pre-existing occupied after that one.
            if (periodAfterThisOne is OccupiedPropertyPeriod)
            {
                var gapFillingVoidPropertyPeriod = new VoidPropertyPeriod(Property, startDate: newEndDate.AddDays(1), endDate: periodAfterThisOne.StartDate.AddDays(-1));
                Property.AddPropertyPeriod(gapFillingVoidPropertyPeriod);

                return; // We don't want to revise the start date of the occupied period
            }

            periodAfterThisOne.ReviseStartDate(periodBeforeDesiredStartDate);
        }
    }

    public void RemoveEndDate() => EndDate = null;

    public void SupersedePeriod(BasePropertyPeriod supercedingPropertyPeriod)
    {
        if (SupercededByPropertyPeriodId.HasValue)
            throw new PropertyPeriodViolation(this, "Unable to supersede a period after it's already been superseded");

        // This would cause references to start and end dates to cause access violation exceptions crashing the whole project
        // Under normal usage this should never happen but including this to guard against it is a MUST
        if (supercedingPropertyPeriod.SupercededByPropertyPeriod == this)
            throw new InvalidOperationException("You can't supercede a property period using a period that is superceding it");

        SupercededByPropertyPeriodId = supercedingPropertyPeriod.Id;
        SupercededByPropertyPeriod = supercedingPropertyPeriod;
    }

    public void UnsupersedePeriod()
    {
        if (!SupercededByPropertyPeriodId.HasValue)
            throw new PropertyPeriodViolation(this, "Unable to remove superseded period when period isn't superceded to begin with");

        SupercededByPropertyPeriodId = null;
        SupercededByPropertyPeriod = null;
    }

    public virtual void DisposeProperty(DateTime disposalDate)
    {
        var disposedPropertyPeriod = new DisposedPropertyPeriod(Property, disposalDate);
        Property.AddPropertyPeriod(disposedPropertyPeriod);

        EndDate = disposalDate.AddDays(-1);
    }

    public virtual void EndDevelopment(DateTime endDate) => throw new PropertyPeriodViolation(this, "This property isn't in development");

    public virtual void EndOccupancy(DateTime occupancyEndDate) => throw new PropertyPeriodViolation(this, "A property must be occupied before you can end an occupancy");

    public abstract Occupancy StartOccupancy(DateTime occupancyStartDate, string uORN);
}
