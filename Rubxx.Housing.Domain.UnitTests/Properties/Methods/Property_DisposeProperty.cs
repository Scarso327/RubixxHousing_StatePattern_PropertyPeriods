using Rubixx.Housing.Domain.Common.Exceptions;
using Rubixx.Housing.Domain.Properties.Entities;
using Rubixx.Housing.Domain.Properties.Entities.Periods;
using Rubixx.Housing.Domain.Properties.Exceptions;

namespace Rubxx.Housing.Domain.UnitTests.Properties.Methods;

[TestFixture(TestOf = typeof(Property))]
internal class Property_DisposeProperty
{
    public const string ExpectedActiveOccupancyDisposalExceptionMessage = "This property has an active occupancy so can't be disposed";

    [Test(Description = "Ensure property periods are created correctly")]
    public void PropertyWithVoidPeriodAWeekAgo_AllowsDisposalToday()
    {
        // Arrange
        var disposalDate = DateTime.Today;
        var voidStartDate = disposalDate.AddDays(-14);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        // Act

        property.DisposeProperty(disposalDate);

        // Assert

        Assert.That(property.PropertyPeriods, Has.Count.EqualTo(2));

        var voidPropertyPeriod = property.PropertyPeriods.OfType<VoidPropertyPeriod>().Single();
        var disposedPropertyPeriod = property.PropertyPeriods.OfType<DisposedPropertyPeriod>().Single();

        Assert.Multiple(() =>
        {
            Assert.That(voidPropertyPeriod.EndDate, Is.EqualTo(disposalDate.AddDays(-1)));

            Assert.That(disposedPropertyPeriod.StartDate, Is.EqualTo(disposalDate));
            Assert.That(disposedPropertyPeriod.EndDate.HasValue, Is.False);
        });
    }

    [Test(Description = "Ensure property periods are created correctly")]
    public void OccupiedProperty_ThrowsExceptionOnDisposalAttempt()
    {
        // Arrange
        var disposalDate = DateTime.Today;
        var voidStartDate = disposalDate.AddDays(-14);
        var occupancyStartDate = voidStartDate.AddDays(7);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);
        property.StartOccupancy(occupancyStartDate, "ALB03-001");

        // Act

        var exception = Assert.Throws<PropertyPeriodViolation>(() => property.DisposeProperty(disposalDate));

        // Assert

        Assert.That(exception.Message, Is.EqualTo(ExpectedActiveOccupancyDisposalExceptionMessage));
    }

    [Test(Description = "Ensure property periods are created correctly")]
    public void OccupiedProperty_ThrowsExceptionOnDisposalAttemptWithDateOverPreviousPeriod()
    {
        // Arrange
        var voidStartDate = DateTime.Today;
        var disposalDate = voidStartDate.AddDays(3); // This will cause the disposal date to be over the void period, not occupied
        var occupancyStartDate = voidStartDate.AddDays(7);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);
        property.StartOccupancy(occupancyStartDate, "ALB03-001");

        // Act

        var exception = Assert.Throws<PropertyPeriodViolation>(() => property.DisposeProperty(disposalDate));

        // Assert

        Assert.That(exception.Message, Is.EqualTo(ExpectedActiveOccupancyDisposalExceptionMessage));
    }

    [Test(Description = "Ensure property periods are created correctly")]
    public void PropertyWithVoidPeriodAWeekAgo_ThrowsExceptionIfDisposalDateIsInPast()
    {
        // Arrange
        var disposalDate = DateTime.Today.AddDays(-2);
        var voidStartDate = disposalDate.AddDays(-14);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        // Act

        var exception = Assert.Throws<DateInPast>(() => property.DisposeProperty(disposalDate));

        // Assert

        Assert.That(exception.Message, Is.EqualTo($"{disposalDate.ToShortDateString()} is in the past"));
    }
}
