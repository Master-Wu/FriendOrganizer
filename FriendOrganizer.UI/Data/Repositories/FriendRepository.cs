using FriendOrganizer.Data;
using FriendOrganizer.Model;
using System.Data.Entity;
using System.Threading.Tasks;

namespace FriendOrganizer.UI.Data.Repositories
{
    public class FriendRepository : IFriendRepository
    {
        private FriendOrganizerDbContext _context;


        public FriendRepository(FriendOrganizerDbContext context)
        {
            _context = context;
        }

        // Is async to maintain the UI responsive while the data is loading
        public async Task<Friend> GetByIdAsync(int friendId)
        {
            return await _context.Friends.SingleAsync(f => f.Id == friendId);
        }

        // Look for changes on the model
        public bool HasChanges()
        {
            return _context.ChangeTracker.HasChanges();
        }

        // Save the model
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();

        }
    }
}
