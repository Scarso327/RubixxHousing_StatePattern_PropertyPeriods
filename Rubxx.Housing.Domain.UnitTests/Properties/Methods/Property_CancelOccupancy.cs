using Rubixx.Housing.Domain.Properties.Entities;
using Rubxx.Housing.Domain.UnitTests.Internal.Common.Extensions;

namespace Rubxx.Housing.Domain.UnitTests.Properties.Methods;

[TestFixture(TestOf = typeof(Property))]
internal class Property_CancelOccupancy
{
    [Test(Description = "Ensure property periods are created correctly")]
    public void OccupiedPropertyWithPreviousVoidPeriod_AllowsCancellationAndUsesExistingVoidPeriod()
    {
        // Arrange
        var voidStartDate = DateTime.Today;
        var occupancyStartDate = DateTime.Today.AddDays(7);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate: voidStartDate, isDevelopment: false);
        var voidPeriod = property.PropertyPeriods.Single();

        var occupancy = property.StartOccupancy(occupancyStartDate, "ALB03-001");

        // Act

        property.CancelOccupancy();

        // Assert

        Assert.Multiple(() =>
        {
            Assert.That(occupancy.OccupiedPropertyPeriod.SupercededByPropertyPeriod, Is.EqualTo(voidPeriod));
            Assert.That(occupancy.OccupiedPropertyPeriod.EndDate, Is.EqualTo(voidPeriod.StartDate));

            Assert.That(property.ValidatePropertyPeriods(), Is.True);
        });
    }

    [Test(Description = "Ensure property periods are created correctly")]
    public void VoidPropertyWithPreviousOccupancyPeriod_ThrowsExceptionIfCancellingPreviousOccupancy()
    {
        // Arrange
        var voidStartDate = DateTime.Today;
        var occupancyStartDate = DateTime.Today.AddDays(7);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate: voidStartDate, isDevelopment: false);

        var occupancy = property.StartOccupancy(occupancyStartDate, "ALB03-001");
        property.EndOccupancy(occupancyStartDate.AddDays(1));

        // Act

        var exception = Assert.Throws<InvalidOperationException>(property.CancelOccupancy);

        // Assert

        Assert.Multiple(() =>
        {
            Assert.That(exception.Message, Is.EqualTo("This property is not currently occupied"));

            Assert.That(property.ValidatePropertyPeriods(), Is.True);
        });
    }
}
