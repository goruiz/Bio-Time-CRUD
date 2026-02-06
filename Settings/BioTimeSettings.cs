namespace BioTime.Settings;

public class BioTimeSettings
{
    public const string Section = "BioTime";
    public string BaseUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
