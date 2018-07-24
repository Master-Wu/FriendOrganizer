using FriendOrganizer.Model;
using FriendOrganizer.UI.Data.Repositories;
using FriendOrganizer.UI.Events;
using FriendOrganizer.UI.Views.Services;
using FriendOrganizer.UI.Wrappers;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FriendOrganizer.UI.ViewModels
{
    public class MeetingDetailViewModel : DetailViewModelBase, IMeetingDetailViewModel
    {
        private MeetingWrapper _meeting;
        private IMeetingRepository _meetingRepository;
        private Friend _selectedAvailableFriend;
        private Friend _selectedAddedFriend;
        private List<Friend> _allFriends;

        public Friend SelectedAvailableFriend
        {
            get => _selectedAvailableFriend;
            set
            {
                _selectedAvailableFriend = value;
                OnPropertyChanged();
                ((DelegateCommand)AddFriendCommand).RaiseCanExecuteChanged();
            }
        }

        public Friend SelectedAddedFriend
        {
            get => _selectedAddedFriend;
            set
            {
                _selectedAddedFriend = value;
                OnPropertyChanged();
                ((DelegateCommand)RemoveFriendCommand).RaiseCanExecuteChanged();
            }
        }

        public MeetingWrapper Meeting
        {
            get => _meeting;
            private set
            {
                _meeting = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Friend> AvailableFriends { get; }
        public ObservableCollection<Friend> AddedFriends { get; }

        public ICommand AddFriendCommand { get; }
        public ICommand RemoveFriendCommand { get; }

        #region CONSTRUCTOR
        public MeetingDetailViewModel(IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService,
            IMeetingRepository meetingRepository) : base(eventAggregator, messageDialogService)
        {
            _meetingRepository = meetingRepository;

            eventAggregator.GetEvent<AfterDetailSavedEvent>().Subscribe(AfterDetailSaved);
            eventAggregator.GetEvent<AfterDetailDeletedEvent>().Subscribe(AfterDetailDeleted);

            AvailableFriends = new ObservableCollection<Friend>();
            AddedFriends = new ObservableCollection<Friend>();

            AddFriendCommand = new DelegateCommand(OnAddFriendExecute, OnAddFriendCanExecute);
            RemoveFriendCommand = new DelegateCommand(OnDeleteFriendExecute, OnDeleteFriendCanExecute);
        }

        #endregion

        private void OnDeleteFriendExecute()
        {
            var friendToRemove = SelectedAddedFriend;

            Meeting.Model.Friends.Remove(friendToRemove);
            AddedFriends.Remove(friendToRemove);
            AvailableFriends.Add(friendToRemove);
            HasChanges = _meetingRepository.HasChanges();
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private bool OnDeleteFriendCanExecute()
        {
            return SelectedAddedFriend != null;
        }

        private void OnAddFriendExecute()
        {
            var friendToAdd = SelectedAvailableFriend;

            Meeting.Model.Friends.Add(friendToAdd);
            AddedFriends.Add(friendToAdd);
            AvailableFriends.Remove(friendToAdd);
            HasChanges = _meetingRepository.HasChanges();
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private bool OnAddFriendCanExecute()
        {
            return SelectedAvailableFriend != null;
        }

        public override async Task LoadAsync(int meetingId)
        {
            var meeting = meetingId > 0 ?
                await _meetingRepository.GetByIdAsync(meetingId) :
                CreateNewMeeting();

            Id = meetingId;

            InitializeMeeting(meeting);

            // Load friends for the meeting
            _allFriends = await _meetingRepository.GetAllFriendsAsync();

            SetUpPickList();


        }

        private void SetUpPickList()
        {
            // Select IDs from the friends on the Meeting model and store in a list
            var meetingFriendsIds = Meeting.Model.Friends.Select(f => f.Id).ToList();

            // From all the friends, select those which are part of the meeting
            var addedFriends = _allFriends.Where(f => meetingFriendsIds.Contains(f.Id)).OrderBy(f => f.FirstName);

            // Select all friends except those which are in the addedFriends variable
            var availableFriends = _allFriends.Except(addedFriends).OrderBy(f => f.FirstName);


            // Clear collections used in view and refill them
            AddedFriends.Clear();
            AvailableFriends.Clear();

            foreach (var friend in addedFriends)
            {
                AddedFriends.Add(friend);
            }

            foreach (var friend in availableFriends)
            {
                AvailableFriends.Add(friend);
            }
        }

        /// <summary>
        /// Creates a new meeting wrapper and stores it in the Meeting property.
        /// Also adds an event handler for the property changed event
        /// </summary>
        /// <param name="meeting"></param>
        private void InitializeMeeting(Meeting meeting)
        {
            Meeting = new MeetingWrapper(meeting);
            Meeting.PropertyChanged += (s, e) =>
            {
                if (!HasChanges)
                    HasChanges = _meetingRepository.HasChanges();
                if (e.PropertyName == nameof(Meeting.HasErrors))
                    ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();

                if (e.PropertyName == nameof(Meeting.Title))
                    SetTitle();

            };

            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();

            // Trigger validation
            if (0 == Meeting.Id)
                Meeting.Title = "";

            SetTitle();
        }

        private void SetTitle()
        {
            Title = Meeting.Title;
        }

        /// <summary>
        /// Creates a new meeting and adds it to the meeting repository
        /// </summary>
        /// <returns></returns>
        private Meeting CreateNewMeeting()
        {
            var meeting = new Meeting
            {
                DateFrom = DateTime.Now.Date,
                DateTo = DateTime.Now.Date
            };
            _meetingRepository.Add(meeting);
            return meeting;
        }

        /// <summary>
        /// Refresh <see cref="FriendDetailViewModel"/> on the picklist when Friend is modified
        /// </summary>
        /// <param name="args"></param>
        private async void AfterDetailSaved(AfterDetailSavedEventArgs args)
        {
            if (args.ViewModelName == nameof(FriendDetailViewModel))
            {
                // Reload modified friend from database, because it is cached and will not instantly reflect the changes on the DbContext
                await _meetingRepository.ReloadAsync(args.Id);

                _allFriends = await _meetingRepository.GetAllFriendsAsync();

                SetUpPickList();

            }
        }

        /// <summary>
        /// Refresh <see cref="FriendDetailViewModel"/> on the picklist when Friend is deleted
        /// </summary>
        /// <param name="args"></param>
        private async void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            if (args.ViewModelName == nameof(FriendDetailViewModel))
            {


                _allFriends = await _meetingRepository.GetAllFriendsAsync();

                SetUpPickList();

            }
        }

        protected override async void OnDeleteExecute()
        {
            var result = await MessageDialogService.ShowOkCancelDialogAsync($"¿Realmente quiere eliminar la reunión?", "Pregunta");

            if (result == MessageDialogResult.OK)
            {

                // Delete friend and save changes
                _meetingRepository.Remove(Meeting.Model);
                await _meetingRepository.SaveAsync();
                RaiseDetailDeletedEvent(Meeting.Id);


            }
        }

        protected override bool OnSaveCanExcute()
        {
            return Meeting != null && !Meeting.HasErrors && HasChanges;
        }

        protected override async void OnSaveExceute()
        {
            await _meetingRepository.SaveAsync();
            HasChanges = _meetingRepository.HasChanges();
            Id = Meeting.Id;
            RaiseDetailSavedEvent(Meeting.Id, Meeting.Title);
        }
    }
}
