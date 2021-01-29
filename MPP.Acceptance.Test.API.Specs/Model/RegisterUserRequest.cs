using System.Runtime.Serialization;

namespace MPP.Acceptance.Test.API.Specs.Model
{
    [DataContract]
    public class RegisterUserRequest
    {
        [DataMember(Name = "email")]
        public string Email { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }
    }

}
