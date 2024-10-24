Feature: Retrieve user details
  As an Operator
  I want to retrieve the User details
  So that the front-end is tested against a real API
  Covers a variety of positive scenarios associated with retrieval of the user details on behalf of operator across API

Scenario: Retrieve user details by Id successfully
  This scenario covers the case when a participant attempts to retrieve user details over API
  and check the response details
	When the operator attempts to retrieve the user details by Id "2" over API
	Then the response status should be "200"
	And the JSON response body is validated using the json path expressions with associated matchers:
		| JsonPathExpression | MatcherName                                                                                    |
		| $.data.id          | @IsNumber('2')@                                                                                |
		| $.data.email       | @EqualsTo(janet.weaver@reqres.in)@                                                             |
		| $.data.first_name  | @EqualsTo(Janet)@                                                                              |
		| $.data.last_name   | @EqualsTo(Weaver)@                                                                             |
		| $.data.avatar      | @EndsWith('/img/faces/2-image.jpg')@                                                           |
		| $.support.url      | @EndsWith('/#support-heading')@                                                                |
		| $.support.text     | @EqualsIgnoreCase('To keep ReqRes free, contributions towards server costs are appreciated!')@ |
	And the JSON response body is validated using the json path expressions with associated matchers:
		| JsonPathExpression | MatcherName                                                          |
		| $.KeySet()         | @ContainsItem('data')@                                               |
		| $.data.KeySet()    | @HasItemsInAnyOrder('id','email','first_name','avatar','last_name')@ |
		| $.support.KeySet() | @HasItems('url','text')@                                             |
	And the values are extracted into the variables from the JSON response headers:
		| HeaderName   | VariableName        |
		| Content-Type | ContentTypeVariable |
	And echo "The value for content type is: ${ContentTypeVariable}"
	And the JSON response headers are validated using matchers:
		| HeaderName        | MatcherName                                           |
		| Content-Type      | @EqualsIgnoreCase('application/json; charset=utf-8')@ |
		| Transfer-Encoding | @EqualsIgnoreCase('chunked')@                         |
		| Connection        | @EqualsIgnoreCase('keep-alive')@                      |