Feature: Create User Successfully

Scenario: Create User

	Given the following user list
	  | ParticipantCode    | ParticipantReferenceId     | UserName    | FullName   | Email         | PhoneNumber | Password   |
      | <Participant Code> | <Participant Reference Id> | <User Name> | <FullName> | test@mail.com | 4568456454  | <Password> |

	  When When the operator attempts to create an user over API

	  Then the user should be created successfully