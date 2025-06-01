Feature: Echo service

    Scenario: Say hello
        Given My name is Agenix
        When I say hello to the service
        Then the service should return: "Hello, my name is Agenix!"

    Scenario: Say goodbye
        Given My name is Agenix
        When I say goodbye to the service
        Then the service should return: "Goodbye from Agenix!"
