namespace amorphie.token.core.Models.User;

public class GetPublicDeviceDto
{
    public string DeviceId { get; set; }
    public int Status { get; set; }
    public string Model { get; set; }
    public string Platform { get; set; }
    public DateTime RegistrationDate { get; set; }
    public Guid Id { get; set; }
}
