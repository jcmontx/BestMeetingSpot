using BestMeetingSpot.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

#pragma warning disable CS1701 // Assuming assembly reference matches identity

namespace BestMeetingSpot.Services
{
    public class GoogleMapsService : IGoogleMapsService
	{			
		private string key = ""; // Your key goes here.
		private HttpClient client;

		public GoogleMapsService()
		{
			client = new HttpClient();
		}

		public async Task<Tuple<Place,Place>> GetBestMatch(string addressA, string meanA, string addressB, string meanB)
		{
			var gA = await GetGeopoint(addressA);
			var gB = await GetGeopoint(addressB);

			var midPoint = GetMidPoint(gA, gB);
			var places = await GetPlacesByPoint(midPoint);

			var distanceMatrix = await GetDistanceMatrix(gA,meanA,gB,meanB,places);

			List<Heuristics> hs = new List<Heuristics>();
			for (int i = 0; i < distanceMatrix.rows[0].elements.Count; i++)
			{
				var h = new Heuristics();
				h.Place = places[i];
				h.ATime = distanceMatrix.rows[0].elements[i].duration.value;
				h.BTime = distanceMatrix.rows[1].elements[i].duration.value;
				h.ADistance =  distanceMatrix.rows[0].elements[i].distance.value;
				h.BDistance =  distanceMatrix.rows[1].elements[i].distance.value;
				hs.Add(h);
			}
			var bestMatchTime = hs.OrderBy(x => x.ATime + x.BTime).First().Place;
			var bestMatchDistance = hs.OrderBy(x => x.ADistance + x.BDistance).First().Place;

			return new Tuple<Place,Place>(bestMatchTime,bestMatchDistance);
		}

		private async Task<DistanceApiResults> GetDistanceMatrix(Geopoint gA, string meansA, Geopoint gB, string meansB, List<Place> places)
		{
			var baseUri = "https://maps.googleapis.com/maps/api/distancematrix/json?";
			var uriDestinations = $"{baseUri}destinations=";
			foreach (var place in places)
			{
				uriDestinations = $"{uriDestinations}{Uri.EscapeDataString(place.geometry.location.lat.ToString())},{Uri.EscapeDataString(place.geometry.location.lng.ToString())}";
				if (places.IndexOf(place) != places.Count - 1)
					uriDestinations = $"{uriDestinations}|";
			}


			if (meansA == meansB) //if the means of transport are the same just make one request
			{
				var uriFinal = new Uri($"{uriDestinations}&origins={Uri.EscapeDataString(gA.Latitude.ToString())},{Uri.EscapeDataString(gA.Longitude.ToString())}|{Uri.EscapeDataString(gB.Latitude.ToString())},{Uri.EscapeDataString(gB.Longitude.ToString())}&mode={meansA}&key={key}");
				var model = await GetResults<DistanceApiResults>(uriFinal);
				return model;
			}
			else
			{
				var uriFinalA = new Uri($"{uriDestinations}&origins={Uri.EscapeDataString(gA.Latitude.ToString())},{Uri.EscapeDataString(gA.Longitude.ToString())}&mode={meansA}&key={key}");
				var modelA = await GetResults<DistanceApiResults>(uriFinalA);				
				var uriFinalB = new Uri($"{uriDestinations}&origins={Uri.EscapeDataString(gB.Latitude.ToString())},{Uri.EscapeDataString(gB.Longitude.ToString())}&mode={meansB}&key={key}");
				var modelB = await GetResults<DistanceApiResults>(uriFinalB);
				modelA.rows.Add(modelB.rows.First());
				return modelA;
			}
		}

		private async Task<List<Place>> GetPlacesByPoint(Geopoint midPoint)
		{
			var uri = new Uri($"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={Uri.EscapeDataString(midPoint.Latitude.ToString())},{Uri.EscapeDataString(midPoint.Longitude.ToString())}&radius=1000&types=bar&key={key}");
			var model = await GetResults<PlacesApiResults>(uri);
			return model.results;
		}
		
		private async Task<T> GetResults<T>(Uri uri){
			var request = new HttpRequestMessage(HttpMethod.Get,uri);
			var response = await client.SendAsync(request);
			response.EnsureSuccessStatusCode();
			var stream = await response.Content.ReadAsStreamAsync();
			var sr = new StreamReader(stream);
			var json = await sr.ReadToEndAsync();
			return JsonConvert.DeserializeObject<T>(json);		
		}

		private Geopoint GetMidPoint(Geopoint gA, Geopoint gB)
		{
			Geopoint midPoint = new Geopoint();

			double dLon = Geopoint.DegreeToRadian(gB.Longitude - gA.Longitude);
			double Bx = Math.Cos(Geopoint.DegreeToRadian(gB.Latitude)) * Math.Cos(dLon);
			double By = Math.Cos(Geopoint.DegreeToRadian(gB.Latitude)) * Math.Sin(dLon);

			midPoint.Latitude = Geopoint.RadianToDegree(Math.Atan2(
						 Math.Sin(Geopoint.DegreeToRadian(gA.Latitude)) + Math.Sin(Geopoint.DegreeToRadian(gB.Latitude)),
						 Math.Sqrt(
							 (Math.Cos(Geopoint.DegreeToRadian(gA.Latitude)) + Bx) *
							 (Math.Cos(Geopoint.DegreeToRadian(gA.Latitude)) + Bx) + By * By)));

			midPoint.Longitude = gA.Longitude + Geopoint.RadianToDegree(Math.Atan2(By, Math.Cos(Geopoint.DegreeToRadian(gA.Latitude)) + Bx));

			return midPoint;

		}

		public async Task<Geopoint> GetGeopoint(string address)
		{
			var requestUri = new Uri($"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={key}");
			var model = await GetResults<GeocodingApiResults>(requestUri);
			return new Geopoint(model.results.First().geometry.location.lat, model.results.First().geometry.location.lng);
		}
	}
}
