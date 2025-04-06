namespace Rack.Contracts.DataCenter.Requests;

public record UpdateDataCenterRequest(
    Guid Id, // Thêm ID cho update
    string Name,
    string? Location
);