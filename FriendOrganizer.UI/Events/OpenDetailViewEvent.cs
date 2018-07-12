using Prism.Events;

namespace FriendOrganizer.UI.Events
{
    // Specify the event arguments, which is an int representing the friend Id
    public class OpenDetailViewEvent : PubSubEvent<OpenDetailViewEventArgs>
    {


    }

    public class OpenDetailViewEventArgs
    {
        public int? Id { get; set; }
        public string ViewModelName { get; set; }
    }
}
