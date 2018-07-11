using FriendOrganizer.Model;
using System;
using System.Collections.Generic;

namespace FriendOrganizer.UI.Wrappers
{
    /// <summary>
    /// A wrapper for the Friend model class
    /// </summary>
    public class FriendWrapper : ModelWrapper<Friend>
    {

        #region PUBLIC PROPERTIES

        public int Id { get => Model.Id; }

        public string FirstName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public string LastName
        {
            get => GetValue<string>();
            set => SetValue(value);
        }

        public int? FavoriteLanguageId
        {
            get => GetValue<int?>();
            set => SetValue(value);
        }


        public string Email
        {
            get => GetValue<string>();
            set => SetValue(value);

        }

        #endregion


        #region CONSTRUCTOR

        public FriendWrapper(Friend model) : base(model)
        {
        }

        #endregion

        #region OVERRIDE METHODS
        protected override IEnumerable<string> ValidateProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(FirstName):
                    if (string.Equals(FirstName, "Robot", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return "Robots are not valid friends";
                    }
                    break;

                case nameof(LastName):
                    break;

                case nameof(Email):
                    break;
            }
        }
        #endregion


    }
}
