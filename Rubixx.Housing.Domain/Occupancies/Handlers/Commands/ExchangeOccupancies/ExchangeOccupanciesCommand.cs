using Rubixx.Housing.Domain.Occupancies.Entities;
using RubixxExtensibility.SharedLibrary.Common.Interfaces;

namespace Rubixx.Housing.Domain.Occupancies.Handlers.Commands.ExchangeOccupancies;

public record ExchangeOccupanciesCommand // Interfaces are ommitted from examples, should be using IRequest from the Mediator package
{
    public DateTime ExchangeDate { get; set; } = DateTime.Today;

    public IReadOnlyList<OccupancyToOccupancyTransferDto> OccupanciesToTransfer { get; set; } = Array.Empty<OccupancyToOccupancyTransferDto>();
}

public class ExchangeOccupanciesCommandHandler
{
    private readonly IRepository<Occupancy> _occupancyRepository;

    public ExchangeOccupanciesCommandHandler(IRepository<Occupancy> occupancyRepository)
    {
        _occupancyRepository = occupancyRepository;
    }

    public async Task Handle(ExchangeOccupanciesCommand command, CancellationToken cancellationToken)
    {
        // TODO: General guard clauses

        var endDate = command.ExchangeDate.AddDays(-1);

        foreach (var occupancyTransfer in command.OccupanciesToTransfer)
        {
            // TODO: Logic for swapping each occupancy

            /* 
             * If one of the occupancies is in future or has end dates should it be ignored?
             * 
             * Current flow should be guard clauses followed by logic to end both occupancies and create the new exchanged occupancy.
             * Something to keep in mind is this will end two occupancies but only start 1 per exchange as each row doesn't contain the destination of the right side.
             * This means any guard clauses relating to end dates should be done prior to this loop even running as occupancies will have different end dates now.
             * 
             */
        }
    }
}