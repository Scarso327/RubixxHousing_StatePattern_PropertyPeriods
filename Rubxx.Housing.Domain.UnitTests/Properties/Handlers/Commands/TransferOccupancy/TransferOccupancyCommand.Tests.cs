using Moq;
using Rubixx.Housing.Domain.Occupancies.Entities;
using Rubixx.Housing.Domain.Occupancies.Handlers.Commands.TransferOccupancy;
using Rubixx.Housing.Domain.Properties.Entities;
using Rubixx.Housing.Domain.Properties.Exceptions;
using RubixxExtensibility.SharedLibrary.Common.Interfaces;
using Rubxx.Housing.Domain.UnitTests.Internal.Common.Extensions;

namespace Rubxx.Housing.Domain.UnitTests.Properties.Handlers.Commands.TransferOccupancy;

[TestFixture]
internal class TransferOccupancyCommandTests
{
    private Mock<IUnitOfWork> _unitOfWork;
    private Mock<IRepository<Occupancy>> _occupancyRepository;
    private Mock<IRepository<Property>> _propertyRepository;

    [SetUp]
    public void Setup()
    {
        _unitOfWork = new Mock<IUnitOfWork>();
        _occupancyRepository = new Mock<IRepository<Occupancy>>();
        _propertyRepository = new Mock<IRepository<Property>>();
    }

    [Test]
    public async Task UnendedOccupancyTransferToVoid_Works()
    {
        var voidStartDate = DateTime.Today.AddDays(-7);
        var occupancyStartDate = DateTime.Today.AddDays(3);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);
        var targetProperty = new Property(uPRN: "ALB05", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        var occupancy = property.StartOccupancy(occupancyStartDate, "ALB03-001");

        var command = new TransferOccupancyCommand()
        {
            OccupancyId = occupancy.Id,
            TransferDate = occupancyStartDate.AddDays(1),
            NewUORN = "ALB05-001",
            TargetPropertyId = targetProperty.Id,
        };

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        // Setup GetByIdAsync returns
        _propertyRepository
            .Setup(e => e.GetByIdAsync(It.IsAny<Guid>()))
            .Returns(Task.FromResult(targetProperty));

        _occupancyRepository
            .Setup(e => e.GetByIdAsync(It.IsAny<Guid>()))
            .Returns(Task.FromResult(occupancy));

#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        var handler = new TransferOccupancyCommandHandler(_unitOfWork.Object, _propertyRepository.Object, _occupancyRepository.Object);

        var transferredOccupancy = await handler.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(transferredOccupancy.UORN, Is.EqualTo(command.NewUORN));
            Assert.That(transferredOccupancy.StartDate, Is.EqualTo(command.TransferDate));
            Assert.That(occupancy.EndDate, Is.EqualTo(command.TransferDate));
        });

        Assert.Multiple(() =>
        {
            Assert.That(property.ValidatePropertyPeriods(), Is.True);
            Assert.That(targetProperty.ValidatePropertyPeriods(), Is.True);
        });
    }

    [Test]
    public void UnendedOccupancyTransferToOccupiedProperty_ThrowsException()
    {
        var voidStartDate = DateTime.Today.AddDays(-7);
        var occupancyStartDate = voidStartDate.AddDays(3);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);
        var targetProperty = new Property(uPRN: "ALB05", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        var occupancy = property.StartOccupancy(occupancyStartDate, "ALB03-001");
        var blockingOccupancy = targetProperty.StartOccupancy(occupancyStartDate, "ALB05-001");

        var command = new TransferOccupancyCommand()
        {
            OccupancyId = occupancy.Id,
            TransferDate = DateTime.Today,
            NewUORN = "ALB05-002",
            TargetPropertyId = targetProperty.Id,
        };

#pragma warning disable CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        // Setup GetByIdAsync returns
        _propertyRepository
            .Setup(e => e.GetByIdAsync(It.IsAny<Guid>()))
            .Returns(Task.FromResult(targetProperty));

        _occupancyRepository
            .Setup(e => e.GetByIdAsync(It.IsAny<Guid>()))
            .Returns(Task.FromResult(occupancy));

#pragma warning restore CS8620 // Argument cannot be used for parameter due to differences in the nullability of reference types.

        var handler = new TransferOccupancyCommandHandler(_unitOfWork.Object, _propertyRepository.Object, _occupancyRepository.Object);

        var exception = Assert.ThrowsAsync<PropertyPeriodViolation>(async () => await handler.Handle(command, CancellationToken.None));
        Assert.That(exception.Message, Is.EqualTo("This property has an occupancy on the date you've specified"));

        Assert.Multiple(() =>
        {
            Assert.That(property.ValidatePropertyPeriods(), Is.True);
            Assert.That(targetProperty.ValidatePropertyPeriods(), Is.True);
        });
    }
}
