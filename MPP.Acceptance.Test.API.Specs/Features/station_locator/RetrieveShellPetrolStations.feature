Feature: Retrieve Shell Petrol Stations
	Retrieve the list of tank/ petrol stations from Station Locator Service.

@shell-api-stub @ignore
Scenario: Retrieve Shell Petrol Stations by only mandatory query parameters
	Given FleetPay mobile app attempts to retrieve the shell petrol stations over API using the query parameters:
		| Lat       | Lon       | Radius |
		| 52.533501 | 13.404813 | 0.3    |
	Then the response status should be "200"
	And the JSON response body is validated using the json path expressions with associated matchers:
		| JsonPathExpression                 | MatcherName                                                                                                                                                   |
		| $.Size()                           | @EqualsTo(10)@                                                                                                                                                |
		| $.[?(@.id == '12170821')].KeySet() | @HasItemsInAnyOrder('id','name','telephone','address','lat','lon','fuelTypes','amenities','openingHours','fullStationIdentifier','isTruckOnly','isUnmanned')@ |
		| $.[?(@.id == '12170823')].KeySet() | @HasItemsInAnyOrder('id','name','telephone','address','lat','lon','fuelTypes','amenities','openingHours','fullStationIdentifier','isTruckOnly','isUnmanned')@ |
		| $.[?(@.id == '12170825')].KeySet() | @HasItemsInAnyOrder('id','name','telephone','address','lat','lon','fuelTypes','amenities','openingHours','fullStationIdentifier','isTruckOnly','isUnmanned')@ |
	And the JSON response body is validated using the json path expressions with associated matchers:
		| JsonPathExpression                              | MatcherName                                                               |
		| $.[?(@.id == '12170821')].id                    | @EqualsTo(12170821)@                                                      |
		| $.[?(@.id == '12170821')].fullStationIdentifier | @EqualsTo(4)@                                                             |
		| $.[?(@.id == '12170821')].lat                   | @StartsWith(53)@                                                          |
		| $.[?(@.id == '12170821')].lon                   | @StartsWith(12)@                                                          |
		| $.[?(@.id == '12170821')].name                  | @EqualsTo(Dortmund)@                                                      |
		| $.[?(@.id == '12170821')].telephone             | @EqualsTo(5615199022)@                                                    |
		| $.[?(@.id == '12170821')].address               | @EqualsTo(Schützenstraße 2-4, 44147 Dortmund, Germany)@                   |
		| $.[?(@.id == '12170821')].fuelTypes             | @HasItemsInAnyOrder('Premium Gasoline','Super Premium Gasoline')@         |
		| $.[?(@.id == '12170821')].amenities             | @HasItemsInAnyOrder('Unamanned','CRT Shell Card Accepted','Pay at pump')@ |
		| $.[?(@.id == '12170821')].openingHours          | @HasItemsInAnyOrder('Mon-Fri 00:00-23:59')@                               |
	And the JSON response body is validated using the json path expressions with associated matchers:
		| JsonPathExpression                              | MatcherName                                                                 |
		| $.[?(@.id == '12170823')].id                    | @EqualsTo(12170823)@                                                        |
		| $.[?(@.id == '12170823')].fullStationIdentifier | @EqualsTo(6)@                                                               |
		| $.[?(@.id == '12170823')].lat                   | @StartsWith(52)@                                                            |
		| $.[?(@.id == '12170823')].lon                   | @StartsWith(12)@                                                            |
		| $.[?(@.id == '12170823')].name                  | @EqualsTo(Mönchengladbach)@                                                 |
		| $.[?(@.id == '12170823')].telephone             | @EqualsTo(8045399022)@                                                      |
		| $.[?(@.id == '12170823')].address               | @EqualsTo(Reyerhütter Str. 26, 41065 Mönchengladbach, Germany)@             |
		| $.[?(@.id == '12170823')].fuelTypes             | @HasItemsInAnyOrder('DieselFit','Premium Diesel','Super Premium Gasoline')@ |
		| $.[?(@.id == '12170823')].amenities             | @HasItemsInAnyOrder('Car Wash','Shop','Pay at pump')@                       |
		| $.[?(@.id == '12170823')].openingHours          | @HasItemsInAnyOrder('Mon-Fri 00:00-23:59')@                                 |
	And the JSON response body is validated using the json path expressions with associated matchers:
		| JsonPathExpression                              | MatcherName                                                         |
		| $.[?(@.id == '12170825')].id                    | @EqualsTo(12170825)@                                                |
		| $.[?(@.id == '12170825')].fullStationIdentifier | @EqualsTo(6)@                                                       |
		| $.[?(@.id == '12170825')].lat                   | @StartsWith(51)@                                                    |
		| $.[?(@.id == '12170825')].lon                   | @StartsWith(13)@                                                    |
		| $.[?(@.id == '12170825')].name                  | @EqualsTo(Neusser)@                                                 |
		| $.[?(@.id == '12170825')].telephone             | @EqualsTo(8045399022)@                                              |
		| $.[?(@.id == '12170825')].address               | @EqualsTo(Neusser Str. 55, 50181 Bedburg, Germany)@                 |
		| $.[?(@.id == '12170825')].fuelTypes             | @HasItemsInAnyOrder('Fuelsave Midgrade Gasoline','Premium Diesel')@ |
		| $.[?(@.id == '12170825')].amenities             | @HasItemsInAnyOrder('Disabled Facilities','Shop','Truck Only')@     |
		| $.[?(@.id == '12170825')].openingHours          | @HasItemsInAnyOrder('Mon-Fri 00:00-23:59')@                         |

@shell-api-stub @ignore
Scenario: Retrieve Shell Petrol Stations by mandatory and optional query parameters
	Given FleetPay mobile app attempts to retrieve the shell petrol stations over API using the query parameters:
		| Lat       | Lon       | Radius | CountryCode | Amenities |
		| 52.533501 | 13.404813 | 0.3    | DE          | 24        |
	Then the response status should be "200"
	And the JSON response body is validated using the json path expressions with associated matchers:
		| JsonPathExpression                             | MatcherName                                                                                                                                                   |
		| $.Size()                                       | @EqualsTo(3)@                                                                                                                                                 |
		| $.[?(@.fullStationIdentifier == '1')].KeySet() | @HasItemsInAnyOrder('id','name','telephone','address','lat','lon','fuelTypes','amenities','openingHours','fullStationIdentifier','isTruckOnly','isUnmanned')@ |
	And the JSON response body is validated using the json path expressions with associated matchers:
		| JsonPathExpression                    | MatcherName                                                                           |
		| $.[?(@.id == '12170818')].amenities   | @HasItemsInAnyOrder('Disabled Facilities','Shop','Unamanned','Truck Only')@           |
		| $.[?(@.id == '12170818')].isTruckOnly | True                                                                                  |
		| $.[?(@.id == '12170818')].isUnmanned  | True                                                                                  |
		| $.[?(@.id == '12170825')].amenities   | @HasItemsInAnyOrder('Disabled Facilities','Shop','Truck Only')@ |
		| $.[?(@.id == '12170825')].isTruckOnly | True                                                                                  |
		| $.[?(@.id == '12170825')].isUnmanned  | False                                                                                 |
		| $.[?(@.id == '12170827')].amenities   | @HasItemsInAnyOrder('Disabled Facilities','Car Wash','Truck Only')@                   |
		| $.[?(@.id == '12170827')].isTruckOnly | True                                                                                  |
		| $.[?(@.id == '12170827')].isUnmanned  | False                                                                                 |