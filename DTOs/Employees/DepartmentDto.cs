using System.Text.Json.Serialization;

namespace BioTime.DTOs.Employees;

public class DepartmentDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("dept_code")]
    public string DeptCode { get; set; } = string.Empty;

    [JsonPropertyName("dept_name")]
    public string DeptName { get; set; } = string.Empty;
}
