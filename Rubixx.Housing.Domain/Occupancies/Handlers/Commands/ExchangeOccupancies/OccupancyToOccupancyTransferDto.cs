﻿namespace Rubixx.Housing.Domain.Occupancies.Handlers.Commands.ExchangeOccupancies;

public record OccupancyToOccupancyTransferDto
{
    public Guid OccupancyId { get; set; }

    public Guid TargetOccupancyId { get; set; }
}
