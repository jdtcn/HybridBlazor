using System.ServiceModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HybridBlazor.Shared.Services
{
    [ServiceContract]
    public interface IWeatherForecastService
    {
        Task<List<WeatherForecast>> GetForecastAsync();
    }
}
