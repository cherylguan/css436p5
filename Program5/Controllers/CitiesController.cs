using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Web.Http;

namespace Program5.Controllers
{
    public class CitiesController : ApiController
    {
        // GET: api/Cities
        public IEnumerable<string> Get()
        {
            return new[] { "City name cannot be empty!" };
        }

        // GET: api/Cities/Seattle
        public string Get(string id)
        {
            return CityHelper.GetCityInfo(id);
        }
    }
}
