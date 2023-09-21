using Rubixx.Housing.Domain.Properties.Entities;
using Rubixx.Housing.Domain.Properties.Entities.Periods;
using Rubixx.Housing.Domain.Properties.Exceptions;
using Rubxx.Housing.Domain.UnitTests.Internal.Common.Extensions;

namespace Rubxx.Housing.Domain.UnitTests.Properties.Methods;

[TestFixture(TestOf = typeof(Property))]
internal class Property_EndDevelopment
{
    public const string PropertyIsNotInDevelopmentMessage = "This property isn't in development";

    [Test(Description = "Ensure property periods are created correctly")]
    public void PropertyInDevelopment_AllowsDevelopmentToEndToday()
    {
        // Arrange
        var developmentEndDate = DateTime.Today;

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate: null, isDevelopment: true);

        // Act

        property.EndDevelopment(developmentEndDate);

        // Assert

        Assert.That(property.PropertyPeriods, Has.Count.EqualTo(2));

        var developmentPropertyPeriod = property.PropertyPeriods.OfType<DevelopmentPropertyPeriod>().Single();
        var voidPropertyPeriod = property.PropertyPeriods.OfType<VoidPropertyPeriod>().Single();

        Assert.Multiple(() =>
        {
            Assert.That(voidPropertyPeriod.StartDate, Is.EqualTo(developmentEndDate.AddDays(1)));
            Assert.That(voidPropertyPeriod.EndDate.HasValue, Is.False);

            // Developments always start on the same day of property creation and as we're ending today the date will also be equal to today.
            Assert.That(developmentPropertyPeriod.StartDate, Is.EqualTo(DateTime.Today));
            Assert.That(developmentPropertyPeriod.EndDate, Is.EqualTo(DateTime.Today));
        });

        Assert.That(property.ValidatePropertyPeriods(), Is.True);
    }

    [Test(Description = "Ensure property periods are created correctly")]
    public void VoidProperty_ThrowsExceptionOnEndDevelopmentAttempt()
    {
        // Arrange
        var developmentEndDate = DateTime.Today;
        var voidStartDate = developmentEndDate.AddDays(-14);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        // Act

        var exception = Assert.Throws<PropertyPeriodViolation>(() => property.EndDevelopment(developmentEndDate));

        // Assert

        Assert.That(exception.Message, Is.EqualTo(PropertyIsNotInDevelopmentMessage));

        Assert.That(property.ValidatePropertyPeriods(), Is.True);
    }


    [Test(Description = "Ensure property periods are created correctly")]
    public void OccupiedProperty_ThrowsExceptionOnEndDevelopmentAttempt()
    {
        // Arrange
        var developmentEndDate = DateTime.Today;
        var voidStartDate = developmentEndDate.AddDays(-14);
        var occupancyStartDate = voidStartDate.AddDays(7);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);
        property.StartOccupancy(occupancyStartDate, "ALB03-001");

        // Act

        var exception = Assert.Throws<PropertyPeriodViolation>(() => property.EndDevelopment(developmentEndDate));

        // Assert

        Assert.That(exception.Message, Is.EqualTo(PropertyIsNotInDevelopmentMessage));

        Assert.That(property.ValidatePropertyPeriods(), Is.True);
    }
}
