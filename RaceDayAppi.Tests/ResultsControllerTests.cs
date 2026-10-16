using Microsoft.AspNetCore.Mvc;
using RaceDayApi.Controllers;
using RaceDayApi.Data;
using RaceDayApi.DTOs;
using RaceDayApi.Models;

namespace RaceDayApi.Tests
{
    public class ResultsControllerTests
    {
        // One event owned by organiser 1, one category, one participant enrolled in it
        private class Scenario
        {
            public RaceDayDbContext Context;
            public Event SavedEvent;
            public Category Category;
            public Participant Participant;
            public Enrollment Enrollment;
        }

        private Scenario CreateScenario()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var category = TestHelper.AddCategory(context, savedEvent.EventID);
            var participant = TestHelper.AddParticipant(context, "p1@test.com");
            var enrollment = TestHelper.AddEnrollment(context, participant.PNumber, savedEvent.EventID, category.CateID);

            return new Scenario
            {
                Context = context,
                SavedEvent = savedEvent,
                Category = category,
                Participant = participant,
                Enrollment = enrollment
            };
        }

        private CreateResultDto NewResultDto(int enrollId, string status, int position)
        {
            return new CreateResultDto
            {
                EnrollID = enrollId,
                ResultsStatus = status,
                FinishTime = new TimeSpan(0, 42, 35),
                Position = position
            };
        }

