using Rubixx.Housing.Domain.Properties.Entities;
using Rubixx.Housing.Domain.Properties.Entities.Periods;
using Rubixx.Housing.Domain.Properties.Exceptions;
using Rubxx.Housing.Domain.UnitTests.Internal.Common.Extensions;

namespace Rubxx.Housing.Domain.UnitTests.Occupancies.Methods;

[TestFixture]
internal class Occupancy_ReviseStartDate
{
    [Test]
    public void OccupancyWithVoidPeriodBeforeIt_RevisesStartDate()
    {
        // Arrange
        var voidStartDate = DateTime.Today.AddDays(-7);
        var occupancyStartDate = DateTime.Today;
        var revisedOccupancyStartDate = occupancyStartDate.AddDays(-2);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        var occupancy = property.StartOccupancy(occupancyStartDate, "ALB03-001");

        // Act
        occupancy.ReviseStartDate(revisedOccupancyStartDate);

        // Assert
        var voidPropertyPeriod = property.PropertyPeriods.OfType<VoidPropertyPeriod>().Single();
        var occupiedPropertyPeriod = property.PropertyPeriods.OfType<OccupiedPropertyPeriod>().Single();

        Assert.Multiple(() =>
        {
            Assert.That(occupiedPropertyPeriod, Is.Not.Null);
            Assert.That(voidPropertyPeriod, Is.Not.Null);

            Assert.That(voidPropertyPeriod.EndDate, Is.EqualTo(revisedOccupancyStartDate.AddDays(-1)));
            Assert.That(occupiedPropertyPeriod.StartDate, Is.EqualTo(revisedOccupancyStartDate));
        });

        Assert.That(property.ValidatePropertyPeriods(), Is.True);
    }

    [Test]
    public void ReviseStartDateIntoPeriodBeforeOccupancyWherePeriodBeforeIsEndedOccupancy_WarnsToReviseEndDateFirst()
    {
        // Arrange
        var voidStartDate = DateTime.Today.AddDays(-7);

        var occupancyStartDate = DateTime.Today;
        var occupancyEndDate = occupancyStartDate.AddDays(1);

        var secondOccupancyStartDate = occupancyEndDate.AddDays(1);

        var revisedOccupancyStartDate = secondOccupancyStartDate.AddDays(-1);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        property.StartOccupancy(occupancyStartDate, "ALB03-001");
        property.EndOccupancy(occupancyEndDate);

        var occupancy = property.StartOccupancy(secondOccupancyStartDate, "ALB03-002");

        // Act
        var exception = Assert.Throws<PropertyPeriodViolation>(() => occupancy.ReviseStartDate(revisedOccupancyStartDate));

        // Assert
        Assert.That(exception.Message, Is.EqualTo("You must revise the end date of the occupancy before this one first"));

        Assert.That(property.ValidatePropertyPeriods(), Is.True);
    }

    [Test]
    public void ReviseStartDateAwayFromPeriodBeforeOccupancyWherePeriodBeforeIsEndedOccupancy_FillsGapInWithVoidPeriod()
    {
        // Arrange
        var voidStartDate = DateTime.Today.AddDays(-7);

        var occupancyStartDate = DateTime.Today;
        var occupancyEndDate = occupancyStartDate.AddDays(1);

        var secondOccupancyStartDate = occupancyEndDate.AddDays(1);

        var revisedOccupancyStartDate = secondOccupancyStartDate.AddDays(3);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        property.StartOccupancy(occupancyStartDate, "ALB03-001");
        property.EndOccupancy(occupancyEndDate);

        var occupancy = property.StartOccupancy(secondOccupancyStartDate, "ALB03-002");

        // Act
        occupancy.ReviseStartDate(revisedOccupancyStartDate);

        // Assert
        var gapFillingVoidPeriod = property.PropertyPeriods.OfType<VoidPropertyPeriod>().Single(e => e.StartDate == occupancyEndDate.AddDays(1) && e.EndDate == revisedOccupancyStartDate.AddDays(-1));

        Assert.Multiple(() =>
        {
            Assert.That(gapFillingVoidPeriod, Is.Not.Null);
        });

        Assert.That(property.ValidatePropertyPeriods(), Is.True);
    }

    [Test]
    public void ReviseStartDateTowardsPeriodBeforeOccupancyWherePeriodBeforeIsEndedOccupancy_SupersedesVoidPeriodBetweenThem()
    {
        // Arrange
        var voidStartDate = DateTime.Today.AddDays(-7);

        var occupancyStartDate = DateTime.Today;
        var occupancyEndDate = occupancyStartDate.AddDays(1);

        var secondOccupancyStartDate = occupancyEndDate.AddDays(4);

        var revisedOccupancyStartDate = occupancyEndDate.AddDays(1);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        property.StartOccupancy(occupancyStartDate, "ALB03-001");
        property.EndOccupancy(occupancyEndDate);

        var occupancy = property.StartOccupancy(secondOccupancyStartDate, "ALB03-002");

        var voidPeriodBetweenOccupancies = property.PropertyPeriods.OfType<VoidPropertyPeriod>().Single(e => e.StartDate == occupancyEndDate.AddDays(1) && e.EndDate == secondOccupancyStartDate.AddDays(-1));

        // Act
        occupancy.ReviseStartDate(revisedOccupancyStartDate);

        // Assert
        Assert.Multiple(() =>
        {
            // Ensure the void period was superseded by the occupancy we're revising
            Assert.That(voidPeriodBetweenOccupancies, Is.Not.Null);
            Assert.That(voidPeriodBetweenOccupancies.OccupiedPropertyPeriodId.HasValue, Is.True);
            Assert.That(voidPeriodBetweenOccupancies.OccupiedPropertyPeriod, Is.EqualTo(occupancy.OccupiedPropertyPeriod));

            Assert.That(property.ValidatePropertyPeriods(), Is.True);
        });
    }
}
