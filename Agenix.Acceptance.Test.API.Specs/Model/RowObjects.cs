namespace FleetPay.Acceptance.Test.API.Specs.Model
{
    public record CreateUserRowObject(string Name, string Job);

    public record RegisterUserRowObject(string Email, string Password);

    public record ShellStationRowObject(string Lat, string Lon, string Radius, string CountryCode, string Amenities);
}