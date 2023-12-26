using System;
using System.Collections.Generic;
using Eventually.Portal.UI.ViewModel;

namespace Eventually.Portal.UI.Areas.Counter.Data
{
    public class GlobalCounterState: IViewModel
    {
        public Guid Id { get; set; }
        
        public long Version { get; set; }
        
        public decimal Value { get; set; }

        public List<GlobalCounterStateChange> RecentHistory { get; set; } = new();
    }
}