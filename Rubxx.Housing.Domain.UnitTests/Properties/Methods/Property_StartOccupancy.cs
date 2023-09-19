using Rubixx.Housing.Domain.Properties.Entities;
using Rubixx.Housing.Domain.Properties.Entities.Periods;
using Rubixx.Housing.Domain.Properties.Exceptions;

namespace Rubxx.Housing.Domain.UnitTests.Properties.Methods;

[TestFixture(TestOf = typeof(Property))]
internal class Property_StartOccupancy
{
    [Test(Description = "Ensure property periods are created correctly")]
    public void PropertyWithVoidPeriodAWeekAgo_AllowsNewOccupancyToday()
    {
        var voidStartDate = DateTime.Today.AddDays(-7);
        var occupancyStartDate = DateTime.Today;

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        property.StartOccupancy(occupancyStartDate, "ALB03-001");

        Assert.That(property.PropertyPeriods, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(property.PropertyPeriods.Any(x => x is VoidPropertyPeriod));
            Assert.That(property.PropertyPeriods.Any(x => x is OccupiedPropertyPeriod));

            var voidPropertyPeriod = property.PropertyPeriods.OfType<VoidPropertyPeriod>().Single();

            Assert.That(voidPropertyPeriod.StartDate, Is.EqualTo(voidStartDate));
            Assert.That(voidPropertyPeriod.EndDate, Is.EqualTo(occupancyStartDate.AddDays(-1)));

            var occupiedPropertyPeriod = property.PropertyPeriods.OfType<OccupiedPropertyPeriod>().Single();

            Assert.That(occupiedPropertyPeriod.StartDate, Is.EqualTo(occupancyStartDate));

            Assert.That(occupiedPropertyPeriod.Occupancy, Is.Not.Null);
            Assert.That(occupiedPropertyPeriod.Occupancy.StartDate, Is.EqualTo(occupancyStartDate));
        });
    }

    [Test]
    public void UnlettableProperty_DoesNotAllowOccupanciesToBeStarted()
    {
        var originalVoidStartDate = DateTime.Today.AddDays(-14);
        var newOccupancyStartDate = DateTime.Today;

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: false, originalVoidStartDate, isDevelopment: false);

        var exception = Assert.Throws<PropertyPeriodViolation>(() => property.StartOccupancy(newOccupancyStartDate, "ALB03-001"));

        Assert.That(exception.Message, Is.EqualTo("This property is unlettable"));
    }

    [Test(Description = "Ensures currently occupied properties can't be let")]
    public void OccupiedProperty_DoesNotAllowOccupanciesToBeStarted()
    {
        var originalVoidStartDate = DateTime.Today.AddDays(-14);
        var originalOccupancyStartDate = DateTime.Today.AddDays(-7);
        var newOccupancyStartDate = DateTime.Today;

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, originalVoidStartDate, isDevelopment: false);

        property.StartOccupancy(originalOccupancyStartDate, "ALB03-001");

        var exception = Assert.Throws<PropertyPeriodViolation>(() => property.StartOccupancy(newOccupancyStartDate, "ALB03-002"));

        Assert.That(exception.Message, Is.EqualTo("This property has an occupancy on the date you've specified"));
    }

    [Test(Description = "Ensures back to back lets work")]
    public void VoidPropertyWithPreviousOccupancy_SupersedesCurrentVoidPeriodWithNewOccupancyDayAfterPreviousOccupancy()
    {
        var originalVoidStartDate = DateTime.Today.AddDays(-14);
        var originalOccupancyStartDate = DateTime.Today.AddDays(-7);
        var originalOccupancyEndDate = DateTime.Today.AddDays(-3);
        var newOccupancyStartDate = originalOccupancyEndDate.AddDays(1);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, originalVoidStartDate, isDevelopment: false);

        property.StartOccupancy(originalOccupancyStartDate, "ALB03-001");
        property.EndOccupancy(originalOccupancyEndDate);

        property.StartOccupancy(newOccupancyStartDate, "ALB03-002");

        Assert.That(property.PropertyPeriods, Has.Count.EqualTo(4));

        Assert.Multiple(() =>
        {
            Assert.That(property.PropertyPeriods.OfType<VoidPropertyPeriod>().Count(), Is.EqualTo(2));
            Assert.That(property.PropertyPeriods.OfType<OccupiedPropertyPeriod>().Count(), Is.EqualTo(2));

            var supercededVoid = property.PropertyPeriods.OfType<VoidPropertyPeriod>().SingleOrDefault(e => e.OccupiedPropertyPeriod is not null);

            Assert.That(supercededVoid, Is.Not.Null);
            Assert.That(supercededVoid!.StartDate, Is.EqualTo(newOccupancyStartDate));
            Assert.That(supercededVoid!.EndDate, Is.EqualTo(newOccupancyStartDate));
        });
    }

    [Test(Description = "Ensures currently occupied properties can't be let")]
    public void VoidPropertyWithPreviousOccupancy_IfNewOccupancyStartDateOverlapsPreviousOccupancyThrowError()
    {
        var originalVoidStartDate = DateTime.Today.AddDays(-14);
        var originalOccupancyStartDate = DateTime.Today.AddDays(-7);
        var originalOccupancyEndDate = DateTime.Today.AddDays(-3);
        var newOccupancyStartDate = originalOccupancyEndDate.AddDays(-1); // This start date would overlap with the previous occupancy

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, originalVoidStartDate, isDevelopment: false);

        property.StartOccupancy(originalOccupancyStartDate, "ALB03-001");
        property.EndOccupancy(originalOccupancyEndDate);

        var exception = Assert.Throws<PropertyPeriodViolation>(() => property.StartOccupancy(newOccupancyStartDate, "ALB03-002"));

        Assert.That(exception.Message, Is.EqualTo("This property has an occupancy on the date you've specified"));
    }

    [Test(Description = "Ensures currently occupied properties can't be let")]
    public void VoidProperty_NewOccupancyBeforeOriginalVoidStartThrowsError()
    {
        var voidStartDate = DateTime.Today;
        var occupancyStartDate = DateTime.Today.AddDays(-7);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        var exception = Assert.Throws<InvalidOperationException>(() => property.StartOccupancy(occupancyStartDate, "ALB03-001"));

        Assert.That(exception.Message, Is.EqualTo("Sequence contains no matching element"));
    }

    [Test]
    public void VoidPropertyWithPreviousOccupancy_DoesNotAllowOccupancyToBeStartedBeforeTheExistingPreviousOne()
    {
        // Arrange
        var voidStartDate = DateTime.Today.AddDays(-14);
        var previousOccupancyStartDate = DateTime.Today.AddDays(-7);
        var previousOccupancyEndDate = previousOccupancyStartDate.AddDays(5);

        var newOccupancyStartDate = voidStartDate.AddDays(1);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        property.StartOccupancy(previousOccupancyStartDate, "ALB03-001");
        property.EndOccupancy(previousOccupancyEndDate);

        // Act

        var exception = Assert.Throws<PropertyPeriodViolation>(() => property.StartOccupancy(newOccupancyStartDate, "ALB03-002"));

        // Assert

        Assert.That(exception.Message, Is.EqualTo("Can't start an occupancy over a void period that has ended as it'll have future occupied or disposed periods"));
    }

    [Test]
    public void DisposedProperty_DoesNotAllowOccupancyToBeStarted()
    {
        // Arrange
        var disposalDate = DateTime.Today;
        var voidStartDate = disposalDate.AddDays(-14);

        var newOccupancyStartDate = disposalDate.AddDays(1);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);
        property.DisposeProperty(disposalDate);

        // Act

        var exception = Assert.Throws<PropertyPeriodViolation>(() => property.StartOccupancy(newOccupancyStartDate, "ALB03-001"));

        // Assert

        Assert.That(exception.Message, Is.EqualTo("An occupancy cannot be started once a property has been disposed"));
    }

    [Test]
    public void DevelopmentProperty_DoesNotAllowOccupancyToBeStarted()
    {
        // Arrange
        var developmentDate = DateTime.Today;
        var newOccupancyStartDate = developmentDate.AddDays(1);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate: null, isDevelopment: true);

        // Act

        var exception = Assert.Throws<PropertyPeriodViolation>(() => property.StartOccupancy(newOccupancyStartDate, "ALB03-001"));

        // Assert

        Assert.That(exception.Message, Is.EqualTo("A property still in development can't be let to someone"));
    }
}
