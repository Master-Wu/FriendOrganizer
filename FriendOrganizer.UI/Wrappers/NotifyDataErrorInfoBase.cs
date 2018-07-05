using FriendOrganizer.UI.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace FriendOrganizer.UI.Wrappers
{
    public class NotifyDataErrorInfoBase : ViewModelBase, INotifyDataErrorInfo
    {
        #region PRIVATE MEMBERS
        private Dictionary<string, List<string>> _errorsByPropertyName = new Dictionary<string, List<string>>();
        #endregion

        public bool HasErrors => _errorsByPropertyName.Any();

        #region ERROR HANDLERS
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            // If the dictionary contains a key with the property name, return the list stored under that key.
            // Else, return null
            return _errorsByPropertyName.ContainsKey(propertyName) ? _errorsByPropertyName[propertyName] : null;
        }

        // If ErrorsChanged is not null, invoke for this instance 
        protected virtual void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            base.OnPropertyChanged(nameof(HasErrors));
        }

        protected void AddError(string propertyName, string error)
        {
            // If the dictionary doesn't contain a key for the property name, add it
            if (!_errorsByPropertyName.ContainsKey(propertyName))
                _errorsByPropertyName[propertyName] = new List<string>();

            // Add the error if is not already part of the list of errors for the property
            if (!_errorsByPropertyName[propertyName].Contains(error))
                _errorsByPropertyName[propertyName].Add(error);


            OnErrorsChanged(propertyName);

        }

        protected void ClearErrors(string propertyName)
        {
            // If the dictionary contains a key for the property name, remove it
            if (_errorsByPropertyName.ContainsKey(propertyName))
            {
                _errorsByPropertyName.Remove(propertyName);
                OnErrorsChanged(propertyName);
            }

        }


        #endregion

    }
}
