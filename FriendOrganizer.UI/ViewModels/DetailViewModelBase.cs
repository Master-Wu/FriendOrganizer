using FriendOrganizer.UI.Events;
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

        protected readonly IEventAggregator EventAggregator;

        public ICommand SaveCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

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

        public DetailViewModelBase(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
            SaveCommand = new DelegateCommand(OnSaveExceute, OnSaveCanExcute);
            DeleteCommand = new DelegateCommand(OnDeleteExecute);

        }

        #endregion

        #region ABSTRACT METHODS

        protected abstract void OnDeleteExecute();

        protected abstract void OnSaveExceute();

        protected abstract bool OnSaveCanExcute();

        public abstract Task LoadAsync(int? id);

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

        #endregion
    }
}
