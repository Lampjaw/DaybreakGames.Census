using System.Collections.Generic;

namespace DaybreakGames.Census.Stream
{
    public class CensusStreamSubscription
    {
        public IEnumerable<string> Characters { get; set; }
        public IEnumerable<string> Worlds { get; set; }
        public IEnumerable<string> EventNames { get; set; }
    }
}
