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

    public bool CanBeCancelled => OccupiedPropertyPeriod.CanBeCancelled;
    public bool CanBeReinstated => OccupiedPropertyPeriod.CanBeReinstated;

    public void ReviseStartDate(DateTime newStartDate) => OccupiedPropertyPeriod.ReviseStartDate(newStartDate);

    public void ReviseEndDate(DateTime newEndDate, DateTime? newNotifiedDate) => OccupiedPropertyPeriod.ReviseEndDate(newEndDate, newNotifiedDate ?? newEndDate);

    public void CancelOccupancy()
    {
        OccupiedPropertyPeriod.Property.CancelOccupancy();

        // NOTE: Not included in this example but we need to set end date for all unended occupants against this occupancy here
    }

    public void ReinstateOccupancy()
    {
        var oldEndDate = EndDate; // We need to save this here to do the note provided below as our end date is going to be removed by the Property side of things

        OccupiedPropertyPeriod.Property.ReinstateOccupancy(OccupiedPropertyPeriod);

        // NOTE: Not included in this example project but here we need to remove the end date from all occupants where their end date was equal to our old end date
    }
}
