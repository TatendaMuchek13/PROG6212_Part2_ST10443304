using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Models;

namespace RaceDayApi.Tests
{
    public static class TestHelper
    {
        // A brand-new, empty database for every test, so tests can't affect each other
        public static RaceDayDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayDbContext(options);
            context.Database.EnsureCreated();

            return context;
        }

        // Pretends a user is logged in, as the token would do in the real API
        public static void SetUser(ControllerBase controller, int userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        public static Event AddEvent(RaceDayDbContext context, int organiserCode, string name = "Joburg Sprints")
        {
            Event newEvent = new Event
            {
                OrganiserCode = organiserCode,
                EventName = name,
                Description = "A 10km city run",
                DateOfEvent = new DateTime(2026, 12, 5, 7, 0, 0),
                RouteInfo = "Joburg City Centre",
                Distance = 10,
                EventType = "Run"
            };

            context.Events.Add(newEvent);
            context.SaveChanges();

            return newEvent;
        }
    }
}
