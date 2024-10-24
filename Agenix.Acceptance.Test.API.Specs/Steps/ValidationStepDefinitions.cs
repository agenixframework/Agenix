using System.Linq;
using FleetPay.Acceptance.Test.API.Specs.Drivers;
using FleetPay.Acceptance.Test.API.Specs.Support;
using FleetPay.Core.Validation;
using FleetPay.Core.Validation.Json.Dsl;
using FluentAssertions;
using NUnit.Framework;
using RestSharp;
using TechTalk.SpecFlow;

namespace FleetPay.Acceptance.Test.API.Specs.Steps
{
    [Binding]
    public sealed class ValidationStepDefinitions
    {
        private readonly IFleetPayActor _actor;

        public ValidationStepDefinitions(IFleetPayActor actor)
        {
            _actor = actor;
        }

        [Then(@"the values are extracted into the variables from the JSON response body:")]
        public void ThenTheValuesAreExtractedIntoVariablesFromTheJsonResponseBody(Table dataTable)
        {
            var datTableRows = dataTable.Rows.ToDictionary(r => r[0], r => r[1]);

            CollectionAssert.IsNotEmpty(datTableRows, "The table rows are empty.");

            var response = _actor.Recall<IRestResponse>(InMemory.CURRENT_RECEIVED_RESPONSES);
            Assert.IsNotNull(response, "The last received http response is null!");
            Assert.IsNotNull(response.Content, "The http response body is null!");
            Assert.IsNotEmpty(response.Content, "The http response body is empty!");

            JsonSupport.Json()
                .JsonPath()
                .Extract()
                .Expressions(datTableRows)
                .Build()
                .ExtractVariables(response.Content, _actor.GeTestContextDriver.GetTestContext);
        }

        [Then(@"the values are extracted into the variables from the JSON response headers:")]
        public void ThenTheValuesAreExtractedIntoVariablesFromTheJsonResponseHeaders(Table dataTable)
        {
            var headersWithAssociatedVariables = dataTable.Rows.ToDictionary(r => r[0], r => r[1]);

            CollectionAssert.IsNotEmpty(headersWithAssociatedVariables, "The table rows are empty.");

            var response = _actor.Recall<IRestResponse>(InMemory.CURRENT_RECEIVED_RESPONSES);
            Assert.IsNotNull(response, "The last received http response is null!");
            Assert.IsNotNull(response.Headers, "The http headers body is null!");
            Assert.IsNotEmpty(response.Headers, "The http headers body is empty!");

            var actualResponseHeaders = response.Headers
                .Where(parameter => parameter.Value != null)
                .ToDictionary(parameter => parameter.Name ?? string.Empty, parameter => parameter.Value.ToString());


            new HeaderVariableExtractor.Builder()
                .Headers(headersWithAssociatedVariables)
                .Build()
                .ExtractVariables(actualResponseHeaders, _actor.GeTestContextDriver.GetTestContext);
        }

        [Then(@"the JSON response headers are validated using matchers:")]
        public void ThenTheJsonResponseHeadersAreValidatedUsingMatchers(Table dataTable)
        {
            var headersWithAssociatedVariables = dataTable.Rows.ToDictionary(r => r[0], r => r[1]);

            CollectionAssert.IsNotEmpty(headersWithAssociatedVariables, "The table rows are empty.");

            var response = _actor.Recall<IRestResponse>(InMemory.CURRENT_RECEIVED_RESPONSES);
            Assert.IsNotNull(response, "The last received http response is null!");
            Assert.IsNotNull(response.Headers, "The http headers body is null!");
            Assert.IsNotEmpty(response.Headers, "The http headers body is empty!");

            var actualResponseHeaders = response.Headers
                .Where(parameter => parameter.Value != null)
                .ToDictionary(parameter => parameter.Name ?? string.Empty, parameter => parameter.Value.ToString());

            foreach (var (key, value) in headersWithAssociatedVariables)
                new DefaultHeaderValidator()
                    .ValidateHeader(key, actualResponseHeaders[key], value, _actor.GeTestContextDriver.GetTestContext);
        }

        [Then(@"the JSON response body is validated using the json path expressions? with associated matchers?:")]
        public void ThenTheJsonResponseIsValidatedUsingTheJsonPathExpressionsWithAssociatedMatchers(Table dataTable)
        {
            var datTableRows = dataTable.Rows.ToDictionary(r => r[0], r => r[1]);

            CollectionAssert.IsNotEmpty(datTableRows, "The table rows are empty.");

            var response = _actor.Recall<IRestResponse>(InMemory.CURRENT_RECEIVED_RESPONSES);
            Assert.IsNotNull(response, "The last received http response is null!");
            Assert.IsNotNull(response.Content, "The http response body is null!");
            Assert.IsNotEmpty(response.Content, "The http response body is empty!");

            JsonSupport.Json()
                .JsonPath()
                .Validate()
                .Expressions(datTableRows)
                .Build()
                .Validate(response.Content, _actor.GeTestContextDriver.GetTestContext);
        }

        [Then(@"the response status should be ""(\d+)""")]
        public void ThenTheResponseStatusShouldBe(int httpResponseCode)
        {
            var response = _actor.Recall<IRestResponse>(InMemory.CURRENT_RECEIVED_RESPONSES);
            Assert.IsNotNull(response, "The last received http response is null!");

            response.StatusCode.Should().Be(httpResponseCode);
        }
    }
}