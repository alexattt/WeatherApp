using System.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WeatherApp.Data;
using WeatherApp.Helpers;
using WeatherApp.Models;

namespace WeatherApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly AppDbContext _db;

        public HomeController(ILogger<HomeController> logger, AppDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            // to return last update time
            WeatherData weatherData = _db.WeatherData.OrderByDescending(x => x.CreateTime).FirstOrDefault();

            if (!_db.WeatherData.Any())
            {
                // fetching first time if no data is present
                await FetchApiData();
            }

            return View(weatherData);
        }

        // cities perhaps might be stored in different DB table, there could be form for adding new cities, deleting cities...

        public async Task FetchApiData()
        {
            FetchApi fetchApi = new FetchApi();
            IEnumerable<WeatherData> weatherDataList = await fetchApi.FetchWeatherData();
            await Create((List<WeatherData>)weatherDataList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<WeatherData> weatherData)
        {
            foreach (var obj in  weatherData)
            {
                if (ModelState.IsValid)
                {
                    _db.WeatherData.Add(obj);
                    _db.SaveChanges();
                }
            }

            return Ok();
        }

        [HttpPost]
        public async Task <BarChartData> GetBarChartData([FromBody] string type)
        {
            // IQueryable<WeatherData> result;
            List<IGrouping<string, WeatherData>> result;
            string[] labels = new string[] { };
            // double[] datasets = new double[] {};
            List<double> datasetsTest = new List<double> { };

            if (type == "Temperature")
            {
                // OLD version, turned out to be quite time consuming
                // result = _db.WeatherData.GroupBy(x => x.City)
                //     .Select(x => x.OrderBy(p => p.TemperatureC).First());

                // DataTable dt = new DataTable();
                // dt.Columns.AddRange(new DataColumn[] {new DataColumn("Location",typeof(string)),
                //     new DataColumn("Temperature",typeof(double)) });
                //
                // foreach (var item in result)
                // {
                //     dt.Rows.Add(item.Country + ", " + item.City, item.TemperatureC);
                // }
                //
                // labels = (from p in dt.AsEnumerable()
                //     select p.Field<string>("Location")).Distinct().ToArray();
                //
                // datasets = (from p in dt.AsEnumerable()
                //     select p.Field<Double>("Temperature")).ToArray();

                result = _db.WeatherData.ToList().GroupBy(x => x.City).ToList();

                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[] { new DataColumn("City", typeof(string)) });

                foreach (var item in result)
                {
                    dt.Rows.Add(item.Key);
                };

                labels = (from p in dt.AsEnumerable()
                    select p.Field<string>("City")).Distinct().ToArray();

                foreach (var distinctCity in result)
                {
                    CityData city = new CityData();
                    city.City = distinctCity.Key;

                    WeatherData wd = distinctCity.OrderBy(x => x.TemperatureC).FirstOrDefault();

                    datasetsTest.Add(wd.TemperatureC);
                }
            }
            if (type == "Wind")
            {
                result = _db.WeatherData.ToList().GroupBy(x => x.City).ToList();

                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[] { new DataColumn("City", typeof(string)) });

                foreach (var item in result)
                {
                    dt.Rows.Add(item.Key);
                };

                labels = (from p in dt.AsEnumerable()
                    select p.Field<string>("City")).Distinct().ToArray();

                foreach (var distinctCity in result)
                {
                    CityData city = new CityData();
                    city.City = distinctCity.Key;

                    WeatherData wd = distinctCity.OrderByDescending(x => x.WindSpeed).FirstOrDefault();

                    datasetsTest.Add(wd.WindSpeed);
                }
            }

            BarChartData chartData = new BarChartData();
            chartData.Labels = labels;
            chartData.Datasets = datasetsTest.ToArray();

            return chartData;
        }

        [HttpPost]
        public async Task<LineChartData> GetLineChartData([FromBody] string type)
        {
           IQueryable<WeatherData> result = _db.WeatherData.Where(x => x.CreateTime >= DateTime.Now.AddMinutes(-119)).OrderBy(x=> x.CreateTime);

           DataTable dt = new DataTable();
           dt.Columns.AddRange(new DataColumn[] { new DataColumn("CreateTime",typeof(string)) });

           foreach (var item in result)
           {
               dt.Rows.Add(item.CreateTime);
           };

           string[] labels = (from p in dt.AsEnumerable()
               select p.Field<string>("CreateTime")).Distinct().ToArray();


            List<IGrouping<string, WeatherData>> distinctCityData = result.ToList()
               .GroupBy(x => x.City)
               .ToList();

            List<CityData> dataset = new List<CityData>();

            foreach (var distinctCity in distinctCityData)
            {
                CityData city= new CityData();
                city.City = distinctCity.Key;

                List<double> numericData = new List<double> { };

                if (type == "Temperature")
                {
                    foreach (var cityData in distinctCity)
                    {
                        numericData.Add(cityData.TemperatureC);
                    }
                }

                if (type == "Wind")
                {
                    foreach (var cityData in distinctCity)
                    {
                        numericData.Add(cityData.WindSpeed);
                    }
                }

                city.Dataset = numericData.ToArray();

                dataset.Add(city);
            }

            LineChartData lineChartData = new LineChartData();
            lineChartData.Labels = labels;
            lineChartData.Datasets = dataset;

            return lineChartData;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}