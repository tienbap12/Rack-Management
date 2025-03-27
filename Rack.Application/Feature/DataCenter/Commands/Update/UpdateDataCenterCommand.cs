using Rack.Contracts.DataCenter;
using Rack.Doamin.Commons.Primitives;

namespace Rack.Application.Feature.DataCenter.Commands.Update;

internal class UpdateDataCenterCommand(UpdateDataCenterDto updateDataCenterDto) : Primitives.ICommand<Response>
{
    public string Name { get; set; } = updateDataCenterDto.Name;
    public string? Location { get; set; } = updateDataCenterDto.Location;
    public bool IsDeleted { get; set; } = updateDataCenterDto.IsDeleted;
    public DateTime? DeletedOn { get; set; } = updateDataCenterDto.DeletedOn;
    public string? DeletedBy { get; set; } = updateDataCenterDto.DeletedBy;
    public DateTime? LastModifiedOn { get; set; } = updateDataCenterDto.LastModifiedOn;
    public string LastModifiedBy { get; set; } = updateDataCenterDto.LastModifiedBy;
}
