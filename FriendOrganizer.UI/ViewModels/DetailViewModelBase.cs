using FriendOrganizer.UI.Events;
using FriendOrganizer.UI.Views.Services;
using Prism.Commands;
using Prism.Events;
using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
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

        protected virtual async void OnCloseDetailViewExecute()
        {
            // Check for changes in the ViewModel
            if (HasChanges)
            {
                var result = await MessageDialogService.ShowOkCancelDialogAsync($"Se realizaron cambios. ¿Desea cerrar la pestaña?", "Pregunta");

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

        protected virtual void RaiseCollectionSavedEvent()
        {
            EventAggregator.GetEvent<AfterCollectionSavedEvent>()
                .Publish(new AfterCollectionSavedEventArgs
                {
                    ViewModelName = this.GetType().Name
                });
        }
        #endregion

        protected async Task SaveWithOptimisticConcurrencyAsync(Func<Task> saveFunc, Action afterSaveAction)
        {
            try
            {
                await saveFunc();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var databaseValues = ex.Entries.Single().GetDatabaseValues();

                // Value in database was deleted
                if (null == databaseValues)
                {
                    await MessageDialogService.ShowInfoDialogAsync("La entidad fue borrada por otro usuario");
                    RaiseDetailDeletedEvent(Id);
                    return;

                }

                var result = await MessageDialogService.ShowOkCancelDialogAsync("La entidad fue cambiada por otro usuario. Presione OK para guardar los cambios" +
                    " o Cancelar para recargar la entidad desde la base de datos.", "Pregunta");

                // Update the original values 
                if (result == MessageDialogResult.OK)
                {
                    var entry = ex.Entries.Single();
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                    await saveFunc();
                }

                else
                {
                    // Reload entity from database
                    await ex.Entries.Single().ReloadAsync();
                    await LoadAsync(Id);
                }

            }


            afterSaveAction();
        }
    }
}
