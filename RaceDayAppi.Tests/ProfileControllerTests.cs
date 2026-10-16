using Microsoft.AspNetCore.Mvc;
using RaceDayApi.Controllers;
using RaceDayApi.DTOs;

namespace RaceDayApi.Tests
{
    public class ProfileControllerTests
    {
        private UpdateProfileDto NewProfileDto(string email)
        {
            return new UpdateProfileDto
            {
                Name = "NewName",
                Surname = "NewSurname",
                Cell = "0799999999",
                Email = email
            };
        }

        [Fact]
        public async Task GetMyProfile_Organiser_ReturnsTheirProfile()
        {
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddOrganiser(context);
            var controller = new ProfileController(context);
            TestHelper.SetUser(controller, organiser.OrganiserCode, "Organiser");

            var result = await controller.GetMyProfile();

            Assert.Equal("Organiser", result.Value.Role);
            Assert.Equal("organiser@test.com", result.Value.Email);
        }

        [Fact]
        public async Task GetMyProfile_Participant_ReturnsTheirProfile()
        {
            var context = TestHelper.CreateContext();
            var participant = TestHelper.AddParticipant(context);
            var controller = new ProfileController(context);
            TestHelper.SetUser(controller, participant.PNumber, "Participant");

            var result = await controller.GetMyProfile();

            Assert.Equal("Participant", result.Value.Role);
            Assert.Equal("participant@test.com", result.Value.Email);
        }

        [Fact]
        public void ProfileDto_DoesNotExposeAPassword()
        {
            Assert.Null(typeof(ProfileDto).GetProperty("Password"));
            Assert.Null(typeof(ProfileDto).GetProperty("PasswordHash"));
        }

        [Fact]
        public async Task GetMyProfile_AccountDoesNotExist_ReturnsNotFound()
        {
            var context = TestHelper.CreateContext();
            var controller = new ProfileController(context);
            TestHelper.SetUser(controller, 999, "Organiser");

            var result = await controller.GetMyProfile();

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task UpdateMyProfile_Participant_UpdatesTheirDetails()
        {
            var context = TestHelper.CreateContext();
            var participant = TestHelper.AddParticipant(context);
            var controller = new ProfileController(context);
            TestHelper.SetUser(controller, participant.PNumber, "Participant");

            var result = await controller.UpdateMyProfile(NewProfileDto("new@test.com"));

            Assert.Equal("NewName", result.Value.Name);

            var saved = context.Participants.Single();
            Assert.Equal("NewSurname", saved.Surname);
            Assert.Equal("0799999999", saved.Cell);
            Assert.Equal("new@test.com", saved.Email);
        }

        [Fact]
        public async Task UpdateMyProfile_EmailOfAnotherAccount_ReturnsConflict()
        {
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddOrganiser(context, "org@test.com");
            TestHelper.AddParticipant(context, "taken@test.com");
            var controller = new ProfileController(context);
            TestHelper.SetUser(controller, organiser.OrganiserCode, "Organiser");

            var result = await controller.UpdateMyProfile(NewProfileDto("taken@test.com"));

            Assert.IsType<ConflictObjectResult>(result.Result);
            Assert.Equal("org@test.com", context.Organisers.Single().Email);
        }

        [Fact]
        public async Task UpdateMyProfile_KeepingOwnEmail_Succeeds()
        {
            var context = TestHelper.CreateContext();
            var participant = TestHelper.AddParticipant(context, "me@test.com");
            var controller = new ProfileController(context);
            TestHelper.SetUser(controller, participant.PNumber, "Participant");

            var result = await controller.UpdateMyProfile(NewProfileDto("me@test.com"));

            Assert.Null(result.Result);
            Assert.NotNull(result.Value);
        }

        [Fact]
        public async Task UpdateMyProfile_DoesNotChangeThePassword()
        {
            var context = TestHelper.CreateContext();
            var organiser = TestHelper.AddOrganiser(context);
            var controller = new ProfileController(context);
            TestHelper.SetUser(controller, organiser.OrganiserCode, "Organiser");

            await controller.UpdateMyProfile(NewProfileDto("changed@test.com"));

            Assert.Equal("original-hash", context.Organisers.Single().Password);
        }
    }
}