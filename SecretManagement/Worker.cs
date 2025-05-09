using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecretManagement.Interfaces;

namespace SecretManagement
{
    public class Worker
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Worker> _logger;
        private readonly IService _service;

        public Worker(IConfiguration configuration, ILogger<Worker> logger, IService service)
        {
            _configuration = configuration;
            _logger = logger;
            _service = service;
        }

        public async Task Run()
        {
            _logger.LogInformation("Worker started.");
            await _service.Start();
            _service.Stop();
        }
    }
}