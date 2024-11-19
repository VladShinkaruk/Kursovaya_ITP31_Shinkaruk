using Microsoft.AspNetCore.Identity;
using WebCityEvents.Models;

namespace WebCityEvents.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, EventContext context)
        {
            string[] roleNames = { "admin", "user" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            var adminUsers = await userManager.GetUsersInRoleAsync("admin");
            if (!adminUsers.Any())
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "TestAdmin",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "0809Vlad");
                await userManager.AddToRoleAsync(adminUser, "admin");
            }

            if (context.Places.Any() && context.Organizers.Any() && context.Customers.Any() && context.Events.Any() && context.TicketOrders.Any())
            {
                return;
            }

            for (int i = 1; i <= 600; i++)
            {
                context.Places.Add(new Place
                {
                    PlaceName = $"Place{i}",
                    Geolocation = $"Geo{i}"
                });
            }

            for (int i = 1; i <= 500; i++)
            {
                context.Organizers.Add(new Organizer
                {
                    FullName = $"Organizer{i}",
                    Post = $"Post{i}"
                });
            }

            for (int i = 1; i <= 500; i++)
            {
                context.Customers.Add(new Customer
                {
                    FullName = $"Customer{i}",
                    PassportData = $"Passport{i}"
                });
            }

            var random = new Random();
            for (int i = 1; i <= 20000; i++)
            {
                context.Events.Add(new Event
                {
                    EventName = $"Event{i}",
                    EventDate = DateTime.Now.AddDays(random.Next(0, 365)),
                    TicketPrice = 100 + (float)(random.NextDouble() * 1000),
                    TicketAmount = 100 + random.Next(500),
                    PlaceID = random.Next(1, 600),
                    OrganizerID = random.Next(1, 500)
                });
            }

            for (int i = 1; i <= 25000; i++)
            {
                context.TicketOrders.Add(new TicketOrder
                {
                    EventID = random.Next(1, 20000),
                    CustomerID = random.Next(1, 500),
                    OrderDate = DateTime.Now.AddDays(-random.Next(0, 365)),
                    TicketCount = random.Next(1, 5)
                });
            }

            await context.SaveChangesAsync();
        }
    }
}