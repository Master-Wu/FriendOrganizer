using FriendOrganizer.Model;
using FriendOrganizer.UI.Data.Lookups;
using FriendOrganizer.UI.Data.Repositories;
using FriendOrganizer.UI.Events;
using FriendOrganizer.UI.Views.Services;
using FriendOrganizer.UI.Wrappers;
using Prism.Commands;
using Prism.Events;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FriendOrganizer.UI.ViewModels
{
    public class

        FriendDetailViewModel : ViewModelBase, IFriendDetailViewModel
    {
        #region PRIVATE MEMBERS
        private IFriendRepository _friendRepository;
        private IEventAggregator _eventAggregator;
        private IMessageDialogService _messageDialogService;
        private IProgrammingLanguageLookupDataService _programmingLanguageLookupDataService;
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

        public ICommand DeleteCommand { get; }

        public ObservableCollection<LookupItem> ProgrammingLanguages { get; }

        #endregion

        #region CONSTRUCTOR
        public FriendDetailViewModel(IFriendRepository friendRepository,
            IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService,
            IProgrammingLanguageLookupDataService programmingLanguageLookupDataService)
        {
            _friendRepository = friendRepository;
            _eventAggregator = eventAggregator;
            _messageDialogService = messageDialogService;
            _programmingLanguageLookupDataService = programmingLanguageLookupDataService;

            SaveCommand = new DelegateCommand(OnSaveExcute, OnSaveCanExecute);
            DeleteCommand = new DelegateCommand(OnDeleteExcute);

            ProgrammingLanguages = new ObservableCollection<LookupItem>();
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


        private async void OnDeleteExcute()
        {
            var result = _messageDialogService.ShowOkCancelDialog($"¿Eliminar a {Friend.FirstName} {Friend.LastName}", "Pregunta");

            if (result == MessageDialogResult.OK)
            {

                // Delete friend and save changes
                _friendRepository.Remove(Friend.Model);
                await _friendRepository.SaveAsync();

                // Inform navigation via event
                _eventAggregator.GetEvent<AfterFriendDeletedEvent>().Publish(Friend.Id);
            }

        }

        public async Task LoadAsync(int? friendId)
        {
            var friend = friendId.HasValue ? await _friendRepository.GetByIdAsync(friendId.Value) : CreateNewFriend();

            InitializeFriend(friend);

            await LoadPropgrammingLanguagesLookupAsyn();
        }

        private void InitializeFriend(Friend friend)
        {
            Friend = new FriendWrapper(friend);

            Friend.PropertyChanged += (s, e) =>
            {
                if (!HasChanges)
                    HasChanges = _friendRepository.HasChanges();

                if (e.PropertyName == nameof(Friend.HasErrors))
                    ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            };

            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();

            // Trick to trigger the validation
            if (Friend.Id == 0)
                Friend.FirstName = "";
        }

        private async Task LoadPropgrammingLanguagesLookupAsyn()
        {
            ProgrammingLanguages.Clear();
            // Add the null lookup item before getting data from database
            ProgrammingLanguages.Add(new NullLookupItem());
            var lookup = await _programmingLanguageLookupDataService.GetProgrammingLanguageLookupAsync();
            foreach (var item in lookup)
            {
                ProgrammingLanguages.Add(item);
            }
        }

        private Friend CreateNewFriend()
        {
            var friend = new Friend();
            _friendRepository.Add(friend);
            return friend;
        }
        #endregion
    }
}
