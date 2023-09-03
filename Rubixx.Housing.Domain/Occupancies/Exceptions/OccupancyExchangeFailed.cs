using Rubixx.Housing.Domain.Properties.Entities;

namespace Rubixx.Housing.Domain.Occupancies.Exceptions;

public class OccupancyExchangeFailed : Exception
{
    public OccupancyExchangeFailed(string? message) : base(message ?? "Failed to exchange occupancy") { }

    public OccupancyExchangeFailed(Property property, string? message) : base(message ?? $"Failed to exchange occupancy from property {property.UPRN}") { }
}
