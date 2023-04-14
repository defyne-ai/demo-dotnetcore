using Newtonsoft.Json;
using SmhiWeather;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace app.Commands
{
    public static class GetWeatherSelfImplCommand
    {

        /// <summary>
        /// Gets the complete SMHI forecast for the days ahead.
        /// </summary>
        /// <returns>A forecast for the days ahead.</returns>
        /// 

        private static Forecast GetForecast(decimal coordLat, decimal coordLon)
        {
            string lat = coordLat.ToString("0.00").Replace(",", ".");
            string lon = coordLon.ToString("0.00").Replace(",", ".");
            string uri = $"http://opendata-download-metfcst.smhi.se/api/category/pmp3g/version/2/geotype/point/lon/{lon}/lat/{lat}/data.json";
            HttpWebRequest webRequest = WebRequest.CreateHttp(uri);

            using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
            using (var reader = new StreamReader(webResponse.GetResponseStream()))
            {
                //JavaScriptSerializer js = new JavaScriptSerializer();
                string sJson = reader.ReadToEnd();
                //var forecast = (Forecast)js.Deserialize(sJson, typeof(Forecast));

                Forecast forecast = JsonConvert.DeserializeObject<Forecast>(sJson);

                return forecast;
            }
        }

        public static string Execute(decimal coordLat, decimal coordLon)
        {
            DateTime utcNow = DateTime.UtcNow;
            Forecast forecast = GetForecast(coordLat, coordLon);

            foreach (var timeSerie in forecast.timeseries.OrderBy(ts => ts.validTime))
            {
                var universalTime = timeSerie.validTime.ToUniversalTime();
                var localTime = timeSerie.validTime.ToLocalTime();

                if (universalTime.AddMinutes(30) > utcNow)
                {
                    return timeSerie.Temperature.ToString();
                }
            }

            return null;
        }
    }
}
