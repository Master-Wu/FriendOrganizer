using FriendOrganizer.Model;
using System.Data.Entity;

namespace FriendOrganizer.Data
{
    public class FriendOrganizerDbContext : DbContext
    {
        #region DBSETS
        public DbSet<Friend> Friends { get; set; }
        #endregion


        #region CONSTRUCTOR

        public FriendOrganizerDbContext() : base("FriendOrganizerDB")
        {

        }
        #endregion

        #region OVERRIDE METHODS

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }

        #endregion


    }
}
