using System;

namespace SchildTeamsManager.Service.MicrosoftGraph
{
    public class AuthenticationStateChangedEventArgs : EventArgs
    {
        public bool IsAuthenticated { get; private set; }

        public AuthenticationStateChangedEventArgs( bool isAuthenticated)
        {
            IsAuthenticated = isAuthenticated;
        }
    }
}
