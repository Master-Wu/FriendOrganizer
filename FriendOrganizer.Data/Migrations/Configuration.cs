namespace FriendOrganizer.Data.Migrations
{
    using FriendOrganizer.Model;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Migrations;
    using System.Linq;

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

            context.SaveChanges();

            context.FriendPhoneNumbers.AddOrUpdate(pn => pn.Number,
                new FriendPhoneNumber { Number = "+54 3466559040", FriendId = context.Friends.First().Id }
            );

            context.Meetings.AddOrUpdate(m => m.Title,
                new Meeting
                {
                    Title = "Mirar el partido",
                    DateFrom = new DateTime(2018, 5, 12),
                    DateTo = new DateTime(2018, 5, 12),
                    Friends = new List<Friend>
                    {
                        context.Friends.Single(f => f.FirstName  == "John" && f.LastName == "Locke"),
                        context.Friends.Single(f => f.FirstName  == "Leonel" && f.LastName == "Villa"),

                    }
                }
                );


        }
    }
}
