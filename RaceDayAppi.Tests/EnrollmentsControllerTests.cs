using Microsoft.AspNetCore.Mvc;
using RaceDayApi.Controllers;
using RaceDayApi.DTOs;
using RaceDayApi.Models;

namespace RaceDayApi.Tests
{
    public class EnrollmentsControllerTests
    {
        [Fact]
        public async Task Enroll_ValidRequest_CreatesEnrolmentForTheTokenUser()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var category = TestHelper.AddCategory(context, savedEvent.EventID);
            var participant = TestHelper.AddParticipant(context);
            var controller = new EnrollmentsController(context);
            TestHelper.SetUser(controller, participant.PNumber, "Participant");

            var result = await controller.Enroll(
                new CreateEnrollmentDto { EventID = savedEvent.EventID, CateID = category.CateID });

            Assert.IsType<CreatedAtActionResult>(result.Result);

            var saved = context.Enrollments.Single();
            Assert.Equal(participant.PNumber, saved.PNumber);
            Assert.Equal(category.CateID, saved.CateID);
        }

        [Fact]
        public async Task Enroll_MissingEvent_ReturnsNotFound()
        {
            var context = TestHelper.CreateContext();
            var participant = TestHelper.AddParticipant(context);
            var controller = new EnrollmentsController(context);
            TestHelper.SetUser(controller, participant.PNumber, "Participant");

            var result = await controller.Enroll(new CreateEnrollmentDto { EventID = 999, CateID = 1 });

            Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Empty(context.Enrollments);
        }

        [Fact]
        public async Task Enroll_MissingCategory_ReturnsNotFound()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var participant = TestHelper.AddParticipant(context);
            var controller = new EnrollmentsController(context);
            TestHelper.SetUser(controller, participant.PNumber, "Participant");

            var result = await controller.Enroll(new CreateEnrollmentDto { EventID = savedEvent.EventID, CateID = 999 });

            Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Empty(context.Enrollments);
        }

        [Fact]
        public async Task Enroll_CategoryFromAnotherEvent_ReturnsBadRequest()
        {
            var context = TestHelper.CreateContext();
            var event1 = TestHelper.AddEvent(context, 1, "Event One");
            var event2 = TestHelper.AddEvent(context, 1, "Event Two");
            var categoryOfEvent2 = TestHelper.AddCategory(context, event2.EventID);
            var participant = TestHelper.AddParticipant(context);
            var controller = new EnrollmentsController(context);
            TestHelper.SetUser(controller, participant.PNumber, "Participant");

            var result = await controller.Enroll(
                new CreateEnrollmentDto { EventID = event1.EventID, CateID = categoryOfEvent2.CateID });

            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Empty(context.Enrollments);
        }

        [Fact]
        public async Task Enroll_SameEventTwice_ReturnsConflict()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var category = TestHelper.AddCategory(context, savedEvent.EventID);
            var participant = TestHelper.AddParticipant(context);
            TestHelper.AddEnrollment(context, participant.PNumber, savedEvent.EventID, category.CateID);
            var controller = new EnrollmentsController(context);
            TestHelper.SetUser(controller, participant.PNumber, "Participant");

            var result = await controller.Enroll(
                new CreateEnrollmentDto { EventID = savedEvent.EventID, CateID = category.CateID });

            Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Single(context.Enrollments);
        }

        [Fact]
        public async Task GetMyEnrollments_ReturnsOnlyTheCallersEnrolments()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var category = TestHelper.AddCategory(context, savedEvent.EventID);
            var participant1 = TestHelper.AddParticipant(context, "p1@test.com");
            var participant2 = TestHelper.AddParticipant(context, "p2@test.com");
            TestHelper.AddEnrollment(context, participant1.PNumber, savedEvent.EventID, category.CateID);
            TestHelper.AddEnrollment(context, participant2.PNumber, savedEvent.EventID, category.CateID);
            var controller = new EnrollmentsController(context);
            TestHelper.SetUser(controller, participant1.PNumber, "Participant");

            var result = await controller.GetMyEnrollments();

            Assert.Single(result.Value);
            Assert.Equal(participant1.PNumber, result.Value.Single().PNumber);
        }

        [Fact]
        public async Task GetEnrollmentsForMyEvents_ReturnsOnlyEnrolmentsForTheOrganisersEvents()
        {
            var context = TestHelper.CreateContext();
            var event1 = TestHelper.AddEvent(context, 1, "Event One");
            var event2 = TestHelper.AddEvent(context, 2, "Event Two");
            var category1 = TestHelper.AddCategory(context, event1.EventID);
            var category2 = TestHelper.AddCategory(context, event2.EventID);
            var participant = TestHelper.AddParticipant(context);
            TestHelper.AddEnrollment(context, participant.PNumber, event1.EventID, category1.CateID);
            TestHelper.AddEnrollment(context, participant.PNumber, event2.EventID, category2.CateID);
            var controller = new EnrollmentsController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.GetEnrollmentsForMyEvents();

            Assert.Single(result.Value);
            Assert.Equal(event1.EventID, result.Value.Single().EventID);
        }

        [Fact]
        public async Task GetEnrollmentsForEvent_OtherOrganiser_ReturnsForbid()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var controller = new EnrollmentsController(context);
            TestHelper.SetUser(controller, 2, "Organiser");

            var result = await controller.GetEnrollmentsForEvent(savedEvent.EventID);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetEnrollment_TheParticipantWhoEnrolled_ReturnsIt()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var category = TestHelper.AddCategory(context, savedEvent.EventID);
            var participant = TestHelper.AddParticipant(context);
            var enrollment = TestHelper.AddEnrollment(context, participant.PNumber, savedEvent.EventID, category.CateID);
            var controller = new EnrollmentsController(context);
            TestHelper.SetUser(controller, participant.PNumber, "Participant");

            var result = await controller.GetEnrollment(enrollment.EnrollID);

            Assert.Equal(enrollment.EnrollID, result.Value.EnrollID);
        }

        [Fact]
        public async Task GetEnrollment_AnotherParticipant_ReturnsForbid()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var category = TestHelper.AddCategory(context, savedEvent.EventID);
            var participant1 = TestHelper.AddParticipant(context, "p1@test.com");
            var participant2 = TestHelper.AddParticipant(context, "p2@test.com");
            var enrollment = TestHelper.AddEnrollment(context, participant1.PNumber, savedEvent.EventID, category.CateID);
            var controller = new EnrollmentsController(context);
            TestHelper.SetUser(controller, participant2.PNumber, "Participant");

            var result = await controller.GetEnrollment(enrollment.EnrollID);

            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetEnrollment_OrganiserOfTheEvent_ReturnsIt()
        {
            var context = TestHelper.CreateContext();
            var savedEvent = TestHelper.AddEvent(context, 1);
            var category = TestHelper.AddCategory(context, savedEvent.EventID);
            var participant = TestHelper.AddParticipant(context);
            var enrollment = TestHelper.AddEnrollment(context, participant.PNumber, savedEvent.EventID, category.CateID);
            var controller = new EnrollmentsController(context);
            TestHelper.SetUser(controller, 1, "Organiser");

            var result = await controller.GetEnrollment(enrollment.EnrollID);

            Assert.Equal(enrollment.EnrollID, result.Value.EnrollID);
        }
    }
}