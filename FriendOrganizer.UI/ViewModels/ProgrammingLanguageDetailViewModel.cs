using FriendOrganizer.Model;
using FriendOrganizer.UI.Data.Repositories;
using FriendOrganizer.UI.Views.Services;
using FriendOrganizer.UI.Wrappers;
using Prism.Commands;
using Prism.Events;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace FriendOrganizer.UI.ViewModels
{
    public class ProgrammingLanguageDetailViewModel : DetailViewModelBase
    {
        #region PRIVATE MEMBERS

        private IProgrammingLanguageRepository _programmingLanguageRepository;
        private ProgrammingLanguageWrapper _selectedProgrammingLanguage;

        #endregion

        #region PUBLIC PROPERTIES
        public ObservableCollection<ProgrammingLanguageWrapper> ProgrammingLanguages { get; }
        public ProgrammingLanguageWrapper SelectedProgrammingLanguage
        {
            get => _selectedProgrammingLanguage;
            set
            {
                _selectedProgrammingLanguage = value;
                OnPropertyChanged();
                ((DelegateCommand)RemoveCommand).RaiseCanExecuteChanged();
            }
        }

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        #endregion

        #region CONSTRUCTOR

        public ProgrammingLanguageDetailViewModel(IEventAggregator eventAggregator,
            IMessageDialogService messageDialogService, IProgrammingLanguageRepository programmingLanguageRepository) : base(eventAggregator, messageDialogService)
        {
            _programmingLanguageRepository = programmingLanguageRepository;

            Title = "Lenguajes de programación";

            ProgrammingLanguages = new ObservableCollection<ProgrammingLanguageWrapper>();

            AddCommand = new DelegateCommand(OnAddExecute);
            RemoveCommand = new DelegateCommand(OnRemoveExecute, OnRemoveCanExecute);
        }


        #endregion

        #region METHODS

        public async override Task LoadAsync(int id)
        {
            Id = id;

            foreach (var wrapper in ProgrammingLanguages)
            {
                wrapper.PropertyChanged -= Wrapper_PropertyChanged;
            }

            // Clear collection
            ProgrammingLanguages.Clear();

            // Load programming languages from database
            var languages = await _programmingLanguageRepository.GetAllAsync();

            // Create wrapper for each language and add to collection
            foreach (var model in languages)
            {
                var wrapper = new ProgrammingLanguageWrapper(model);
                wrapper.PropertyChanged += Wrapper_PropertyChanged;
                ProgrammingLanguages.Add(wrapper);
            }
        }

        private void Wrapper_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!HasChanges)
                HasChanges = _programmingLanguageRepository.HasChanges();
            if (e.PropertyName == nameof(ProgrammingLanguageWrapper.HasErrors))
                ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        protected override void OnDeleteExecute()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Allow saving data if it has changes and none of the programming languages on the collection have errors
        /// </summary>
        /// <returns></returns>
        protected override bool OnSaveCanExcute()
        {
            return HasChanges && ProgrammingLanguages.All(pl => !pl.HasErrors);
        }

        protected override async void OnSaveExceute()
        {
            try
            {
                await _programmingLanguageRepository.SaveAsync();
                HasChanges = _programmingLanguageRepository.HasChanges();
                RaiseCollectionSavedEvent();
            }
            catch (Exception ex)
            {
                while (ex.InnerException != null)
                    ex = ex.InnerException;
                await MessageDialogService.ShowInfoDialogAsync("Error guardando. Los datos serán recargados." + ex.Message);
                await LoadAsync(Id);
            }

        }

        private bool OnRemoveCanExecute()
        {
            return null != SelectedProgrammingLanguage;
        }

        private async void OnRemoveExecute()
        {
            var isReferenced = await _programmingLanguageRepository.IsReferencedByFriendAsync(SelectedProgrammingLanguage.Id);
            if (isReferenced)
            {
                await MessageDialogService.ShowInfoDialogAsync($"El lenguaje {SelectedProgrammingLanguage.Name} no puede eliminarse porque está referenciado por uno o más contactos");
                return;
            }

            SelectedProgrammingLanguage.PropertyChanged -= Wrapper_PropertyChanged;
            _programmingLanguageRepository.Remove(SelectedProgrammingLanguage.Model);
            ProgrammingLanguages.Remove(SelectedProgrammingLanguage);
            SelectedProgrammingLanguage = null;
            HasChanges = _programmingLanguageRepository.HasChanges();
            ((DelegateCommand)SaveCommand).RaiseCanExecuteChanged();
        }

        private void OnAddExecute()
        {
            var wrapper = new ProgrammingLanguageWrapper(new ProgrammingLanguage());
            wrapper.PropertyChanged += Wrapper_PropertyChanged;
            _programmingLanguageRepository.Add(wrapper.Model);
            ProgrammingLanguages.Add(wrapper);

            // Trigger validation
            wrapper.Name = "";
        }

        #endregion
    }
}
