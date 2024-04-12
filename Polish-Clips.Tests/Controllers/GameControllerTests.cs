using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using FakeItEasy;
using Polish_Clips.Controllers;
using Polish_Clips.Services.GameService;
using Microsoft.AspNetCore.Mvc;
using Polish_Clips.Dtos.Game;
using Polish_Clips.Dtos.Helpers;
using Polish_Clips.Models;

namespace Polish_Clips.Tests.Controllers
{
    public class GameControllerTests
    {
        private readonly IGameService _gameService;
        private readonly GameController _controller;

        public GameControllerTests()
        {
            _gameService = A.Fake<IGameService>();
            _controller = new GameController(_gameService);
        }

        [Fact]
        public async Task GameController_GetGames_ReturnOkAndValidResponse()
        {
            // Arrange
            var query = new QueryObject();

            var responseData = new List<GetGameDto>
            {
                new GetGameDto { Id = 1, Name = "string", ArtUrl = "string" },
                new GetGameDto { Id = 2, Name = "string2", ArtUrl = "string2" },
                new GetGameDto { Id = 3, Name = "string3", ArtUrl = "string3" },
            };
            A.CallTo(() => _gameService.GetGames(query)).Returns(Task.FromResult(new ServiceResponse<List<GetGameDto>> { Data = responseData, Success = true, Message = "" }));

            // Act
            var actionResult = await _controller.GetGames(query);

            // Assert
            actionResult.Should().BeOfType<ActionResult<ServiceResponse<List<GetGameDto>>>>();
            var okObjectResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
            var serviceResponse = okObjectResult.Value.Should().BeOfType<ServiceResponse<List<GetGameDto>>>().Subject;
            serviceResponse.Data.Should().NotBeNull().And.BeEquivalentTo(responseData);
            serviceResponse.Success.Should().BeTrue();
        }
    }
}
