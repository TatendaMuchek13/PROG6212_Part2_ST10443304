using Microsoft.AspNetCore.Mvc;
using RaceDayApi.Controllers;
using RaceDayApi.DTOs;
using RaceDayApi.Models;

namespace RaceDayApi.Tests
{
    public class CategoriesControllerTests
    {
        [Fact]
        public async Task GetCategories_ReturnsAllCategories()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            TestHelper.AddCategory(context, savedEvent.EventID, "10km");
            TestHelper.AddCategory(context, savedEvent.EventID, "21km");
            var controller = new CategoriesController(context);

            var result = await controller.GetCategories();

            Assert.Equal(2, result.Value.Count());
        }

        [Fact]
        public async Task GetCategory_MissingId_ReturnsNotFound()
        {
            var context = TestHelper.CreateContext();
            var controller = new CategoriesController(context);

            var result = await controller.GetCategory(999);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetCategoriesForEvent_ReturnsOnlyThatEventsCategories()
        {
            var context = TestHelper.CreateContext();
            var event1 = TestHelper.AddEvent(context, 1, "Event One");
            var event2 = TestHelper.AddEvent(context, 1, "Event Two");
            TestHelper.AddCategory(context, event1.EventID, "10km");
            TestHelper.AddCategory(context, event1.EventID, "21km");
            TestHelper.AddCategory(context, event2.EventID, "5km");
            var controller = new CategoriesController(context);

            var result = await controller.GetCategoriesForEvent(event1.EventID);

            Assert.Equal(2, result.Value.Count());
        }

        [Fact]
        public async Task GetCategoriesForEvent_MissingEvent_ReturnsNotFound()
        {
            var context = TestHelper.CreateContext();
            var controller = new CategoriesController(context);

            var result = await controller.GetCategoriesForEvent(999);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateCategory_OwnerOfEvent_CreatesCategory()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var controller = new CategoriesController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.CreateCategory(
                new CreateCategoryDto { EventID = savedEvent.EventID, CategoryName = "Under 20" });

            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("Under 20", context.Categories.Single().CategoryName);
        }

        [Fact]
        public async Task CreateCategory_OtherOrganiser_ReturnsForbid()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var controller = new CategoriesController(context);
            TestHelper.SetUser(controller, 2, "Organiser");

            var result = await controller.CreateCategory(
                new CreateCategoryDto { EventID = savedEvent.EventID, CategoryName = "Under 20" });

            Assert.IsType<ForbidResult>(result.Result);
            Assert.Empty(context.Categories);
        }

        [Fact]
        public async Task CreateCategory_MissingEvent_ReturnsNotFound()
        {
            var context = TestHelper.CreateContext();
            var controller = new CategoriesController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.CreateCategory(
                new CreateCategoryDto { EventID = 999, CategoryName = "Under 20" });

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateCategory_DuplicateNameOnSameEvent_ReturnsConflict()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            TestHelper.AddCategory(context, savedEvent.EventID, "10km");
            var controller = new CategoriesController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.CreateCategory(
                new CreateCategoryDto { EventID = savedEvent.EventID, CategoryName = "10km" });

            Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Single(context.Categories);
        }

        [Fact]
        public async Task UpdateCategory_Owner_RenamesCategory()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var category = TestHelper.AddCategory(context, savedEvent.EventID, "10km");
            var controller = new CategoriesController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.UpdateCategory(category.CateID, new UpdateCategoryDto { CategoryName = "5km" });

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal("5km", context.Categories.Single().CategoryName);
        }

        [Fact]
        public async Task UpdateCategory_OtherOrganiser_ReturnsForbid()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var category = TestHelper.AddCategory(context, savedEvent.EventID, "10km");
            var controller = new CategoriesController(context);
            TestHelper.SetUser(controller, 2, "Organiser");

            var result = await controller.UpdateCategory(category.CateID, new UpdateCategoryDto { CategoryName = "5km" });

            Assert.IsType<ForbidResult>(result);
            Assert.Equal("10km", context.Categories.Single().CategoryName);
        }

        [Fact]
        public async Task DeleteCategory_WithEnrolments_ReturnsConflict()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var category = TestHelper.AddCategory(context, savedEvent.EventID);
            var participant = TestHelper.AddParticipant(context);
            TestHelper.AddEnrollment(context, participant.PNumber, savedEvent.EventID, category.CateID);
            var controller = new CategoriesController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.DeleteCategory(category.CateID);

            Assert.IsType<ConflictObjectResult>(result);
            Assert.Single(context.Categories);
        }

        [Fact]
        public async Task DeleteCategory_Owner_RemovesCategory()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var category = TestHelper.AddCategory(context, savedEvent.EventID);
            var controller = new CategoriesController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.DeleteCategory(category.CateID);

            Assert.IsType<OkObjectResult>(result);
            Assert.Empty(context.Categories);
        }
    }
}