using Microsoft.AspNetCore.Mvc;
using RaceDayApi.Controllers;
using RaceDayApi.DTOs;
using RaceDayApi.Models;

namespace RaceDayApi.Tests
{
    public class EventsControllerTests
    {
        private EventDto NewEventDto()
        {
            return new EventDto
            {
                EventName = "Updated Name",
                Description = "Updated description",
                DateOfEvent = new DateTime(2027, 1, 16, 6, 30, 0),
                RouteInfo = "Pretoria CBD",
                Distance = 21,
                EventType = "Cycle"
            };
        }

        [Fact]
        public async Task GetEvents_ReturnsAllEvents()
        {
            var context = TestHelper.CreateContext();
            TestHelper.AddEvent(context, 1, "Event One");
            TestHelper.AddEvent(context, 2, "Event Two");
            var controller = new EventsController(context);

            var result = await controller.GetEvents();

            Assert.Equal(2, result.Value.Count());
        }

        [Fact]
        public async Task GetEvent_ExistingId_ReturnsTheEvent()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var controller = new EventsController(context);

            var result = await controller.GetEvent(savedEvent.EventID);

            Assert.Equal("Joburg Sprints", result.Value.EventName);
        }

        [Fact]
        public async Task GetEvent_MissingId_ReturnsNotFound()
        {
            var context = TestHelper.CreateContext();
            var controller = new EventsController(context);

            var result = await controller.GetEvent(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_UsesOrganiserFromToken()
        {
            var context = TestHelper.CreateContext();
            var controller = new EventsController(context);
            TestHelper.SetUser(controller, 5, "Organiser");

            var result = await controller.CreateEvent(NewEventDto());

            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(5, context.Events.Single().OrganiserCode);
        }

        [Fact]
        public async Task UpdateEvent_Owner_UpdatesTheEvent()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var controller = new EventsController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.UpdateEvent(savedEvent.EventID, NewEventDto());

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Updated Name", context.Events.Single().EventName);
        }

        [Fact]
        public async Task UpdateEvent_OtherOrganiser_ReturnsForbid()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var controller = new EventsController(context);
            TestHelper.SetUser(controller, 2, "Organiser");

            var result = await controller.UpdateEvent(savedEvent.EventID, NewEventDto());

            Assert.IsType<ForbidResult>(result);
            Assert.Equal("Joburg Sprints", context.Events.Single().EventName);
        }

        [Fact]
        public async Task UpdateEvent_MissingId_ReturnsNotFound()
        {
            var context = TestHelper.CreateContext();
            var controller = new EventsController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.UpdateEvent(999, NewEventDto());

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteEvent_WithCategories_ReturnsConflict()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            context.Categories.Add(new Category { EventID = savedEvent.EventID, CategoryName = "10km" });
            context.SaveChanges();
            var controller = new EventsController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.DeleteEvent(savedEvent.EventID);

            Assert.IsType<ConflictObjectResult>(result);
            Assert.Single(context.Events);
        }

        [Fact]
        public async Task DeleteEvent_Owner_RemovesTheEvent()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var controller = new EventsController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.DeleteEvent(savedEvent.EventID);

            Assert.IsType<OkObjectResult>(result);
            Assert.Empty(context.Events);
        }
    }
}