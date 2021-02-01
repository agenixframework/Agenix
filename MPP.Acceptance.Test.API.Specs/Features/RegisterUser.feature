Feature: Register User Successfully

Scenario: User is registered successfully

This scenario covers the case when the operator is willing to register a user via https://reqres.in/api/register

	Given the following user details
		| Email              | Password |
		| eve.holt@reqres.in | pistol   |

	When operator registers the user over API

	Then operator receives 200 response code

	And user is registered successfully