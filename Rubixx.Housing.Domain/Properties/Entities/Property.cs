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

    public BasePropertyPeriod CurrentPropertyPeriod => GetPropertyPeriodAtDate(DateTime.Today) ?? throw new InvalidOperationException("A property should always have a valid current period");

    public BasePropertyPeriod LatestPropertyPeriod => _propertyPeriods.Single(e => !e.EndDate.HasValue);

    public BasePropertyPeriod? GetPropertyPeriodAtDate(DateTime date) => _propertyPeriods.First(e => e.StartDate <= date && (!e.EndDate.HasValue || e.EndDate >= date));

    public void AddPropertyPeriod(BasePropertyPeriod period) => _propertyPeriods.Add(period);

    public void DisposeProperty() => LatestPropertyPeriod.DisposeProperty();

    public void EndDevelopment() => LatestPropertyPeriod.EndDevelopment();

    public void EndOccupancy(DateTime occupancyEndDate) => LatestPropertyPeriod.EndOccupancy(occupancyEndDate);

    public void StartOccupancy(DateTime occupancyStartDate, string uORN) => (GetPropertyPeriodAtDate(occupancyStartDate) ?? LatestPropertyPeriod).StartOccupancy(occupancyStartDate, uORN);

}
