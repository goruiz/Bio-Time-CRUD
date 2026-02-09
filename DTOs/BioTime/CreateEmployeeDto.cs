namespace BioTime.DTOs.BioTime;

public class CreateEmployeeDto
{
    public string EmpCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? Department { get; set; }
    public int? Position { get; set; }
    public List<int> Area { get; set; } = [];
    public string? HireDate { get; set; }
}
