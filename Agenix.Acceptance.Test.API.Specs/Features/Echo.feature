Feature: Echo runner features
Covers a set of scenarios to echo/ print messages to the console/logger during test execution.

Scenario: Echo messages resolving dynamic the variables

    Given variables
      | Name    | Value         |
      | hello   | I say hello   |
      | goodbye | I say goodbye |

    Then echo "Variable hello=${hello}"

    Then echo "Variable goodbye=${goodbye}"

Scenario: Echo messages resolving variables with functions

    Given variable randomUUID is "core:RandomUUID()"

    Then echo "Variable 'randomUUID'=${randomUUID}"

    Given variables
      | Name        | Value              |
      | currentDate | core:CurrentDate() |

    Then echo "Variable 'currentDate'=${currentDate}"