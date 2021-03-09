using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web;

namespace Program5.Controllers
{
    internal class CityHelper
    {
        const string WeatherAPIKey = "5133f20d56bcc5d175d1b94100c9578b";
        const int MaxRetries = 2;

        public static string GetCityInfo(string cityName)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(cityName))
            {
                return PrintBadArgs(cityName);
            }

            StringBuilder result = new StringBuilder();
            var weatherUriStr = "http://api.openweathermap.org/data/2.5/weather?q=" + cityName + "&appid=" + WeatherAPIKey;
            var weatherInfo = GetApiResponse<WeatherInfo>(new Uri(weatherUriStr));
            float kelvinTemp = weatherInfo.main.temp;
            int temp = (int)((kelvinTemp - 275.15) * 1.8 + 32);
            result.AppendLine("Current temperature in " + cityName + " is " + temp + " degrees.");
            if (temp < 40)
            {
                result.AppendLine("Wear winter jacket!");
            }
            else if (temp < 60)
            {
                result.AppendLine("Wear a light jacket!");
            }
            else
            {
                result.AppendLine("Enjoy the nice weather!");
            }

            var cityUrlStr = "http://geodb-free-service.wirefreethought.com/v1/geo/cities?namePrefix=" + cityName;
            var cityInfo = GetApiResponse<CityInfo>(new Uri(cityUrlStr));
            if (cityInfo.data == null || cityInfo.data.Length == 0)
            {
                return PrintBadArgs(cityName);
            }

            var city = cityInfo.data[0];
            result.AppendLine("City of " + cityName + " is located in " + city.country + " " + city.region + " at coordinates: latitude: " + city.latitude + " longtitude: " + city.longitude);
            return result.ToString();
        }

        static string PrintBadArgs(string cityName)
        {
            return "Invalid city [" + cityName + "]! Retry with a valid city name to get travel information.";
        }

        static T GetApiResponse<T>(Uri url)
        {
            int currentRetry = 0;
            double backofSeconds = 1;
            while (currentRetry <= MaxRetries)
            {
                try
                {
                    try
                    {
                        using (var client = new HttpClient(new HttpClientHandler()))
                        {
                            HttpResponseMessage response = client.GetAsync(url).Result;

                            response.EnsureSuccessStatusCode();
                            var currentPageHtml = response.Content.ReadAsStringAsync().Result;
                            var result = JsonConvert.DeserializeObject<T>(currentPageHtml);
                            return result;
                        }
                    }
                    catch (AggregateException e)
                    {
                        throw e.InnerExceptions[0];
                    }
                }
                catch (Exception e)
                {
                    if (currentRetry == MaxRetries)
                    {
                        throw new Exception("Max retry reached. Last error: " + e.Message);
                    }

                    // First time, this waits for 1 second. It doubles for every subsequent failure, up to the max.
                    var expBackoffSeconds = backofSeconds * Math.Pow(2, currentRetry);
                    Console.WriteLine("Error while accessing " + url.OriginalString + ". Error: " + e.Message + ". Going to retry after " + expBackoffSeconds + " seconds. Retries left: " + (MaxRetries - currentRetry));
                    Thread.Sleep(TimeSpan.FromSeconds(expBackoffSeconds));
                    currentRetry++;
                }
            }

            // It should never reach here, but compiler wants me to return something.
            return default(T);
        }


        public class WeatherInfo
        {
            public Coord coord { get; set; }
            public Weather[] weather { get; set; }
            public string _base { get; set; }
            public Main main { get; set; }
            public int visibility { get; set; }
            public Wind wind { get; set; }
            public Clouds clouds { get; set; }
            public int dt { get; set; }
            public Sys sys { get; set; }
            public int timezone { get; set; }
            public int id { get; set; }
            public string name { get; set; }
            public int cod { get; set; }
        }

        public class Coord
        {
            public float lon { get; set; }
            public float lat { get; set; }
        }

        public class Main
        {
            public float temp { get; set; }
            public float feels_like { get; set; }
            public float temp_min { get; set; }
            public float temp_max { get; set; }
            public int pressure { get; set; }
            public int humidity { get; set; }
        }

        public class Wind
        {
            public float speed { get; set; }
            public int deg { get; set; }
        }

        public class Clouds
        {
            public int all { get; set; }
        }

        public class Sys
        {
            public int type { get; set; }
            public int id { get; set; }
            public string country { get; set; }
            public int sunrise { get; set; }
            public int sunset { get; set; }
        }

        public class Weather
        {
            public int id { get; set; }
            public string main { get; set; }
            public string description { get; set; }
            public string icon { get; set; }
        }

        // Classes autogenerated by pasting JSON from Geodb API for city coordinates.

        public class CityInfo
        {
            public Datum[] data { get; set; }
            public Metadata metadata { get; set; }
        }

        public class Metadata
        {
            public int currentOffset { get; set; }
            public int totalCount { get; set; }
        }

        public class Datum
        {
            public int id { get; set; }
            public string wikiDataId { get; set; }
            public string type { get; set; }
            public string city { get; set; }
            public string name { get; set; }
            public string country { get; set; }
            public string countryCode { get; set; }
            public string region { get; set; }
            public string regionCode { get; set; }
            public float latitude { get; set; }
            public float longitude { get; set; }
        }

    }
}