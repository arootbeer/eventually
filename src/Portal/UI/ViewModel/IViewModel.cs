using System;

namespace Eventually.Portal.UI.ViewModel
{
    public interface IViewModel
    {
        Guid Id { get; set; }

        int Version { get; set; }
    }
}