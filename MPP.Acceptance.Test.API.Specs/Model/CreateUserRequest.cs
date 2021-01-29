namespace MPP.Acceptance.Test.API.Specs.Model
{
    class CreateUserRequest
    {
        public string UserId { get; set; }

        public string ParticipantCode { get; set; }

        public string ParticipantReferenceId { get; set; }

        public string UserName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Password { get; set; }

    }
}
