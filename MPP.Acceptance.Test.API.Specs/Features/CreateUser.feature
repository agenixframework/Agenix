Feature: Create User Successfully

Scenario: Create User

This scenario covers the case when the operator is willing to create a user via https://reqres.in/api/users

	Given the following user list
	  | Name     | Job    | 
      | morpheus | leader | 

	  When the operator attempts to create an user over API

	  Then the operator should see the http response code '201'

	  And the user should be created successfully