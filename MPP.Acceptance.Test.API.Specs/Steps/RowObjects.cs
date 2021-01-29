using System;

namespace MPP.Acceptance.Test.API.Specs.Steps
{
   public record CreateUserRowObject(string UserId, string ParticipantCode, string ParticipantReferenceId, string UserName, string FullName, string Email, string PhoneNumber, String Password);
}