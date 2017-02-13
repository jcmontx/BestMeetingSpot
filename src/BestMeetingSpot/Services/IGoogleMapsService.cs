using BestMeetingSpot.Model;
using System;
using System.Threading.Tasks;

namespace BestMeetingSpot.Services
{
	public interface IGoogleMapsService
	{
		Task<Geopoint> GetGeopoint(string address);
		Task<Tuple<Place, Place>> GetBestMatch(string addressA, string meansA, string addressB, string meansB);
	}
}