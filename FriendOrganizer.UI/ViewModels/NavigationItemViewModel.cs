using FriendOrganizer.UI.Events;
using Prism.Commands;
using Prism.Events;
using System.Windows.Input;

namespace FriendOrganizer.UI.ViewModels
{
    public class NavigationItemViewModel : ViewModelBase
    {
        private IEventAggregator _eventAggregator;

        public int Id { get; set; }

        private string _detailViewModelName;
        private string _displayMember;

        public string DisplayMember
        {
            get { return _displayMember; }
            set
            {
                _displayMember = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenDetailViewCommand { get; }

        public NavigationItemViewModel(int id, string displayMember, IEventAggregator eventAggregator, string detailViewModelName)
        {
            _eventAggregator = eventAggregator;
            Id = id;
            _detailViewModelName = detailViewModelName;
            DisplayMember = displayMember;
            OpenDetailViewCommand = new DelegateCommand(OnOpenDetailViewExcute);
        }

        private void OnOpenDetailViewExcute()
        {
            _eventAggregator.GetEvent<OpenDetailViewEvent>()
                        .Publish(
                new OpenDetailViewEventArgs
                {
                    Id = Id,
                    ViewModelName = _detailViewModelName

                });
        }
    }
}
