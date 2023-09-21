using Rubixx.Housing.Domain.Properties.Entities;
using Rubxx.Housing.Domain.UnitTests.Internal.Common.Extensions;

namespace Rubxx.Housing.Domain.UnitTests.Properties.Methods;

[TestFixture(TestOf = typeof(Property))]
internal class Property_ReinstateOccupancy
{
    [Test(Description = "Ensure property periods are created correctly")]
    public void VoidPropertyWithASinglePreviousOccupiedPeriod_AllowsReinstatingOfPreviousOccupancy()
    {
        // Arrange
        var voidStartDate = DateTime.Today;

        var occupancyStartDate = DateTime.Today.AddDays(7);
        var occupancyEndDate = occupancyStartDate.AddDays(4);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate: voidStartDate, isDevelopment: false);

        var occupancy = property.StartOccupancy(occupancyStartDate, "ALB03-001");
        property.EndOccupancy(occupancyEndDate);

        var voidPeriod = property.LatestPropertyPeriod;

        // Act

        occupancy.ReinstateOccupancy();

        // Assert

        Assert.Multiple(() =>
        {
            Assert.That(voidPeriod.SupercededByPropertyPeriod, Is.EqualTo(occupancy.OccupiedPropertyPeriod));
            Assert.That(occupancy.OccupiedPropertyPeriod.EndDate.HasValue, Is.False);

            Assert.That(property.ValidatePropertyPeriods(), Is.True);
        });
    }

    [Test(Description = "Ensure property periods are created correctly")]
    public void OccupiedPropertyWithASinglePreviousOccupiedPeriod_ThrowsExceptionWhenAttemptingToReinstatePreviousOccupancy()
    {
        // Arrange
        var voidStartDate = DateTime.Today;

        var occupancyStartDate = DateTime.Today.AddDays(7);
        var occupancyEndDate = occupancyStartDate.AddDays(4);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate: voidStartDate, isDevelopment: false);

        var occupancy = property.StartOccupancy(occupancyStartDate, "ALB03-001");
        property.EndOccupancy(occupancyEndDate);

        property.StartOccupancy(occupancyEndDate.AddDays(1), "ALB03-002");

        // Act

        var exception = Assert.Throws<InvalidOperationException>(occupancy.ReinstateOccupancy);

        // Assert

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Is.EqualTo("This occupancy can't be reinstated"));

            Assert.That(property.ValidatePropertyPeriods(), Is.True);
        });
    }

    [Test(Description = "Ensure property periods are created correctly")]
    public void ReinstatingOccupancyThatWasCancelledAndCreatedANewVoidPeriod_CorrectlySupercedesTheOverlappingVoidPeriod()
    {
        // Arrange
        var voidStartDate = DateTime.Today;

        var occupancyStartDate = DateTime.Today.AddDays(7);
        var occupancyEndDate = occupancyStartDate.AddDays(4);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate: voidStartDate, isDevelopment: false);

        property.StartOccupancy(occupancyStartDate, "ALB03-001");
        property.EndOccupancy(occupancyEndDate);

        var occupancy = property.StartOccupancy(occupancyEndDate.AddDays(1), "ALB03-002");
        property.CancelOccupancy();

        var voidPeriod = property.LatestPropertyPeriod;

        // Act

        occupancy.ReinstateOccupancy();

        // Assert

        Assert.Multiple(() =>
        {
            Assert.That(voidPeriod.SupercededByPropertyPeriod, Is.EqualTo(occupancy.OccupiedPropertyPeriod));
            Assert.That(occupancy.OccupiedPropertyPeriod.EndDate.HasValue, Is.False);

            Assert.That(property.ValidatePropertyPeriods(), Is.True, "Property Periods are invalid");
        });
    }
}
