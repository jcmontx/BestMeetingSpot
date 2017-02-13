using System;
using Microsoft.AspNetCore.Mvc;
using BestMeetingSpot.Services;
using Swashbuckle.SwaggerGen.Annotations;
using BestMeetingSpot.Model;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace BestMeetingSpot.Controllers
{
    [Route("[controller]")]
	public class GeoController : Controller
    {
		private IGoogleMapsService service;

		public GeoController(IGoogleMapsService service)
		{
			this.service = service;
		}

		[Produces(typeof(Tuple<Place,Place>))]
		[SwaggerResponse(System.Net.HttpStatusCode.OK, Type = typeof(Tuple<Place, Place>))]
		[HttpGet]
        public IActionResult BestMatch([FromQuery]string addressA, [FromQuery]string meansA, [FromQuery]string meansB, [FromQuery]string addressB)
        {
			return new ObjectResult(service.GetBestMatch(addressA, meansA, addressB, meansB));
        }
    }
}
