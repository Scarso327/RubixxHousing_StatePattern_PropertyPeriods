﻿using Rubixx.Housing.Domain.Occupancies.Entities;
using Rubixx.Housing.Domain.Properties.Exceptions;

namespace Rubixx.Housing.Domain.Properties.Entities.Periods;

public class DevelopmentPropertyPeriod : BasePropertyPeriod
{
    protected DevelopmentPropertyPeriod() : base() { }

    public DevelopmentPropertyPeriod(Property property, DateTime startDate, DateTime? endDate = null) : base(property, startDate, endDate) { }

    public override bool CanReviseStartDate => false;

    public override bool CanReviseEndDate => false;

    public override void EndDevelopment(DateTime endDate)
    {
        var voidPropertyPeriod = new VoidPropertyPeriod(Property, endDate.AddDays(1));
        Property.AddPropertyPeriod(voidPropertyPeriod);

        EndDate = endDate;
    }

    public override Occupancy StartOccupancy(DateTime occupancyStartDate, string uORN) => throw new PropertyPeriodViolation(this, "A property still in development can't be let to someone");
}
