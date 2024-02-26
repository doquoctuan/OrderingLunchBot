using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderRice.Entities;
using OrderRice.Interfaces;
using System.Net;

namespace OrderRice.Functions
{
    public class UserFunction
    {
        private readonly ILogger<UserFunction> _logger;
        private readonly IUserService _userService;

        public UserFunction(ILogger<UserFunction> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        [Function(nameof(CreateUser))]
        public async Task<HttpResponseData> CreateUser([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData request)
        {
            _logger.LogInformation("Invoke function create user");
            var response = request.CreateResponse(HttpStatusCode.OK);
            await _userService.CreateUser(new Users
            {
                Id = Guid.NewGuid(),
                TelegramId = "123",
                UserName = "456",
                FullName = "789"
            });
            return response;
        }

    }
}
