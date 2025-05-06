using Mapster;
using Rack.Contracts.Card.Response;
using Rack.Contracts.ConfigurationItem.Response;
using Rack.Contracts.Device.Requests;
using Rack.Contracts.Device.Responses;
using Rack.Contracts.DeviceRack.Response;
using Rack.Contracts.Port.Response;
using Rack.Domain.Entities;

namespace Rack.Application.Mapping
{
    public class MapsterConfig
    {
        public static void Configure()
        {
            // Cấu hình ánh xạ từ CreateDeviceRequest sang Device
            TypeAdapterConfig<CreateDeviceRequest, Device>
                .NewConfig()
                .Ignore(dest => dest.ConfigurationItems)
                .Ignore(dest => dest.Cards)
                .Ignore(dest => dest.Ports)
                .Ignore(dest => dest.ChildDevices)
                .Ignore(dest => dest.ServerRentals)
                .Ignore(dest => dest.ParentDevice)
                .Ignore(dest => dest.Rack)
                .Ignore(dest => dest.IsDeleted)
                .Ignore(dest => dest.DeletedOn)
                .Ignore(dest => dest.DeletedBy)
                .Ignore(dest => dest.CreatedOn)
                .Ignore(dest => dest.CreatedBy)
                .Ignore(dest => dest.LastModifiedOn)
                .Ignore(dest => dest.LastModifiedBy);

            // Cấu hình ánh xạ từ Device sang DeviceResponse
            TypeAdapterConfig<Device, DeviceResponse>
                .NewConfig()
                .Map(dest => dest.ParentDeviceID, src => src.ParentDeviceID)
                .Map(dest => dest.RackID, src => src.RackID)
                .Map(dest => dest.ConfigurationItems, src => src.ConfigurationItems != null ? src.ConfigurationItems.Select(ci => ci.Adapt<ConfigurationItemResponse>()).ToList() : null)
                .Map(dest => dest.Cards, src => src.Cards != null ? src.Cards.Select(c => c.Adapt<CardResponse>()).ToList() : null)
                .Map(dest => dest.Ports, src => src.Ports != null ? src.Ports.Select(p => p.Adapt<PortResponse>()).ToList() : null)
                .Map(dest => dest.ChildDevices, src => src.ChildDevices != null ? src.ChildDevices.Select(cd => cd.Adapt<DeviceResponse>()).ToList() : null);
        }
    }
}