using Autofac;
using FriendOrganizer.Data;
using FriendOrganizer.UI.Data;
using FriendOrganizer.UI.ViewModels;
using Prism.Events;

namespace FriendOrganizer.UI.Startup
{
    public class Bootstrapper
    {
        public IContainer Bootstrap()
        {
            var builder = new ContainerBuilder();

            // When an interface is required, it creates an object that implements it.
            builder.RegisterType<FriendDataService>().As<IFriendDataService>();
            builder.RegisterType<NavigationViewModel>().As<INavigationViewModel>();
            builder.RegisterType<FriendDetailViewModel>().As<IFriendDetailViewModel>();

            // Register a single instance of the Prism EventAggregator to enable communication between viewmodels
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
