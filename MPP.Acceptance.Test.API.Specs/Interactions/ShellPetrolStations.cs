using Boa.Constrictor.RestSharp;
using Boa.Constrictor.Screenplay;
using MPP.Acceptance.Test.API.Specs.Drivers;
using MPP.Acceptance.Test.API.Specs.Model;
using MPP.Acceptance.Test.API.Specs.Support;
using RestSharp;

namespace MPP.Acceptance.Test.API.Specs.Interactions
{
    public class ShellPetrolStations : IQuestion<IRestResponse>
    {
        private readonly ShellStationRowObject _shellStationRowObject;

        private ShellPetrolStations(ShellStationRowObject shellStationRowObject)
        {
            _shellStationRowObject = shellStationRowObject;
        }

        public IRestResponse RequestAs(IActor actor)
        {
            var response = actor.Calls(Rest.Request(StationLocatorFromRowObjectToRestRequest((MPPActor) actor,
                _shellStationRowObject)));

            ((MPPActor) actor).LogLastRequestAndResponse();

            return response;
        }

        public static ShellPetrolStations UsingQueryParameters(ShellStationRowObject shellStationRowObject)
        {
            return new(shellStationRowObject);
        }

        private static RestRequest StationLocatorFromRowObjectToRestRequest(IMPPActor actor,
            ShellStationRowObject shellStation)
        {
            var restRequest = new RestRequest(Endpoint.StationLocator.ToString(), Method.GET);
            restRequest.AddHeader(ContentType.ContentTypeName, ContentType.Json.ToString());

            if (shellStation.Lat != null)
                restRequest.AddQueryParameter("lat",
                    actor.GeTestContextDriver().GetTestContext.ReplaceDynamicContentInString(shellStation.Lat));

            if (shellStation.Lon != null)
                restRequest.AddQueryParameter("lon",
                    actor.GeTestContextDriver().GetTestContext.ReplaceDynamicContentInString(shellStation.Lon));

            if (shellStation.Radius != null)
                restRequest.AddQueryParameter("radius",
                    actor.GeTestContextDriver().GetTestContext.ReplaceDynamicContentInString(shellStation.Radius));

            if (shellStation.CountryCode != null)
                restRequest.AddQueryParameter("countryCode",
                    actor.GeTestContextDriver().GetTestContext.ReplaceDynamicContentInString(shellStation.CountryCode));

            if (shellStation.UnmannedOnly != null)
                restRequest.AddQueryParameter("unmannedOnly",
                    actor.GeTestContextDriver().GetTestContext
                        .ReplaceDynamicContentInString(shellStation.UnmannedOnly));

            if (shellStation.TruckOnly != null)
                restRequest.AddQueryParameter("truckOnly",
                    actor.GeTestContextDriver().GetTestContext.ReplaceDynamicContentInString(shellStation.TruckOnly));

            return restRequest;
        }
    }
}