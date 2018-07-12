using FriendOrganizer.UI.Data.Lookups;
using FriendOrganizer.UI.Events;
using Prism.Events;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FriendOrganizer.UI.ViewModels
{
    public class NavigationViewModel : ViewModelBase, INavigationViewModel
    {
        #region PRIVATE MEMBERS

        private IFriendLookupDataService _friendLookupService;
        private IEventAggregator _eventAggregator;

        #endregion

        #region PUBLIC PROPERTIES

        public ObservableCollection<NavigationItemViewModel> Friends { get; }

        #endregion

        #region CONSTRUCTOR

        public NavigationViewModel(IFriendLookupDataService friendLookupService, IEventAggregator eventAggregator)
        {
            _friendLookupService = friendLookupService;
            _eventAggregator = eventAggregator;
            Friends = new ObservableCollection<NavigationItemViewModel>();
            _eventAggregator.GetEvent<AfterDetailSavedEvent>().Subscribe(AfterDetailSaved);
            _eventAggregator.GetEvent<AfterDetailDeletedEvent>().Subscribe(AfterDetailDeleted);
        }



        #endregion

        #region METHODS

        public async Task LoadAsync()
        {
            var lookup = await _friendLookupService.GetFriendLookupAsync();
            foreach (var item in lookup)
            {
                Friends.Add(new NavigationItemViewModel(item.Id, item.DisplayMember, _eventAggregator, nameof(FriendDetailViewModel)));
            }
        }

        // Notify navigation of changes after a friend is added
        private void AfterDetailSaved(AfterDetailSavedEventArgs obj)
        {
            switch (obj.ViewModelName)
            {
                case nameof(FriendDetailViewModel):
                    var lookupItem = Friends.SingleOrDefault(l => l.Id == obj.Id);

                    // If we are creating a friend...
                    if (null == lookupItem)
                        Friends.Add(new NavigationItemViewModel(obj.Id, obj.DisplayMember, _eventAggregator, nameof(FriendDetailViewModel)));

                    // Else if the friend already exists...
                    else lookupItem.DisplayMember = obj.DisplayMember;
                        
                    break;
            }


        }

        // Notify navigation of changes after a friend is deleted
        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            switch (args.ViewModelName)
            {
                case (nameof(FriendDetailViewModel)):
                    var friend = Friends.SingleOrDefault(f => f.Id == args.Id);
                    if (null != friend)
                        Friends.Remove(friend);
                    break;
            }



        }

        #endregion
    }
}
