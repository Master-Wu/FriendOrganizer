using FriendOrganizer.UI.Data.Repositories;
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
        private IFriendRepository _friendRepository;
        private IEventAggregator _eventAggregator;
        private FriendWrapper _friend;
        private bool _hasChanges;
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

        public bool HasChanges
        {
            get { return _hasChanges; }
            set
            {
                // Execute the logic only if the property has changed
                if (value != _hasChanges)
                {
                    _hasChanges = value;
                    OnPropertyChanged();
                    ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
                }
            }
        }


        // Don't need the setter because the property is initialized directly on the constructor
        public ICommand SaveCommand { get; }

        #endregion

        #region CONSTRUCTOR
        public FriendDetailViewModel(IFriendRepository friendRepository, IEventAggregator eventAggregator)
        {
            _friendRepository = friendRepository;
            _eventAggregator = eventAggregator;

            SaveCommand = new DelegateCommand(OnSaveExcute, OnSaveCanExecute);
        }

        #endregion

        #region METHODS
        private async void OnSaveExcute()
        {
            await _friendRepository.SaveAsync();

            HasChanges = _friendRepository.HasChanges();

            // Raise event to update navigation 
            _eventAggregator.GetEvent<AfterFriendSaveEvent>().Publish(new AfterFriendSaveEventArgs
            {
                Id = Friend.Id,
                DisplayMember = $"{Friend.FirstName} {Friend.LastName}"
            });

        }

        private bool OnSaveCanExecute()
        {

            return null != Friend && !Friend.HasErrors && HasChanges;
        }



        public async Task LoadAsync(int friendId)
        {
            var friend = await _friendRepository.GetByIdAsync(friendId);

            Friend = new FriendWrapper(friend);

            Friend.PropertyChanged += (s, e) =>
              {
                  if (!HasChanges)
                      HasChanges = _friendRepository.HasChanges();

                  if (e.PropertyName == nameof(Friend.HasErrors))
                      ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
              };

            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }
        #endregion
    }
}
