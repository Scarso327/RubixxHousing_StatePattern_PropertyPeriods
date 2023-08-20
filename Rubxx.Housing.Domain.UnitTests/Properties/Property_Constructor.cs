using Rubixx.Housing.Domain.Properties.Entities;
using Rubixx.Housing.Domain.Properties.Entities.Periods;

namespace Rubxx.Housing.Domain.UnitTests.Properties;

[TestFixture(TestOf = typeof(Property))]
internal class Property_Constructor
{
    [Test(Description = "Ensure property periods are created correctly")]
    public void IsDevelopment_HasOneDevelopmentPropertyPeriod()
    {
        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate: null, isDevelopment: true);

        Assert.That(property.PropertyPeriods, Has.Count.EqualTo(1));
        Assert.That(property.PropertyPeriods.Single(), Is.TypeOf<DevelopmentPropertyPeriod>());
    }

    [Test(Description = "Ensure property periods are created correctly")]
    public void VoidStartDateInFuture_CreatesDevelopmentPeriodBeforeVoid()
    {
        var voidStartDate = DateTime.Today.AddDays(1);

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        Assert.That(property.PropertyPeriods, Has.Count.EqualTo(2));

        Assert.Multiple(() =>
        {
            Assert.That(property.PropertyPeriods.Any(x => x is VoidPropertyPeriod));
            Assert.That(property.PropertyPeriods.Any(x => x is DevelopmentPropertyPeriod));

            Assert.That(property.PropertyPeriods.OfType<DevelopmentPropertyPeriod>().Single().StartDate, Is.EqualTo(voidStartDate.AddDays(-1)));
        });
    }

    [Test(Description = "Ensure property periods are created correctly")]
    public void VoidStartDate_IsSetOnVoidPeriod()
    {
        var voidStartDate = DateTime.Today;

        var property = new Property(uPRN: "ALB03", isLettablePropertyType: true, voidStartDate, isDevelopment: false);

        Assert.That(property.PropertyPeriods, Has.Count.EqualTo(1));
        Assert.That(property.PropertyPeriods.Single().StartDate, Is.EqualTo(voidStartDate));
    }
}
