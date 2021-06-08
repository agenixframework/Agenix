Feature: Retrieve Shell Petrol Stations
	Retrieve the list of tank/ petrol stations from Station Locator Service.

@shell-api-stub
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
		| JsonPathExpression                                 | MatcherName                                                                                                                                 |
		| $.[?(@.fullStationIdentifier == '1')].id           | @EqualsTo(12170818)@                                                                                                                        |
		| $.[?(@.fullStationIdentifier == '1')].lat          | @StartsWith(52)@                                                                                                                            |
		| $.[?(@.fullStationIdentifier == '1')].lon          | @StartsWith(13)@                                                                                                                            |
		| $.[?(@.fullStationIdentifier == '1')].name         | @EqualsTo(Zum Biotop)@                                                                                                                      |
		| $.[?(@.fullStationIdentifier == '1')].telephone    | @EqualsTo(9611199089)@                                                                                                                      |
		| $.[?(@.fullStationIdentifier == '1')].address      | @EqualsTo(Zum Biotop 19, 50127 Bergheim, Germany)@                                                                                          |
		| $.[?(@.fullStationIdentifier == '1')].fuelTypes    | @HasItemsInAnyOrder('CNG','DieselFit','Fuelsave Midgrade Gasoline','Kerosene')@                                                             |
		| $.[?(@.fullStationIdentifier == '1')].amenities    | @HasItemsInAnyOrder('Air & Water','Credit card - American Express','Disabled Facilities','Shop','Unamanned','Mobile Payment','Truck Only')@ |
		| $.[?(@.fullStationIdentifier == '1')].openingHours | @HasItemsInAnyOrder('Mon-Fri 10:00-19:00','Sat-Sun 12:00-17:00')@                                                                           |