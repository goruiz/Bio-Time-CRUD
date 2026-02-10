namespace BioTime.DTOs.Devices;

public class SyncResultDto
{
    public bool Success { get; set; }
    public string TerminalSn { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
