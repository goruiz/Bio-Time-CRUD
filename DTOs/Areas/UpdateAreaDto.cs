namespace BioTime.DTOs.Areas;

public class UpdateAreaDto
{
    public string AreaCode { get; set; } = string.Empty;
    public string AreaName { get; set; } = string.Empty;
    public int? ParentArea { get; set; }
}
