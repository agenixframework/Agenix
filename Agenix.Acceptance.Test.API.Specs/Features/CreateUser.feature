Feature: Create User Successfully

Scenario: Create an user successfully using variables

This scenario covers the case when the operator is willing to create a user via https://reqres.in/api/users

	  Given variables
		| Name     | Value    |
		| name	   | Morpheus |
		| job      | Leader   | 

	  And the following user list
		| Name     | Job    | 
		| ${name}  | ${job} | 

	  When the operator attempts to create an user over API

	  Then the response status should be "201"

	  And the values are extracted into the variables from the JSON response body:
	  | JsonPathExpression | VariableName |
	  | $.Name             | Name         |
	  | $.Job			   | Job		  |
      | $.KeySet()		   | KeySet		  |
      | $.Size()           | Size		  |
      | $.id			   | Id			  |

	  And the JSON response body is validated using the json path expressions with associated matchers:
	  |JsonPathExpression| MatcherName				|
      | $.Name			 | @StartsWith('Morp')@     |
      | $.Job			 | @EndsWith('der')@        |

	  And the JSON response body is validated using the json path expressions with associated matchers:
	  |JsonPathExpression| MatcherName					 |
      | $.Name		     | @EqualsIgnoreCase('${Name}')@ |
      | $.id			 | @IsNumber('${Id}')@           |

Scenario: Create an user successfully using examples of Functions

	  Given variables
		| Name     | Value						|
		| name	   | core:UpperCase('Morpheus') |
		| job      | core:LowerCase('LEADER')   | 

	  And the following user list
		| Name     | Job    | 
		| ${name}  | ${job} | 

	  When the operator attempts to create an user over API

	  Then the response status should be "201"

	  And the values are extracted into the variables from the JSON response body:
	  | JsonPathExpression | VariableName |
	  | $.Name             | Name         |
	  | $.Job			   | Job		  |
      | $.KeySet()		   | KeySet		  |
      | $.Size()           | Size		  |
      | $.id			   | Id			  |

	  And the JSON response body is validated using the json path expressions with associated matchers:
	  |JsonPathExpression| MatcherName								|
      | $.Name			 | @StartsWith(${Name})@					|
      | $.Job			 | @EndsWith(core:LowerCase('DER'))@        |

	  And the JSON response body is validated using the json path expressions with associated matchers:
	  |JsonPathExpression| MatcherName					 |
      | $.Name		     | @EqualsIgnoreCase('${Name}')@ |
      | $.id			 | @IsNumber('${Id}')@           |