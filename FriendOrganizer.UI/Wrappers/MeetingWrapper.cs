using FriendOrganizer.Model;
using System;

namespace FriendOrganizer.UI.Wrappers
{
    public class MeetingWrapper : ModelWrapper<Meeting>
    {
        public int Id { get => Model.Id; }

        public string Title
        {
            get => GetValue<string>();
            set => SetValue(value);

        }

        public DateTime DateFrom
        {
            get => GetValue<DateTime>();
            set => SetValue(value);
        }

        public DateTime DateTo
        {
            get => GetValue<DateTime>();
            set => SetValue(value);
        }

        public MeetingWrapper(Meeting model) : base(model)
        {

        }


    }
}
