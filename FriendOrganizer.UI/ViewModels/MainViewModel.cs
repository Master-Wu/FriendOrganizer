using FriendOrganizer.UI.Events;
using FriendOrganizer.UI.Views.Services;
using Prism.Commands;
using Prism.Events;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FriendOrganizer.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private IMessageDialogService _messageDialogService;
        private Func<IFriendDetailViewModel> _friendDetailViewModelCreator;

        private IEventAggregator _eventAggregator;

        private IFriendDetailViewModel _friendDetailViewModel;

        public IFriendDetailViewModel FriendDetailViewModel
        {
            get => _friendDetailViewModel;
            private set
            {
                _friendDetailViewModel = value;
                OnPropertyChanged();
            }
        }

        public INavigationViewModel NavigationViewModel { get; }

        public ICommand CreateNewFriendCommand { get; }


        public MainViewModel(INavigationViewModel navigationViewModel, Func<IFriendDetailViewModel> friendDetailViewModelCreator,
            IEventAggregator eventAggregator, IMessageDialogService messageDialogService)
        {
            _messageDialogService = messageDialogService;
            _friendDetailViewModelCreator = friendDetailViewModelCreator;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OpenFriendDetailViewEvent>()
                .Subscribe(OnOpenFriendDetailView);
            CreateNewFriendCommand = new DelegateCommand(OnCreateNewFriendExcute);
            NavigationViewModel = navigationViewModel;
        }

        private void OnCreateNewFriendExcute()
        {
            OnOpenFriendDetailView(null);
        }

        public async Task LoadAsync()
        {
            await NavigationViewModel.LoadAsync();
        }

        private async void OnOpenFriendDetailView(int? friendId)
        {
            // Block navigation if the user has made changes to a friend
            if (null != FriendDetailViewModel && FriendDetailViewModel.HasChanges)
            {
                var result = _messageDialogService.ShowOkCancelDialog("Se realizaron cambios. Desea salir?", "Pregunta");
                if (result == MessageDialogResult.Cancel) return;
            }

            // Navigate
            FriendDetailViewModel = _friendDetailViewModelCreator();
            await FriendDetailViewModel.LoadAsync(friendId);
        }



    }
}
