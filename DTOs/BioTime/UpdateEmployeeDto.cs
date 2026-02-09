using System.Text.Json.Serialization;

namespace BioTime.DTOs.BioTime;

public class UpdateEmployeeDto
{
    [JsonPropertyName("emp_code")]
    public string EmpCode { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("department")]
    public int? Department { get; set; }

    [JsonPropertyName("position")]
    public int? Position { get; set; }

    [JsonPropertyName("hire_date")]
    public string? HireDate { get; set; }
}
