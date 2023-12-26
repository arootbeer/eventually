using NodaTime;

namespace Eventually.Portal.UI.Areas.Counter.Data
{
    public class GlobalCounterStateChange
    {
        public long ChangeSequence { get; set; }

        public Instant Timestamp { get; set; }

        public string UserName { get; set; }

        public decimal PreviousValue { get; set; }
    }
}