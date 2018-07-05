using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace FriendOrganizer.UI.Wrappers
{
    public class ModelWrapper<T> : NotifyDataErrorInfoBase
    {
        public T Model { get; }

        public ModelWrapper(T model)
        {
            Model = model;
        }

        // Changed type to TValue to avoid colliding with the generic parameter T of the class
        protected TValue GetValue<TValue>([CallerMemberName]string propertyName = null)
        {
            // Get the type of the model and call the property value of the object model. Next, cast the object to the TValue
            return (TValue)typeof(T).GetProperty(propertyName).GetValue(Model);
        }

        // Changed type to TValue to avoid colliding with the generic parameter T of the class
        protected void SetValue<TValue>(TValue value, [CallerMemberName]string propertyName = null)
        {
            // Get the type of the model and call the property value of the object model.
            typeof(T).GetProperty(propertyName).SetValue(Model, value);
            OnPropertyChanged(propertyName);
            ValidatePropertyInternal(propertyName, value);

        }

        private void ValidatePropertyInternal(string propertyName, object currentValue)
        {
            ClearErrors(propertyName);            
            ValidateDataAnnotations(propertyName, currentValue);            
            ValidateCustomErrors(propertyName);
        }

        private void ValidateDataAnnotations(string propertyName, object currentValue)
        {
            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(Model) { MemberName = propertyName };

            Validator.TryValidateProperty(currentValue, ctx, results);

            foreach (var result in results)
            {
                AddError(propertyName, result.ErrorMessage);
            }
        }

        private void ValidateCustomErrors(string propertyName)
        {
            var errors = ValidateProperty(propertyName);
            if (null != errors)
                foreach (var err in errors)
                {
                    AddError(propertyName, err);
                }
        }

        protected virtual IEnumerable<string> ValidateProperty(string propertyName)
        {
            return null;
        }
    }
}
