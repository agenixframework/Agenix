Feature: Retrieve Shell Petrol Stations
	Retrieve the list of tank/ petrol stations from Station Locator Service.

@shell-api-stub @ignore
Scenario: Retrieve Shell Petrol Stations by mandatory query parameters
	Given FleetPay mobile app attempts to retrieve the shell petrol stations over API using the query parameters:
		| Lat       | Lon       | Radius |
		| 52.533501 | 13.404813 | 0.3    |
	Then the response status should be "200"
	And the JSON response body is validated using the json path expressions with associated matchers:
		| JsonPathExpression                             | MatcherName                                                                                                                        |
		| $.Size()                                       | @GreaterThan(0)@                                                                                                                   |
		| $.[?(@.fullStationIdentifier == '1')].KeySet() | @HasItemsInAnyOrder('id','name','telephone','address','lat','lon','fuelTypes','amenities','openingHours','fullStationIdentifier')@ |
	And the JSON response body is validated using the json path expressions with associated matchers:
		| JsonPathExpression                       | MatcherName          |
		| $.[?(@.fullStationIdentifier == '1')].id | @EqualsTo(12170818)@ |