using Newtonsoft.Json;
using WeatherApp.Models;
using WeatherApp.Helpers;

namespace WeatherApp.Helpers
{
    
    public class FetchApi
    {
        public async Task<List<WeatherData>> FetchWeatherData()
        {
            string[] cities = { "Riga", "Rezekne", "Barcelona", "Madrid", "Oslo", "Bergen", "Amsterdam", "Utrecht", "Aveiro", "Lisbon"};

            List<WeatherData> weatherDataList = new List<WeatherData>();

            foreach (var city in cities)
            {
                var client = new HttpClient();

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://weatherapi-com.p.rapidapi.com/current.json?q={city}"),
                    Headers =
                    {
                        { "X-RapidAPI-Key", Secrets.ApiKey },
                        { "X-RapidAPI-Host", "weatherapi-com.p.rapidapi.com" },
                    },
                };
                using (var response = await client.SendAsync(request))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync();
                    dynamic responseContent = JsonConvert.DeserializeObject(body);

                    // would be possible to do deserialization of response in a more efficient way, but since I need just some properties from response
                    // I did not do that

                    WeatherData weatherData = new WeatherData()
                    {
                        Country = responseContent.location.country,
                        City = responseContent.location.name,
                        TemperatureC = responseContent.current.temp_c,
                        Clouds = responseContent.current.condition.text,
                        WindSpeed = responseContent.current.wind_kph
                    };

                    weatherDataList.Add(weatherData);
                }
            }

            return weatherDataList;
        }
    }
}
