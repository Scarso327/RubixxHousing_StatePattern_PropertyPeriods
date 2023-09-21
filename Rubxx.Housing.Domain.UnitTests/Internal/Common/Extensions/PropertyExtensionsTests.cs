using Rubixx.Housing.Domain.Properties.Entities;
using Rubixx.Housing.Domain.Properties.Entities.Periods;

namespace Rubxx.Housing.Domain.UnitTests.Internal.Common.Extensions;

internal class PropertyExtensionsTests
{
    [Test]
    public void PropertyWithGapsInPeriods_ThrowsSequenceError()
    {
        // Arrange
        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate: DateTime.Today, isDevelopment: false);
        
        // Add a development period with a day gap between it and the void
        property.AddPropertyPeriod(new DevelopmentPropertyPeriod(property, startDate: DateTime.Today.AddDays(-7), endDate: DateTime.Today.AddDays(-2)));

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => property.ValidatePropertyPeriods());

        // Assert

        // We expect this error as the property period will attempt to call Linq Single to get the period before or after itself for validation purposes
        Assert.That(exception.Message, Is.EqualTo("Sequence contains no matching element"));
    }

    [Test]
    public void PropertyWithOverlappingPeriods_ReturnsFalse()
    {
        // Arrange
        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate: DateTime.Today, isDevelopment: false);

        // Just overlap a void period to test
        property.AddPropertyPeriod(new DevelopmentPropertyPeriod(property, startDate: DateTime.Today.AddDays(-7), endDate: DateTime.Today.AddDays(-1)));
        property.AddPropertyPeriod(new VoidPropertyPeriod(property, startDate: DateTime.Today.AddDays(-4), endDate: DateTime.Today.AddDays(-3)));

        // Act
        var isValid = property.ValidatePropertyPeriods();

        // Assert
        Assert.That(isValid, Is.False);
    }
}
