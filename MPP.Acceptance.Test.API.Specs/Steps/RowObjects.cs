using System;

namespace MPP.Acceptance.Test.API.Specs.Steps
{
    public record CreateUserRowObject(string Name, string Job);

    public record RegisterUserRowObject(string Email, string Password);
}