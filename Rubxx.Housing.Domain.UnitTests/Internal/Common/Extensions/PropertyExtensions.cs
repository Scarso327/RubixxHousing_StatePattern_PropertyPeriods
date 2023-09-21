using Rubixx.Housing.Domain.Properties.Entities;
using Rubixx.Housing.Domain.Properties.Entities.Periods;

namespace Rubxx.Housing.Domain.UnitTests.Internal.Common.Extensions;

internal static class PropertyExtensions
{
    /// <summary>
    /// Validates the Property Periods against a property have no gaps, only one without an end date and no overlapping unless superceded
    /// </summary>
    public static bool ValidatePropertyPeriods(this Property property)
    {
        var propertyPeriods = property
            .PropertyPeriodsWithoutSupercededVoids
            .OrderBy(e => e.StartDate);

        var doesNotHaveASinglePeriodWithoutEndDate = propertyPeriods.Count(e => !e.EndDate.HasValue) != 1;
        if (doesNotHaveASinglePeriodWithoutEndDate) return false;

        var hasOverlappingPeriods = propertyPeriods.Any(e => propertyPeriods.Any(x => e != x && e.StartDate <= x.EndDate && e.EndDate >= x.StartDate));
        if (hasOverlappingPeriods) return false;

        var hasPeriodsWithOutOfOrderDates = propertyPeriods.Any(e => e.StartDate > e.EndDate);
        if (hasPeriodsWithOutOfOrderDates) return false;

        return propertyPeriods.First().ValidatePropertyPeriod();
    }

    /// <summary>
    /// Validates that we expect to have a period after if we have an end date etc
    /// </summary>
    private static bool ValidatePropertyPeriod(this BasePropertyPeriod propertyPeriod)
    {
        var periodAfter = propertyPeriod.PeriodAfterThisOne;
        return !propertyPeriod.EndDate.HasValue || periodAfter is not null && periodAfter.ValidatePropertyPeriod();
    }
}
