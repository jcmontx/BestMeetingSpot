using BestMeetingSpot.Model;

namespace BestMeetingSpot.Services
{
	internal class Heuristics
	{
		public int ADistance { get; internal set; }
		public int ATime { get; internal set; }
		public int BDistance { get; internal set; }
		public int BTime { get; internal set; }
		public Place Place { get; internal set; }
	}
}