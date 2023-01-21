using Autofac;
using Microsoft.Toolkit.Mvvm.Messaging;
using SchildTeamsManager.Service.MicrosoftGraph;
using SchildTeamsManager.Service.Schild;
using SchildTeamsManager.Service.Teams;
using SchildTeamsManager.Settings;
using SchildTeamsManager.UI;
using SchildTeamsManager.UI.Dialog;
using SchulIT.SchildExport;

namespace SchildTeamsManager.ViewModel
{
    public class ViewModelLocator
    {
        private static readonly IContainer container;

        static ViewModelLocator()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<DispatcherHelper>().As<IDispatcherHelper>().SingleInstance().OnActivated(x => x.Instance.Initialize());
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<MicrosoftGraph>().As<IMicrosoftGraph>().SingleInstance();
            builder.RegisterType<DialogHelper>().As<IDialogHelper>().SingleInstance();
            builder.RegisterType<JsonSettingsManager>().As<ISettingsManager>().SingleInstance();
            builder.RegisterType<Exporter>().As<IExporter>().SingleInstance();
            builder.RegisterType<LegacyTeamDataResolver>().As<ITeamDataResolver>().SingleInstance();
            builder.RegisterType<DefaultNameResolver>().As<INameResolver>().SingleInstance();

            builder.RegisterType<SplashScreenViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<TeamsViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<GradesViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<TuitionsViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<SettingsViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<GraphViewModel>().AsSelf().SingleInstance();
            builder.RegisterType<AboutViewModel>().AsSelf().SingleInstance();

            container = builder.Build();
        }

        public IMessenger Messenger { get { return container.Resolve<IMessenger>(); } }

        public IMicrosoftGraph MicrosoftGraph { get { return container.Resolve<IMicrosoftGraph>(); } }

        public SplashScreenViewModel SpashScreen { get { return container.Resolve<SplashScreenViewModel>(); } }

        public TeamsViewModel Teams { get { return container.Resolve<TeamsViewModel>();} }

        public GradesViewModel Grades { get { return container.Resolve<GradesViewModel>(); } }

        public TuitionsViewModel Tuitions { get { return container.Resolve<TuitionsViewModel>(); } }

        public SettingsViewModel Settings { get { return container.Resolve<SettingsViewModel>(); } }

        public GraphViewModel Graph { get { return container.Resolve<GraphViewModel>(); } }

        public AboutViewModel About { get { return container.Resolve<AboutViewModel>(); } }
    }
}
