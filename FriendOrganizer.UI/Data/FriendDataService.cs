using FriendOrganizer.Data;
using FriendOrganizer.Model;
using System;
using System.Data.Entity;
using System.Threading.Tasks;

namespace FriendOrganizer.UI.Data
{
    public class FriendDataService : IFriendDataService
    {
        private Func<FriendOrganizerDbContext> _contextCreator;

        // Uses the DbContext registered in the bootstrapper class
        public FriendDataService(Func<FriendOrganizerDbContext> contextCreator)
        {
            _contextCreator = contextCreator;
        }

        // Is async to maintain the UI responsive while the data is loading
        public async Task<Friend> GetByIdAsync(int friendId)
        {
            using (var ctx = _contextCreator())
            {
                return await ctx.Friends.AsNoTracking().SingleAsync(f => f.Id == friendId);
            }
        }

        public async Task SaveAsync(Friend friend)
        {
            using (var ctx = _contextCreator())
            {
                // Save the friend in the context
                ctx.Friends.Add(friend);

                // Set the state property of the entry to modified
                ctx.Entry(friend).State = EntityState.Modified;

                await ctx.SaveChangesAsync();
            }
        }
    }
}
