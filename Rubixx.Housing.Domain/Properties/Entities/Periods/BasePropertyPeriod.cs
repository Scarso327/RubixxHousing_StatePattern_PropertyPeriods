using Rubixx.Housing.Domain.Properties.Exceptions;

namespace Rubixx.Housing.Domain.Properties.Entities.Periods;

public abstract class BasePropertyPeriod
{
    protected BasePropertyPeriod() { }

    protected BasePropertyPeriod(Property property, DateTime startDate, DateTime? endDate = null)
    {
        Property = property;
        _startDate = startDate;
        _endDate = endDate;
    }

    private DateTime _startDate;
    public virtual DateTime StartDate { get => _startDate; protected set => _startDate = value; }

    private DateTime? _endDate;
    public virtual DateTime? EndDate { get => _endDate; protected set => _endDate = value; }

    public Guid PropertyId { get; private set; }
    public virtual Property Property { get; set; }

    public abstract void DisposeProperty();

    public virtual void EndDevelopment() => throw new PropertyPeriodViolation(this, "This property isn't in development");

    public abstract void EndOccupancy(DateTime occupancyEndDate);

    public abstract void StartOccupancy(DateTime occupancyStartDate, string uORN);
}
