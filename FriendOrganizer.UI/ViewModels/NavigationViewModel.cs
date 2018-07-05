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
            _eventAggregator.GetEvent<AfterFriendSaveEvent>().Subscribe(AfterFriendSaved);
        }

        #endregion

        #region METHODS

        private void AfterFriendSaved(AfterFriendSaveEventArgs obj)
        {
            var lookupItem = Friends.Single(l => l.Id == obj.Id);
            lookupItem.DisplayMember = obj.DisplayMember;
        }

        public async Task LoadAsync()
        {
            var lookup = await _friendLookupService.GetFriendLookupAsync();
            foreach (var item in lookup)
            {
                Friends.Add(new NavigationItemViewModel(item.Id, item.DisplayMember, _eventAggregator));
            }
        }

        #endregion
    }
}
