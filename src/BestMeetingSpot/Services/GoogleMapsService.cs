using BestMeetingSpot.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

#pragma warning disable CS1701 // Assuming assembly reference matches identity

namespace BestMeetingSpot.Services
{
    public class GoogleMapsService : IGoogleMapsService
	{			
		private string key = ""; // Your key goes here.
		private IConfigurationRoot config;

		public GoogleMapsService()
		{
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
				var uriFinal = $"{uriDestinations}&origins={Uri.EscapeDataString(gA.Latitude.ToString())},{Uri.EscapeDataString(gA.Longitude.ToString())}|{Uri.EscapeDataString(gB.Latitude.ToString())},{Uri.EscapeDataString(gB.Longitude.ToString())}&mode={meansA}&key={key}";
				var request = WebRequest.Create(new Uri(uriFinal));
				var response = await request.GetResponseAsync();
				var stream = response.GetResponseStream();
				var sr = new StreamReader(stream);
				var json = await sr.ReadToEndAsync();
				var model = JsonConvert.DeserializeObject<DistanceApiResults>(json);
				return model;
			}
			else
			{
				var uriFinalA = $"{uriDestinations}&origins={Uri.EscapeDataString(gA.Latitude.ToString())},{Uri.EscapeDataString(gA.Longitude.ToString())}&mode={meansA}&key={key}";
				var request = WebRequest.Create(new Uri(uriFinalA));
				var response = await request.GetResponseAsync();
				var stream = response.GetResponseStream();
				var sr = new StreamReader(stream);
				var json = await sr.ReadToEndAsync();
				var modelA = JsonConvert.DeserializeObject<DistanceApiResults>(json);

				var uriFinalB = $"{uriDestinations}&origins={Uri.EscapeDataString(gB.Latitude.ToString())},{Uri.EscapeDataString(gB.Longitude.ToString())}&mode={meansB}&key={key}";
				var requestB = WebRequest.Create(new Uri(uriFinalA));
				var responseB = await requestB.GetResponseAsync();
				var streamB = responseB.GetResponseStream();
				var srB = new StreamReader(streamB);
				var jsonB = await srB.ReadToEndAsync();
				var modelb = JsonConvert.DeserializeObject<DistanceApiResults>(jsonB);
				modelA.rows.Add(modelb.rows.First());
				return modelA;
			}
		}

		private async Task<List<Place>> GetPlacesByPoint(Geopoint midPoint)
		{
			var uri = new Uri($"https://maps.googleapis.com/maps/api/place/nearbysearch/json?location={Uri.EscapeDataString(midPoint.Latitude.ToString())},{Uri.EscapeDataString(midPoint.Longitude.ToString())}&radius=1000&types=bar&key={key}");
			var request = WebRequest.Create(uri);
			var response = await request.GetResponseAsync();
			var stream = response.GetResponseStream();
			var sr = new StreamReader(stream);
			var json = await sr.ReadToEndAsync();
			var model = JsonConvert.DeserializeObject<PlacesApiResults>(json);

			//if (!string.IsNullOrEmpty(model.next_page_token))
			//{
			//	await GetMoreResults(model);
			//}

			return model.results;
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
			var request = WebRequest.Create(requestUri);
			var response = await request.GetResponseAsync();
			var stream = response.GetResponseStream();
			var sr = new StreamReader(stream);
			var json = await sr.ReadToEndAsync();
			var model = JsonConvert.DeserializeObject<GeocodingApiResults>(json);
			

			return new Geopoint(model.results.First().geometry.location.lat, model.results.First().geometry.location.lng);
		}

		#region TBC

		//private async Task GetMoreResults(PlacesApiResults model)
		//{
		//	var uri = new Uri($"https://maps.googleapis.com/maps/api/place/nearbysearch/json?pagetoken={model.next_page_token}&key={key}");
		//	var request = WebRequest.Create(uri);
		//	var response = await request.GetResponseAsync();
		//	var stream = response.GetResponseStream();
		//	var sr = new StreamReader(stream);
		//	var json = await sr.ReadToEndAsync();
		//	var nextModel = JsonConvert.DeserializeObject<PlacesApiResults>(json);

		//	model.results.AddRange(nextModel.results);

		//	if (!string.IsNullOrEmpty(model.next_page_token))
		//	{
		//		await GetMoreResults(model);
		//	}
		//}
		#endregion
	}
}
