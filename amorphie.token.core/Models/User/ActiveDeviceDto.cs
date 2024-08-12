using amorphie.core.Base;

public class ActiveDeviceDto : DtoBase
{
    public string DeviceId { get; set; }
    public string Model { get; set; }
    public string Platform { get; set; }
    public DateTime RegistrationDate { get; set; }
    public DateTime? LastLogonDate { get; set; }
    public DateTime? ActivationRemovalDate { get; set; }
    public string CreatedByUserName { get; set; }
    public int status { get; set; }
}