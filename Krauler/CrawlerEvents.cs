using System;

namespace Krauler
{
    public abstract class CrawlerEvents
    {
        public event Action? OnInitializeEvent;
        public event Action? OnDispatchEvent;
        public event Action? OnDestroyEvent;

        protected abstract void OnInitialize();

        protected abstract void OnDispatch();

        protected abstract void OnDestroy();

        public void DispatchOnInitialize()
        {
            OnInitializeEvent?.Invoke();
            OnInitialize();
        }
        public void DispatchOnDispatch()
        {
            OnDispatchEvent?.Invoke();
            OnDispatch();
        }

        public void DispatchOnDestroy()
        {
            OnDestroyEvent?.Invoke();
            OnDestroy();
        }
    }
}
