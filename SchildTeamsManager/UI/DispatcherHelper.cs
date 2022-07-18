using System;
using System.Windows.Threading;

namespace SchildTeamsManager.UI
{
    public class DispatcherHelper : IDispatcherHelper
    {
        private Dispatcher disptacher;


        public void Initialize()
        {
            disptacher = Dispatcher.CurrentDispatcher;
        }

        public void InvokeOnUiThread(Action action)
        {
            disptacher?.Invoke(action);
        }
    }
}
