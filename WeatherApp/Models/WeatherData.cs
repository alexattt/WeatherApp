
using System.ComponentModel.DataAnnotations;

namespace WeatherApp.Models
{
    public class WeatherData
    {
        [Key]
        public int Id { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public double TemperatureC { get; set; }
        public string Clouds { get; set; }
        public double WindSpeed { get; set; }
        public DateTime CreateTime { get; set; } = DateTime.Parse(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
    }
}
