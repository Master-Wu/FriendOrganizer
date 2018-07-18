using FriendOrganizer.UI.Events;
using FriendOrganizer.UI.Views.Services;
using Prism.Commands;
using Prism.Events;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FriendOrganizer.UI.ViewModels
{
    public abstract class DetailViewModelBase : ViewModelBase, IDetailViewModel
    {
        #region PROPERTIES AND FIELDS

        private bool _hasChanges;
        private int _id;
        private string _title;

        public IMessageDialogService MessageDialogService { get; }

        protected readonly IEventAggregator EventAggregator;

        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }
        public ICommand CloseDetailViewCommand { get; set; }




        public string Title
        {
            get { return _title; }
            protected set { _title = value; OnPropertyChanged(); }
        }


        public int Id
        {
            get { return _id; }
            protected set { _id = value; }
        }


        public bool HasChanges
        {
            get => _hasChanges;
            set
            {
                if (value != _hasChanges)
                {
                    _hasChanges = value;
                    OnPropertyChanged();
                    ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();

                }
            }
        }

        #endregion

        #region CONSTRUCTOR

        public DetailViewModelBase(IEventAggregator eventAggregator, IMessageDialogService messageDialogService)
        {
            MessageDialogService = messageDialogService;

            EventAggregator = eventAggregator;
            SaveCommand = new DelegateCommand(OnSaveExceute, OnSaveCanExcute);
            DeleteCommand = new DelegateCommand(OnDeleteExecute);
            CloseDetailViewCommand = new DelegateCommand(OnCloseDetailViewExecute);

        }

        #endregion

        #region ABSTRACT METHODS

        protected abstract void OnDeleteExecute();

        protected abstract void OnSaveExceute();

        protected abstract bool OnSaveCanExcute();

        public abstract Task LoadAsync(int id);

        #endregion


        #region VIRTUAL METHODS

        protected virtual void RaiseDetailDeletedEvent(int modelId)
        {
            EventAggregator.GetEvent<AfterDetailDeletedEvent>().Publish(
                new AfterDetailDeletedEventArgs
                {
                    Id = modelId,
                    ViewModelName = this.GetType().Name
                });
        }

        protected virtual void RaiseDetailSavedEvent(int modelId, string displayMember)
        {
            EventAggregator.GetEvent<AfterDetailSavedEvent>().Publish(
                new AfterDetailSavedEventArgs
                {
                    Id = modelId,
                    DisplayMember = displayMember,
                    ViewModelName = this.GetType().Name
                });
        }

        protected virtual void OnCloseDetailViewExecute()
        {
            // Check for changes in the ViewModel
            if (HasChanges)
            {
                var result = MessageDialogService.ShowOkCancelDialog($"Se realizaron cambios. ¿Desea cerrar la pestaña?", "Pregunta");

                if (result == MessageDialogResult.Cancel)
                    return;
            }

            // Publish args to AfterDetailClosedEvent. MainViewModel subscribes to this event in order to close a tab
            EventAggregator.GetEvent<AfterDetailClosedEvent>()
                .Publish(new AfterDetailClosedEventArgs
                {
                    Id = this.Id,
                    ViewModelName = this.GetType().Name
                });
        }

        #endregion
    }
}
