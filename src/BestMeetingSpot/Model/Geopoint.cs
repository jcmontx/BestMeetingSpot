using System;

namespace BestMeetingSpot.Services
{
	public class Geopoint
	{
		private double lat;
		private double lng;

		public double Latitude
		{
			get
			{
				return lat;
			}

			set
			{
				lat = value;
			}
		}

		public double Longitude
		{
			get
			{
				return lng;
			}

			set
			{
				lng = value;
			}
		}

		public Geopoint(string lat, string lng)
		{
			Latitude = Convert.ToDouble(lat);
			Longitude = Convert.ToDouble(lng);
		}
		public Geopoint(double lat, double lng)
		{
			Latitude = lat;
			Longitude = lng;
		}
		public Geopoint()
		{

		}
		
		public static double DegreeToRadian(double angle)
		{
			return Math.PI * angle / 180.0;
		}

		public static double RadianToDegree(double angle)
		{
			return angle * (180.0 / Math.PI);
		}
	}
}