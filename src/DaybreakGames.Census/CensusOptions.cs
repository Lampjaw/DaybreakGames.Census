namespace DaybreakGames.Census
{
    public class CensusOptions
    {
        public string CensusServiceId { get; set; } = Constants.DefaultServiceId;
        public string CensusServiceNamespace { get; set; } = Constants.DefaultServiceNamespace;
        public string CensusApiEndpoint { get; set; } = Constants.CensusEndpoint;
        public string CensusWebsocketEndpoint { get; set; } = Constants.CensusWebsocketEndpoint;
        public bool LogCensusErrors { get; set; } = false;
    }
}
