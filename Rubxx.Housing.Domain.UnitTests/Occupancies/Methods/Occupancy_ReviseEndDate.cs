﻿using Rubixx.Housing.Domain.Properties.Entities;
using Rubixx.Housing.Domain.Properties.Entities.Periods;
using Rubixx.Housing.Domain.Properties.Exceptions;
using Rubxx.Housing.Domain.UnitTests.Internal.Common.Extensions;

namespace Rubxx.Housing.Domain.UnitTests.Occupancies.Methods;

[TestFixture]
internal class Occupancy_ReviseEndDate
{
    [Test]
    public void EndedOccupancyWithVoidPeriodAfterIt_RevisesEndDate()
    {
        // Arrange
        var voidStartDate = DateTime.Today.AddDays(-7);

        var occupancyStartDate = DateTime.Today;
        var occupancyEndDate = occupancyStartDate.AddDays(2);

        var revisedOccupancyEndDate = occupancyEndDate.AddDays(2);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        var occupancy = property.StartOccupancy(occupancyStartDate, "ALB03-001");
        property.EndOccupancy(occupancyEndDate);

        // Act
        occupancy.ReviseEndDate(revisedOccupancyEndDate, null);

        // Assert
        var latestVoidPropertyPeriod = property.PropertyPeriods.OfType<VoidPropertyPeriod>().OrderBy(e => e.StartDate).Last();
        var occupiedPropertyPeriod = property.PropertyPeriods.OfType<OccupiedPropertyPeriod>().Single();

        Assert.Multiple(() =>
        {
            Assert.That(occupiedPropertyPeriod, Is.Not.Null);
            Assert.That(latestVoidPropertyPeriod, Is.Not.Null);

            Assert.That(latestVoidPropertyPeriod.StartDate, Is.EqualTo(revisedOccupancyEndDate.AddDays(1)));
            Assert.That(occupiedPropertyPeriod.EndDate, Is.EqualTo(revisedOccupancyEndDate));
        });

        Assert.That(property.ValidatePropertyPeriods(), Is.True);
    }

    [Test]
    public void ReviseEndDateIntoPeriodAfterEndedOccupancyWherePeriodAfterIsOccupied_WarnsToReviseStartDateFirst()
    {
        // Arrange
        var voidStartDate = DateTime.Today.AddDays(-7);

        var occupancyStartDate = DateTime.Today;
        var occupancyEndDate = occupancyStartDate.AddDays(1);

        var secondOccupancyStartDate = occupancyEndDate.AddDays(1);

        var revisedOccupancyEndDate = occupancyEndDate.AddDays(3);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        var occupancy = property.StartOccupancy(occupancyStartDate, "ALB03-001");
        property.EndOccupancy(occupancyEndDate);

        property.StartOccupancy(secondOccupancyStartDate, "ALB03-002");

        // Act
        var exception = Assert.Throws<PropertyPeriodViolation>(() => occupancy.ReviseEndDate(revisedOccupancyEndDate, null));

        // Assert
        Assert.That(exception.Message, Is.EqualTo("You must revise the start date of the occupancy after this one first"));

        Assert.That(property.ValidatePropertyPeriods(), Is.True);
    }

    [Test]
    public void ReviseEndDateAwayFromPeriodAfterEndedOccupancyWherePeriodAfterIsOccupied_FillsGapBetweenWithVoidPeriod()
    {
        // Arrange
        var voidStartDate = DateTime.Today.AddDays(-7);

        var occupancyStartDate = DateTime.Today.AddDays(-5);
        var occupancyEndDate = occupancyStartDate.AddDays(5);

        var secondOccupancyStartDate = occupancyEndDate.AddDays(1);

        var revisedOccupancyEndDate = occupancyEndDate.AddDays(-3);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        var occupancy = property.StartOccupancy(occupancyStartDate, "ALB03-001");
        property.EndOccupancy(occupancyEndDate);

        property.StartOccupancy(secondOccupancyStartDate, "ALB03-002");

        // Act
        occupancy.ReviseEndDate(revisedOccupancyEndDate, null);

        // Assert
        var gapFillingVoidPeriod = property.PropertyPeriods.OfType<VoidPropertyPeriod>().Single(e => e.StartDate == revisedOccupancyEndDate.AddDays(1) && e.EndDate == secondOccupancyStartDate.AddDays(-1));

        Assert.Multiple(() =>
        {
            Assert.That(gapFillingVoidPeriod, Is.Not.Null);
        });

        Assert.That(property.ValidatePropertyPeriods(), Is.True);
    }

    [Test]
    public void ReviseEndDateTowardsPeriodAfterEndedOccupancyWherePeriodAfterIsOccupied_SupersedesVoidPeriodBetweenThem()
    {
        // Arrange
        var voidStartDate = DateTime.Today.AddDays(-7);

        var occupancyStartDate = DateTime.Today.AddDays(-5);
        var occupancyEndDate = occupancyStartDate.AddDays(5);

        var secondOccupancyStartDate = occupancyEndDate.AddDays(4);

        var revisedOccupancyEndDate = secondOccupancyStartDate.AddDays(-1);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        var occupancy = property.StartOccupancy(occupancyStartDate, "ALB03-001");
        property.EndOccupancy(occupancyEndDate);

        property.StartOccupancy(secondOccupancyStartDate, "ALB03-002");

        var voidPeriodBetweenOccupancies = property.PropertyPeriods.OfType<VoidPropertyPeriod>().Single(e => e.StartDate == occupancyEndDate.AddDays(1) && e.EndDate == secondOccupancyStartDate.AddDays(-1));

        // Act
        occupancy.ReviseEndDate(revisedOccupancyEndDate, null);

        // Assert
        Assert.Multiple(() =>
        {
            // Ensure the void period was superseded by the occupancy we're revising
            Assert.That(voidPeriodBetweenOccupancies, Is.Not.Null);
            Assert.That(voidPeriodBetweenOccupancies.SupercededByPropertyPeriodId.HasValue, Is.True);
            Assert.That(voidPeriodBetweenOccupancies.SupercededByPropertyPeriod, Is.EqualTo(occupancy.OccupiedPropertyPeriod));

            Assert.That(property.ValidatePropertyPeriods(), Is.True);
        });
    }
}
