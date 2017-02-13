﻿using System.Collections.Generic;

namespace BestMeetingSpot.Model
{
    public class AddressComponent
	{
		public string long_name { get; set; }
		public string short_name { get; set; }
		public List<string> types { get; set; }
	}

	public class Bounds
	{
		public Northeast northeast { get; set; }
		public Southwest southwest { get; set; }
	}

	public class Northeast2
	{
		public double lat { get; set; }
		public double lng { get; set; }
	}

	public class Southwest2
	{
		public double lat { get; set; }
		public double lng { get; set; }
	}
	
	public class Result
	{
		public List<AddressComponent> address_components { get; set; }
		public string formatted_address { get; set; }
		public Geometry geometry { get; set; }
		public string place_id { get; set; }
		public List<string> types { get; set; }
	}

	public class GeocodingApiResults
	{
		public List<Result> results { get; set; }
		public string status { get; set; }
	}
}
