namespace Rack.Contracts.DataCenter.Requests;

public record UpdateDataCenterRequest(
    string? Name,
    string? Location
);