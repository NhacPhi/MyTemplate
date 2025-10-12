using System;

namespace Core.Scope
{
    public interface IPreload
    {
        public bool IsLoadDone();
        public Action OnLoadDone { get; set; }
    }
}

