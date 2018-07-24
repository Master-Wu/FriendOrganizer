using Autofac;
using FriendOrganizer.Data;
using FriendOrganizer.UI.Data.Lookups;
using FriendOrganizer.UI.Data.Repositories;
using FriendOrganizer.UI.ViewModels;
using FriendOrganizer.UI.Views.Services;
using Prism.Events;

namespace FriendOrganizer.UI.Startup
{
    /// <summary>
    /// Inversion of Control 
    /// </summary>
    public class Bootstrapper
    {
        public IContainer Bootstrap()
        {
            var builder = new ContainerBuilder();

            // When an interface is required, it creates an object that implements it.
            builder.RegisterType<FriendRepository>().As<IFriendRepository>();
            builder.RegisterType<MeetingRepository>().As<IMeetingRepository>();
            builder.RegisterType<ProgrammingLanguageRepository>().As<IProgrammingLanguageRepository>();
            builder.RegisterType<NavigationViewModel>().As<INavigationViewModel>();
            builder.RegisterType<MessageDialogService>().As<IMessageDialogService>();

            // Keyed indexes for more simple access on main ViewModel as it has to redirect to multiple views
            builder.RegisterType<FriendDetailViewModel>().Keyed<IDetailViewModel>(nameof(FriendDetailViewModel));
            builder.RegisterType<MeetingDetailViewModel>().Keyed<IDetailViewModel>(nameof(MeetingDetailViewModel));
            builder.RegisterType<ProgrammingLanguageDetailViewModel>().Keyed<IDetailViewModel>(nameof(ProgrammingLanguageDetailViewModel));

            // Register a single instance of the Prism EventAggregator to enable communication between ViewModels
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            // Register a type that implements multiple interfaces
            builder.RegisterType<LookupDataService>().AsImplementedInterfaces();

            // Also can register concrete types that don't implement an interface
            builder.RegisterType<MainViewModel>().AsSelf();
            builder.RegisterType<MainWindow>().AsSelf();
            builder.RegisterType<FriendOrganizerDbContext>().AsSelf();


            return builder.Build();
        }
    }
}
