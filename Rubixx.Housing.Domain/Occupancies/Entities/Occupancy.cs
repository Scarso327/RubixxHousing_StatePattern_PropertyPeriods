using Rubixx.Housing.Domain.Properties.Entities.Periods;
using RubixxExtensibility.SharedLibrary.Common.Interfaces;

namespace Rubixx.Housing.Domain.Occupancies.Entities;

public class Occupancy : IEntity
{
    protected Occupancy() { }

    public Occupancy(OccupiedPropertyPeriod occupiedPropertyPeriod, string uORN)
    {
        OccupiedPropertyPeriod = occupiedPropertyPeriod;
        UORN = uORN;
    }

    public Guid Id { get; set; }

    public string UORN { get; private set; }

    public DateTime StartDate { get => OccupiedPropertyPeriod.StartDate; private set => _ = value; }
    public DateTime? EndDate { get => OccupiedPropertyPeriod.EndDate; private set => _ = value; }
    public DateTime? NofitiedDate { get => OccupiedPropertyPeriod.NotifiedDate; private set => _ = value; }

    public Guid OccupiedPropertyPeriodId { get; private set; }
    public virtual OccupiedPropertyPeriod OccupiedPropertyPeriod { get; set; }

    public void ReviseStartDate(DateTime newStartDate) => OccupiedPropertyPeriod.ReviseStartDate(newStartDate);

    public void ReviseEndDate(DateTime newEndDate, DateTime? newNotifiedDate) => OccupiedPropertyPeriod.ReviseEndDate(newEndDate, newNotifiedDate ?? newEndDate);
}
