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

        public static Participant AddParticipant(RaceDayDbContext context, string email = "participant@test.com")
        {
            Participant participant = new Participant
            {
                Name = "Test",
                Surname = "Participant",
                Cell = "0712345678",
                Email = email,
                Password = "original-hash"
            };

            context.Participants.Add(participant);
            context.SaveChanges();

            return participant;
        }

        public static Organiser AddOrganiser(RaceDayDbContext context, string email = "organiser@test.com")
        {
            Organiser organiser = new Organiser
            {
                Name = "Test",
                Surname = "Organiser",
                Cell = "0712345678",
                Email = email,
                Password = "original-hash"
            };

            context.Organisers.Add(organiser);
            context.SaveChanges();

            return organiser;
        }

        public static Category AddCategory(RaceDayDbContext context, int eventId, string name = "10km")
        {
            Category category = new Category
            {
                EventID = eventId,
                CategoryName = name
            };

            context.Categories.Add(category);
            context.SaveChanges();

            return category;
        }

        public static Enrollment AddEnrollment(RaceDayDbContext context, int pNumber, int eventId, int cateId)
        {
            Enrollment enrollment = new Enrollment
            {
                PNumber = pNumber,
                EventID = eventId,
                CateID = cateId,
                DateOfEnroll = new DateTime(2026, 10, 1)
            };

            context.Enrollments.Add(enrollment);
            context.SaveChanges();

            return enrollment;
        }

        public static Result AddResult(RaceDayDbContext context, int enrollId, int cateId, int position)
        {
            Result result = new Result
            {
                EnrollID = enrollId,
                CateID = cateId,
                ResultsStatus = position > 0 ? "Finished" : "DNF",
                FinishTime = new TimeSpan(0, 40, 0),
                Position = position
            };

            context.Results.Add(result);
            context.SaveChanges();

            return result;
        }
    }
}