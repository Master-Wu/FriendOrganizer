namespace FriendOrganizer.UI.ViewModels
{
    public class NavigationItemViewModel : ViewModelBase
    {
        public int Id { get; set; }
        private string _displayMember;

        public string DisplayMember
        {
            get { return _displayMember; }
            set
            { _displayMember = value;
                OnPropertyChanged();
            }
        }

        public NavigationItemViewModel(int id, string displayMember)
        {
            Id = id;
            DisplayMember = displayMember;
        }

    }
}
