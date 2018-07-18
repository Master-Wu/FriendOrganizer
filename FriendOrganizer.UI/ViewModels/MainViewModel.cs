using Autofac.Features.Indexed;
using FriendOrganizer.UI.Events;
using FriendOrganizer.UI.Views.Services;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FriendOrganizer.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private IMessageDialogService _messageDialogService;
        private IIndex<string, IDetailViewModel> _detailViewModelCreator;
        private IEventAggregator _eventAggregator;
        private int _nextNewItemId = 0;
        private IDetailViewModel _selectedDetailViewModel;

        public ObservableCollection<IDetailViewModel> DetailViewModels { get; }

        public IDetailViewModel SelectedDetailViewModel
        {
            get => _selectedDetailViewModel;
            set
            {
                _selectedDetailViewModel = value;
                OnPropertyChanged();
            }
        }

        public INavigationViewModel NavigationViewModel { get; }

        public ICommand CreateNewDetailCommand { get; }

        #region CONSTRUCTOR

        public MainViewModel(INavigationViewModel navigationViewModel, IIndex<string, IDetailViewModel> detailViewModelCreator, IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService)
        {
            _messageDialogService = messageDialogService;
            _detailViewModelCreator = detailViewModelCreator;

            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<OpenDetailViewEvent>()
                .Subscribe(OnOpenDetailView);
            _eventAggregator.GetEvent<AfterDetailDeletedEvent>()
                .Subscribe(AfterDetailDeleted);
            _eventAggregator.GetEvent<AfterDetailClosedEvent>()
                .Subscribe(AfterDetailClosed);


            CreateNewDetailCommand = new DelegateCommand<Type>(OnCreateNewDetailExcute);

            NavigationViewModel = navigationViewModel;
            DetailViewModels = new ObservableCollection<IDetailViewModel>();
        }


        #endregion

        /// <summary>
        /// Remove DetailViewModel from collection and close tab when it's deleted
        /// </summary>
        /// <param name="args"></param>
        private void AfterDetailDeleted(AfterDetailDeletedEventArgs args)
        {
            RemoveDetailViewModel(args.Id, args.ViewModelName);
        }

        /// <summary>
        /// Remove DetailViewModel from collection and close tab
        /// </summary>
        /// <param name="args"></param>
        private void AfterDetailClosed(AfterDetailClosedEventArgs args)
        {
            RemoveDetailViewModel(args.Id, args.ViewModelName);
        }

        private void OnCreateNewDetailExcute(Type viewModelType)
        {
            OnOpenDetailView(new OpenDetailViewEventArgs { Id = _nextNewItemId--, ViewModelName = viewModelType.Name });
        }

        public async Task LoadAsync()
        {
            await NavigationViewModel.LoadAsync();
        }

        private async void OnOpenDetailView(OpenDetailViewEventArgs args)
        {
            var detailViewModel = DetailViewModels.SingleOrDefault(vm => vm.Id == args.Id && vm.GetType().Name == args.ViewModelName);

            // Create new detail view tab if it's not open
            if (null == detailViewModel)
            {
                detailViewModel = _detailViewModelCreator[args.ViewModelName];
                await detailViewModel.LoadAsync(args.Id);
                // Add to collection of detail views
                DetailViewModels.Add(detailViewModel);
            }

            // Set as SelectedDetailViewModel (open tab)
            SelectedDetailViewModel = detailViewModel;
        }

        /// <summary>
        /// Remove a DetailViewModel object from the DetailViewModelsCollection
        /// </summary>
        /// <param name="id">The Id of the DetailViewModel remove</param>
        /// <param name="viewModelName">The Name of the DetailViewModel remove</param>
        private void RemoveDetailViewModel(int? id, string viewModelName)
        {

            var detailViewModel = DetailViewModels.SingleOrDefault(vm => vm.Id == id && vm.GetType().Name == viewModelName);

            if (null != detailViewModel)
                DetailViewModels.Remove(detailViewModel);
        }

    }
}
