namespace Rack.Contracts.DataCenter.Requests;

public record CreateDataCenterRequest(
    string Name,
    string? Location
);