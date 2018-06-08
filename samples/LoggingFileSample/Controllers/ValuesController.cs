using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LoggingFileSample.Controllers
{
    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        public ValuesController(ILogger<ValuesController> logger)
        {
            Logger = logger;
        }

        public ILogger Logger { get; }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            Logger.LogDebug("[GET] api/values");
            Logger.LogInformation("[GET] api/values");
            Task.Run(() => throw new Exception("Exception from another thread")).Wait();

            return new string[] { "value1", "value2" };
        }
    }
}
