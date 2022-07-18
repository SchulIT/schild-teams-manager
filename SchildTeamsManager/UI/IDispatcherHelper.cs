using System;

namespace SchildTeamsManager.UI
{
    public interface IDispatcherHelper
    {
        void Initialize();

        void InvokeOnUiThread(Action action);
    }
}
