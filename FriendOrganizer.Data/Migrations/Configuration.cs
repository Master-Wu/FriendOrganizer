namespace FriendOrganizer.Data.Migrations
{
    using FriendOrganizer.Model;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<FriendOrganizer.Data.FriendOrganizerDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(FriendOrganizer.Data.FriendOrganizerDbContext context)
        {
            // FirstName is the identifier expression of my table. It will identify items with this. Normally we use an ID
            context.Friends.AddOrUpdate(f => f.FirstName,
                new Friend { FirstName = "John", LastName = "Locke" },
                new Friend { FirstName = "Leonel", LastName = "Villa" },
                new Friend { FirstName = "Sally", LastName = "Amaki" },
                new Friend { FirstName = "Lumi", LastName = "Lagro" }
                );

            context.ProgrammingLanguages.AddOrUpdate(pl => pl.Name,
                new ProgrammingLanguage { Name = "C#" },
                new ProgrammingLanguage { Name = "TypeScript" },
                new ProgrammingLanguage { Name = "JAVA" },
                new ProgrammingLanguage { Name = "PHP" },
                new ProgrammingLanguage { Name = "Swift" }
                );
        }
    }
}
