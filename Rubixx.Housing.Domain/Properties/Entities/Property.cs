using Rubixx.Housing.Domain.Common.Extensions;
using Rubixx.Housing.Domain.Occupancies.Entities;
using Rubixx.Housing.Domain.Properties.Entities.Periods;
using RubixxExtensibility.SharedLibrary.Common.Interfaces;

namespace Rubixx.Housing.Domain.Properties.Entities;

public class Property : IEntity
{
    protected Property() { }

    public Property(string uPRN, bool isLettablePropertyType, DateTime? voidStartDate, bool isDevelopment)
    {
        UPRN = uPRN;

        if (isDevelopment)
        {
            _propertyPeriods.Add(new DevelopmentPropertyPeriod(this, DateTime.Today));
            return;
        }

        if (isLettablePropertyType)
        {
            if (!voidStartDate.HasValue)
                throw new InvalidOperationException("No start date was provided");

            _propertyPeriods.Add(new VoidPropertyPeriod(this, voidStartDate.Value));

            // If the start date of the void is in the future we assume from the current date till the day before it was in development
            if (voidStartDate > DateTime.Today)
                _propertyPeriods.Add(new DevelopmentPropertyPeriod(this, DateTime.Today, voidStartDate.Value.AddDays(-1)));

            return; 
        }

        _propertyPeriods.Add(new UnlettablePropertyPeriod(this, DateTime.Today));
    }

    public Guid Id { get; set; }

    public string UPRN { get; private set; }

    private readonly List<BasePropertyPeriod> _propertyPeriods = new();
    public virtual IReadOnlyList<BasePropertyPeriod> PropertyPeriods => _propertyPeriods.AsReadOnly();

    /// <summary>
    /// Returns same as <see cref="PropertyPeriods"/> but excludes any <see cref="VoidPropertyPeriod"/> where <see cref="VoidPropertyPeriod.OccupiedPropertyPeriodId"/> has an id
    /// </summary>
    public IReadOnlyList<BasePropertyPeriod> PropertyPeriodsWithoutSupercededVoids
        => _propertyPeriods.Where(e => !e.SupercededByPropertyPeriodId.HasValue).ToList().AsReadOnly();

    public BasePropertyPeriod CurrentPropertyPeriod => GetPropertyPeriodAtDate(DateTime.Today) ?? throw new InvalidOperationException("A property should always have a valid current period");

    public BasePropertyPeriod LatestPropertyPeriod => _propertyPeriods.Single(e => !e.EndDate.HasValue); // No need to filter superceded voids as superceded voids always have end dates

    public BasePropertyPeriod? GetPropertyPeriodAtDate(DateTime date) => PropertyPeriodsWithoutSupercededVoids.First(e => e.StartDate <= date && (!e.EndDate.HasValue || e.EndDate >= date));

    public void AddPropertyPeriod(BasePropertyPeriod period) => _propertyPeriods.Add(period);

    public void DisposeProperty(DateTime disposalDate)
    {
        disposalDate.ThrowIfInPast();

        LatestPropertyPeriod.DisposeProperty(disposalDate);
    }

    public void EndDevelopment(DateTime endDate)
    {
        endDate.ThrowIfInPast();

        LatestPropertyPeriod.EndDevelopment(endDate);
    }

    public void EndOccupancy(DateTime occupancyEndDate) => LatestPropertyPeriod.EndOccupancy(occupancyEndDate);

    public Occupancy StartOccupancy(DateTime occupancyStartDate, string uORN) => (GetPropertyPeriodAtDate(occupancyStartDate) ?? LatestPropertyPeriod).StartOccupancy(occupancyStartDate, uORN);

    public void CancelOccupancy()
    {
        var latestPropertyPeriod = LatestPropertyPeriod;

        if (latestPropertyPeriod is not OccupiedPropertyPeriod occupiedPropertyPeriod)
            throw new InvalidOperationException("This property is not currently occupied");

        if (!occupiedPropertyPeriod.CanBeCancelled)
            throw new InvalidOperationException("This occupancy can't be canncelled");

        // Finding the void period to use

        // 1 - We use the void period that was superceded by the occupancy if start dates still match
        var voidPeriod = _propertyPeriods.OfType<VoidPropertyPeriod>().SingleOrDefault(p => p.SupercededByPropertyPeriod == occupiedPropertyPeriod)
            // 2 - If the period before isn't void we create a new one
            ?? (occupiedPropertyPeriod.PeriodBeforeThisOne is not VoidPropertyPeriod voidPeriodBeforeOccupancy ? 
                new VoidPropertyPeriod(this, occupiedPropertyPeriod.StartDate)
                    // 3 - If it is a void, use that
                    : voidPeriodBeforeOccupancy);

        // Superceded ourselves
        occupiedPropertyPeriod.SupersedePeriod(voidPeriod);

        if (voidPeriod.EndDate.HasValue) voidPeriod.RemoveEndDate();

        // NOTE: This isn't included in this example but at this stage we need to take all transactions from all accounts on the occupancy and move them onto the void period
    }

#pragma warning disable CA1822 // Mark members as static
    public void ReinstateOccupancy(OccupiedPropertyPeriod periodToReinstate)
#pragma warning restore CA1822 // Mark members as static
    {
        if (!periodToReinstate.CanBeReinstated)
            throw new InvalidOperationException("This occupancy can't be reinstated");

        if (periodToReinstate.PeriodAfterThisOne is not VoidPropertyPeriod voidPeriod || voidPeriod.EndDate.HasValue)
            throw new InvalidOperationException("There have been changes to this property's periods meaning you can no longer reinstate this historial occupancy");

        voidPeriod.SupersedePeriod(periodToReinstate);
        periodToReinstate.RemoveEndDate();

        // Cancelled occupancies get their periods superceded by a void, here we just swap it so our occupancy supercedes whatever superceded us
        if (periodToReinstate.SupercededByPropertyPeriod is not null)
            periodToReinstate.SupercedeOverlappingPeriod();
    }

}
