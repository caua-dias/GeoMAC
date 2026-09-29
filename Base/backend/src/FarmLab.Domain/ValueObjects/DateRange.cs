using System;

namespace FarmLab.Domain.ValueObjects
{
    public class DateRange
    {
        public DateTime Start { get; init; }
        public DateTime End { get; init; }

        public DateRange(DateTime start, DateTime end)
        {
            if (end < start)
                throw new ArgumentException("End date must be after start date.");
            Start = start;
            End = end;
        }
    }
}