        [Fact]
        public async Task CaptureResult_Owner_SavesResultWithCategoryFromEnrolment()
        {
            var s = CreateScenario();
            var controller = new ResultsController(s.Context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.CaptureResult(NewResultDto(s.Enrollment.EnrollID, "Finished", 1));

            Assert.IsType<CreatedAtActionResult>(result.Result);

            var saved = s.Context.Results.Single();
            Assert.Equal(s.Category.CateID, saved.CateID);
            Assert.Equal(1, saved.Position);
        }

        [Fact]
        public async Task CaptureResult_OtherOrganiser_ReturnsForbid()
        {
            var s = CreateScenario();
            var controller = new ResultsController(s.Context);
            TestHelper.SetUser(controller, 2, "Organiser");

            var result = await controller.CaptureResult(NewResultDto(s.Enrollment.EnrollID, "Finished", 1));

            Assert.IsType<ForbidResult>(result.Result);
            Assert.Empty(s.Context.Results);
        }

        [Fact]
        public async Task CaptureResult_MissingEnrolment_ReturnsNotFound()
        {
            var context = TestHelper.CreateContext();
            var controller = new ResultsController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.CaptureResult(NewResultDto(999, "Finished", 1));

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CaptureResult_FinishedWithPositionZero_ReturnsBadRequest()
        {
            var s = CreateScenario();
            var controller = new ResultsController(s.Context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.CaptureResult(NewResultDto(s.Enrollment.EnrollID, "Finished", 0));

            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Empty(s.Context.Results);
        }

        [Fact]
        public async Task CaptureResult_DidNotFinishWithPosition_ReturnsBadRequest()
        {
            var s = CreateScenario();
            var controller = new ResultsController(s.Context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.CaptureResult(NewResultDto(s.Enrollment.EnrollID, "DNF", 3));

            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Empty(s.Context.Results);
        }

        [Fact]
        public async Task CaptureResult_SecondResultForSameEnrolment_ReturnsConflict()
        {
            var s = CreateScenario();
            TestHelper.AddResult(s.Context, s.Enrollment.EnrollID, s.Category.CateID, 1);
            var controller = new ResultsController(s.Context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.CaptureResult(NewResultDto(s.Enrollment.EnrollID, "Finished", 2));

            Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Single(s.Context.Results);
        }

        [Fact]
        public async Task CaptureResult_PositionAlreadyTaken_ReturnsConflict()
        {
            var s = CreateScenario();
            TestHelper.AddResult(s.Context, s.Enrollment.EnrollID, s.Category.CateID, 1);
            var participant2 = TestHelper.AddParticipant(s.Context, "p2@test.com");
            var enrollment2 = TestHelper.AddEnrollment(s.Context, participant2.PNumber, s.SavedEvent.EventID, s.Category.CateID);
            var controller = new ResultsController(s.Context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.CaptureResult(NewResultDto(enrollment2.EnrollID, "Finished", 1));

            Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Single(s.Context.Results);
        }

        [Fact]
        public async Task UpdateResult_Owner_ChangesPositionAndTime()
        {
            var s = CreateScenario();
            var savedResult = TestHelper.AddResult(s.Context, s.Enrollment.EnrollID, s.Category.CateID, 2);
            var controller = new ResultsController(s.Context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.UpdateResult(savedResult.ResultsID,
                new UpdateResultDto { ResultsStatus = "Finished", FinishTime = new TimeSpan(0, 39, 0), Position = 1 });

            Assert.Equal(1, result.Value.Position);
            Assert.Equal(new TimeSpan(0, 39, 0), s.Context.Results.Single().FinishTime);
        }

        [Fact]
        public async Task UpdateResult_KeepingItsOwnPosition_IsNotAConflict()
        {
            var s = CreateScenario();
            var savedResult = TestHelper.AddResult(s.Context, s.Enrollment.EnrollID, s.Category.CateID, 1);
            var controller = new ResultsController(s.Context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.UpdateResult(savedResult.ResultsID,
                new UpdateResultDto { ResultsStatus = "Finished", FinishTime = new TimeSpan(0, 38, 0), Position = 1 });

            Assert.Null(result.Result);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task UpdateResult_OtherOrganiser_ReturnsForbid()
        {
            var s = CreateScenario();
            var savedResult = TestHelper.AddResult(s.Context, s.Enrollment.EnrollID, s.Category.CateID, 2);
            var controller = new ResultsController(s.Context);
            TestHelper.SetUser(controller, 2, "Organiser");

            var result = await controller.UpdateResult(savedResult.ResultsID,
                new UpdateResultDto { ResultsStatus = "Finished", FinishTime = new TimeSpan(0, 39, 0), Position = 1 });

            Assert.IsType<ForbidResult>(result.Result);
            Assert.Equal(2, s.Context.Results.Single().Position);
        }

        [Fact]
        public async Task GetMyResults_ReturnsOnlyTheCallersResults()
        {
            var s = CreateScenario();
            TestHelper.AddResult(s.Context, s.Enrollment.EnrollID, s.Category.CateID, 1);
            var participant2 = TestHelper.AddParticipant(s.Context, "p2@test.com");
            var enrollment2 = TestHelper.AddEnrollment(s.Context, participant2.PNumber, s.SavedEvent.EventID, s.Category.CateID);
            TestHelper.AddResult(s.Context, enrollment2.EnrollID, s.Category.CateID, 2);
            var controller = new ResultsController(s.Context);
            TestHelper.SetUser(controller, s.Participant.PNumber, "Participant");

            var result = await controller.GetMyResults();

            Assert.Single(result.Value);
            Assert.Equal(s.Participant.PNumber, result.Value.Single().PNumber);
        }

        [Fact]
        public async Task GetResult_AnotherParticipant_ReturnsForbid()
        {
            var s = CreateScenario();
            var savedResult = TestHelper.AddResult(s.Context, s.Enrollment.EnrollID, s.Category.CateID, 1);
            var participant2 = TestHelper.AddParticipant(s.Context, "p2@test.com");
            var controller = new ResultsController(s.Context);
            TestHelper.SetUser(controller, participant2.PNumber, "Participant");

            var result = await controller.GetResult(savedResult.ResultsID);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetResultsForEvent_OtherOrganiser_ReturnsForbid()
        {
            var s = CreateScenario();
            var controller = new ResultsController(s.Context);
            TestHelper.SetUser(controller, 2, "Organiser");

            var result = await controller.GetResultsForEvent(s.SavedEvent.EventID);

            Assert.IsType<ForbidResult>(result.Result);
        }
    }
}