using System.Text.Json.Serialization;
using BioTime.Converters;

namespace BioTime.DTOs.BioTime;

public class EmployeeDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("emp_code")]
    public string EmpCode { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("department")]
    [JsonConverter(typeof(DepartmentDtoConverter))]
    public DepartmentDto? Department { get; set; }

    [JsonPropertyName("position")]
    [JsonConverter(typeof(PositionDtoConverter))]
    public PositionDto? Position { get; set; }

    [JsonPropertyName("hire_date")]
    public string? HireDate { get; set; }
}
