using System;

namespace MedReminder.Services {
    public class DateTimeService {
        public virtual DateTime Now => DateTime.Now;
        public virtual DateTime UtcNow => DateTime.UtcNow;
    }
}