using CommunityToolkit.Mvvm.ComponentModel;
using SchildTeamsManager.Service.MicrosoftGraph;

namespace SchildTeamsManager.ViewModel
{
    public class GraphViewModel : ObservableRecipient
    {
        #region Properties
        private bool isAuthenticated;

        public bool IsAuthenticated
        {
            get { return isAuthenticated; }
            set { SetProperty(ref isAuthenticated, value); }
        }

        private string tenantName;

        public string TenantName
        {
            get { return tenantName; }
            set { SetProperty(ref tenantName, value); }
        }
        #endregion

        public GraphViewModel(IMicrosoftGraph graph)
        {
            graph.AuthenticationStateChanged += OnAuthenticationStateChanged;

            OnAuthenticationStateChanged(graph, new AuthenticationStateChangedEventArgs(graph.IsAuthenticated));
        }

        private void OnAuthenticationStateChanged(IMicrosoftGraph graph, AuthenticationStateChangedEventArgs eventArgs)
        {
            IsAuthenticated = eventArgs.IsAuthenticated;
            TenantName = IsAuthenticated ? graph.TenantName : null;
        }
    }
}
