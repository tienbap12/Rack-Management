namespace Rack.Contracts.DataCenter;

public class DataCenterDto
{
    public string Name { get; set; } = null!;
    public string? Location { get; set; }
}

public class CreateDataCenterDto
{
    public string Name { get; set; } = null!;
    public string? Location { get; set; }
}

public class UpdateDataCenterDto
{
    public string Name { get; set; } = null!;
    public string? Location { get; set; }
}
