using FriendOrganizer.Model;
using FriendOrganizer.UI.Data.Lookups;
using FriendOrganizer.UI.Data.Repositories;
using FriendOrganizer.UI.Events;
using FriendOrganizer.UI.Views.Services;
using FriendOrganizer.UI.Wrappers;
using Prism.Commands;
using Prism.Events;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FriendOrganizer.UI.ViewModels
{
    public class

        FriendDetailViewModel : DetailViewModelBase, IFriendDetailViewModel
    {
        #region PRIVATE MEMBERS
        private IFriendRepository _friendRepository;

        private IProgrammingLanguageLookupDataService _programmingLanguageLookupDataService;
        private FriendWrapper _friend;
        private FriendPhoneNumberWrapper _selectedPhoneNumber;

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

        public FriendPhoneNumberWrapper SelectedPhoneNumber
        {
            get => _selectedPhoneNumber;
            set
            {
                _selectedPhoneNumber = value;
                OnPropertyChanged();
                ((DelegateCommand)RemovePhoneNumberCommand).RaiseCanExecuteChanged();
            }
        }



        public ICommand AddPhoneNumberCommand { get; }

        public ICommand RemovePhoneNumberCommand { get; }


        public ObservableCollection<LookupItem> ProgrammingLanguages { get; }

        public ObservableCollection<FriendPhoneNumberWrapper> PhoneNumbers { get; }

        #endregion

        #region CONSTRUCTOR
        public FriendDetailViewModel(IFriendRepository friendRepository,
            IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService,
            IProgrammingLanguageLookupDataService programmingLanguageLookupDataService) : base(eventAggregator, messageDialogService)
        {
            _friendRepository = friendRepository;

            _programmingLanguageLookupDataService = programmingLanguageLookupDataService;
            eventAggregator.GetEvent<AfterCollectionSavedEvent>().Subscribe(AfterCollectionSaved);

            AddPhoneNumberCommand = new DelegateCommand(OnAddPhoneNumberExecute);
            RemovePhoneNumberCommand = new DelegateCommand(OnRemovePhoneNumberExecute, OnRemovePhoneNumberCanExceute);

            ProgrammingLanguages = new ObservableCollection<LookupItem>();
            PhoneNumbers = new ObservableCollection<FriendPhoneNumberWrapper>();
        }



        #endregion

        #region METHODS

        private async void AfterCollectionSaved(AfterCollectionSavedEventArgs args)
        {
            if (args.ViewModelName == nameof(ProgrammingLanguageDetailViewModel))
                await LoadProgrammingLanguagesLookupAsync();
        }


        private void OnRemovePhoneNumberExecute()
        {
            // Remove property changed event handler
            SelectedPhoneNumber.PropertyChanged -= FriendPhoneNumberWrapper_PropertyChanged;

            // Remove number from model
            _friendRepository.RemovePhoneNumber(SelectedPhoneNumber.Model);

            // Remove number from collection
            PhoneNumbers.Remove(SelectedPhoneNumber);

            // Set selected number property to null
            SelectedPhoneNumber = null;

            // Update HasChanges property by calling the HasChanges method fron the repository
            HasChanges = _friendRepository.HasChanges();

            // Raise can execute so the user can save changes
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private bool OnRemovePhoneNumberCanExceute()
        {
            return SelectedPhoneNumber != null;
        }

        private void OnAddPhoneNumberExecute()
        {
            // Initialize a new number
            var newNumber = new FriendPhoneNumberWrapper(new FriendPhoneNumber());

            // Add property changed event handler
            newNumber.PropertyChanged += FriendPhoneNumberWrapper_PropertyChanged;

            // Add new number to collection
            PhoneNumbers.Add(newNumber);

            // Add the number on the model to the collections of numbers of the friend model
            Friend.Model.PhoneNumbers.Add(newNumber.Model);

            newNumber.Number = ""; // Trigger validation
        }


        public override async Task LoadAsync(int friendId)
        {
            var friend = friendId > 0 ? await _friendRepository.GetByIdAsync(friendId) : CreateNewFriend();

            Id = friendId;

            InitializeFriend(friend);

            InitializeFriendPhoneNumbers(friend.PhoneNumbers);

            await LoadProgrammingLanguagesLookupAsync();
        }

        private void InitializeFriendPhoneNumbers(ICollection<FriendPhoneNumber> phoneNumbers)
        {
            // Remove the event handler for the property changed event
            foreach (var wrapper in PhoneNumbers)
            {
                wrapper.PropertyChanged -= FriendPhoneNumberWrapper_PropertyChanged;
            }

            // Clear collection
            PhoneNumbers.Clear();

            //Wrap models, add to collection and add event handler for property changed
            foreach (var item in phoneNumbers)
            {
                var wrapper = new FriendPhoneNumberWrapper(item);
                PhoneNumbers.Add(wrapper);
                wrapper.PropertyChanged += FriendPhoneNumberWrapper_PropertyChanged;

            }

        }

        private void FriendPhoneNumberWrapper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!HasChanges)
            {
                HasChanges = _friendRepository.HasChanges();
            }
            if (e.PropertyName == nameof(FriendPhoneNumberWrapper.HasErrors))
            {
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
            }
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

                if (e.PropertyName == nameof(Friend.FirstName) || e.PropertyName == nameof(Friend.LastName))
                    SetTitle();
            };



            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();

            // Trick to trigger the validation
            if (Friend.Id == 0)
                Friend.FirstName = "";

            SetTitle();
        }

        private void SetTitle()
        {
            Title = $"{Friend.FirstName} {Friend.LastName}";
        }

        private async Task LoadProgrammingLanguagesLookupAsync()
        {
            // Clear collection
            ProgrammingLanguages.Clear();

            // Add the null lookup item before getting data from database
            ProgrammingLanguages.Add(new NullLookupItem());

            // Look for programming languages
            var lookup = await _programmingLanguageLookupDataService.GetProgrammingLanguageLookupAsync();
            foreach (var item in lookup)
            {
                // Add programming langugaes to the collection
                ProgrammingLanguages.Add(item);
            }
        }

        private Friend CreateNewFriend()
        {
            var friend = new Friend();
            _friendRepository.Add(friend);
            return friend;
        }

        protected async override void OnSaveExceute()
        {
            await SaveWithOptimisticConcurrencyAsync(_friendRepository.SaveAsync, () =>
                 {

                     HasChanges = _friendRepository.HasChanges();

                     Id = Friend.Id;

                     RaiseDetailSavedEvent(Friend.Id, $"{Friend.FirstName} {Friend.LastName}");

                 });
        }

        protected override async void OnDeleteExecute()
        {
            if (await _friendRepository.HasMeetingsAsync(Friend.Id))
            {
                await MessageDialogService.ShowInfoDialogAsync($"El contacto {Friend.FirstName} {Friend.LastName} no puede ser eliminado porque tiene reuniones.");
                return;
            }

            var result = await MessageDialogService.ShowOkCancelDialogAsync($"¿Eliminar a {Friend.FirstName} {Friend.LastName}?", "Pregunta");

            if (result == MessageDialogResult.OK)
            {

                // Delete friend and save changes
                _friendRepository.Remove(Friend.Model);
                await _friendRepository.SaveAsync();
                RaiseDetailDeletedEvent(Friend.Id);


            }
        }

        protected override bool OnSaveCanExcute()
        {
            return null != Friend
                && !Friend.HasErrors
                && PhoneNumbers.All(pn => !pn.HasErrors)
                && HasChanges;
        }
        #endregion
    }
}
