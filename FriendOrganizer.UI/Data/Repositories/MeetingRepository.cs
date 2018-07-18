using FriendOrganizer.Data;
using FriendOrganizer.Model;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace FriendOrganizer.UI.Data.Repositories
{
    public class MeetingRepository : GenericRepository<Meeting, FriendOrganizerDbContext>, IMeetingRepository
    {
        public MeetingRepository(FriendOrganizerDbContext context) : base(context)
        {
        }

        public override async Task<Meeting> GetByIdAsync(int id)
        {
            return await Context.Meetings
                .Include(m => m.Friends)
                .SingleAsync(m => m.Id == id); ;
        }

        public async Task<List<Friend>> GetAllFriendsAsync()
        {
            return await Context.Set<Friend>().ToListAsync();
        }

        /// <summary>
        /// Reload a single friend from database
        /// </summary>
        /// <param name="friendId"></param>
        /// <returns></returns>
        public async Task ReloadAsync(int friendId)
        {
            var dbEntityEntry = Context.ChangeTracker.Entries<Friend>().SingleOrDefault(db => db.Entity.Id == friendId);
            if (null != dbEntityEntry)
                await dbEntityEntry.ReloadAsync();
        }
    }
}
