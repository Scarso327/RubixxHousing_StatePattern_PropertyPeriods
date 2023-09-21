using Rubixx.Housing.Domain.Occupancies.Entities;
using Rubixx.Housing.Domain.Properties.Entities;
using RubixxExtensibility.SharedLibrary.Common.Exceptions;
using RubixxExtensibility.SharedLibrary.Common.Interfaces;

namespace Rubixx.Housing.Domain.Occupancies.Handlers.Commands.TransferOccupancy;

public class TransferOccupancyCommand
{
    public required Guid OccupancyId { get; set; }

    public required Guid TargetPropertyId { get; set; }

    public required DateTime TransferDate { get; set; } = DateTime.Today;

    public required string NewUORN { get; set; } = string.Empty;
}

public class TransferOccupancyCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Property> _propertyRepository;
    private readonly IRepository<Occupancy> _occupancyRepository;

    public TransferOccupancyCommandHandler(IUnitOfWork unitOfWork, IRepository<Property> propertyRepository, IRepository<Occupancy> occupancyRepository)
    {
        _unitOfWork = unitOfWork;
        _propertyRepository = propertyRepository;
        _occupancyRepository = occupancyRepository;
    }

    public async Task<Occupancy> Handle(TransferOccupancyCommand command, CancellationToken cancellationToken)
    {
        // Guard Clauses
        var isUORNInUse = _occupancyRepository.Any(o => o.UORN.ToLower() == command.NewUORN.ToLower());

        if (isUORNInUse)
            throw new InvalidOperationException("The specified UORN is already in use");

        // Get occupancy being transferred
        var occupancy = await _occupancyRepository.GetByIdAsync(command.OccupancyId)
            ?? throw new NotFoundException(typeof(Occupancy), command.OccupancyId);

        // More Guard Clauses
        if (occupancy.EndDate.HasValue)
            throw new InvalidOperationException("You can't transfer an occupancy once it's ended");

        if (command.TransferDate <= occupancy.StartDate)
            throw new InvalidOperationException("The transfer date can't be before the occupancy started");

        // Get new property
        var property = await _propertyRepository.GetByIdAsync(command.TargetPropertyId)
            ?? throw new NotFoundException(typeof(Property), command.TargetPropertyId);

        // First we try starting the "new" occupancy
        var newOccupancy = property.StartOccupancy(command.TransferDate, command.NewUORN);

        // Now End Current Occupancy
        occupancy.OccupiedPropertyPeriod.Property.EndOccupancy(command.TransferDate);

        // We don't need to call repo save methods here as EF would've tracked our changes before this point
        await _unitOfWork.CommitAsync(cancellationToken);

        return newOccupancy;
    }
}