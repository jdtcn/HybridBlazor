using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using HybridBlazor.Shared;
using HybridBlazor.Shared.Services;

namespace HybridBlazor.Server.Services
{
    [Authorize]
    public class WeatherForecastService : IWeatherForecastService
    {
        private readonly ILogger<WeatherForecastService> logger;

        public WeatherForecastService(ILogger<WeatherForecastService> logger)
        {
            this.logger = logger;
        }

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public Task<List<WeatherForecast>> GetForecastAsync()
        {
            logger.LogInformation("GetForecastAsync");
            var rng = new Random();
            return Task.FromResult(Enumerable.Range(1, 10).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.Date.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }).ToList());
        }
    }
}
