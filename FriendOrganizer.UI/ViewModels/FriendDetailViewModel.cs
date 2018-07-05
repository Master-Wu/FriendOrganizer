using FriendOrganizer.UI.Data;
using FriendOrganizer.UI.Events;
using FriendOrganizer.UI.Wrappers;
using Prism.Commands;
using Prism.Events;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FriendOrganizer.UI.ViewModels
{
    public class FriendDetailViewModel : ViewModelBase, IFriendDetailViewModel
    {
        #region PRIVATE MEMBERS
        private IFriendDataService _dataService;
        private IEventAggregator _eventAggregator;
        private FriendWrapper _friend;
        #endregion

        #region PUBLIC PROPERTIES

        public FriendWrapper Friend
        {
            get => _friend;
            private set
            {
                _friend = value;
                OnPropertyChanged();
            }

        }

        // Don't need the setter because the property is initialized directly on the constructor
        public ICommand SaveCommand { get; }

        #endregion

        #region CONSTRUCTOR
        public FriendDetailViewModel(IFriendDataService dataService, IEventAggregator eventAggregator)
        {
            _dataService = dataService;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OpenFriendDetailViewEvent>()
                .Subscribe(OnOpenFriendDetailView);

            SaveCommand = new DelegateCommand(OnSaveExcute, OnSaveCanExecute);
        }

        #endregion

        #region METHODS
        private async void OnSaveExcute()
        {
            await _dataService.SaveAsync(Friend.Model);

            // Raise event to update navigation 
            _eventAggregator.GetEvent<AfterFriendSaveEvent>().Publish(new AfterFriendSaveEventArgs
            {
                Id = Friend.Id,
                DisplayMember = $"{Friend.FirstName} {Friend.LastName}"
            });

        }

        private bool OnSaveCanExecute()
        {
            // TODO: Check in addition if friend has changes
            return null != Friend && !Friend.HasErrors;
        }

        private async void OnOpenFriendDetailView(int friendId)
        {
            await LoadAsync(friendId);
        }

        public async Task LoadAsync(int friendId)
        {
            var friend = await _dataService.GetByIdAsync(friendId);

            Friend = new FriendWrapper(friend);

            Friend.PropertyChanged += (s, e) =>
              {
                  if (e.PropertyName == nameof(Friend.HasErrors))
                      ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();


              };

            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }
        #endregion
    }
}
