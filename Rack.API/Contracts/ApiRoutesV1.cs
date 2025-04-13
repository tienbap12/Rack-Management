using Microsoft.AspNetCore.Mvc;

namespace Rack.API.Contracts
{
    public static class ApiRoutesV1
    {
        public const string Root = "api";
        public const string Version = "v1";

        // Bao gồm cả Version trong Base path
        public const string Base = Root + "/" + Version; // Sẽ là "api/v1"

        public static class DataCenter
        {
            // Sử dụng tên resource số nhiều và làm base cho controller này
            private const string ControllerBase = Base + "/datacenters"; // Sẽ là "api/v1/datacenters"

            // GET api/v1/datacenters
            public const string GetAll = ControllerBase;

            // POST api/v1/datacenters
            public const string Create = ControllerBase;

            // GET api/v1/datacenters/{dcId:guid}
            // Sử dụng placeholder {dcId} và ràng buộc kiểu dữ liệu :guid
            public const string GetById = ControllerBase + "/{dcId:guid}";

            // PATCH hoặc PUT api/v1/datacenters/{dcId:guid}
            public const string Update = ControllerBase + "/{dcId:guid}";

            // DELETE api/v1/datacenters/{dcId:guid}
            public const string Delete = ControllerBase + "/{dcId:guid}";
        }

        public static class Rack
        {
            // Sử dụng tên resource số nhiều và làm base cho controller này
            private const string ControllerBase = Base + "/racks"; // Sẽ là "api/v1/racks"

            // GET api/v1/racks
            public const string GetAll = ControllerBase;

            // POST api/v1/racks
            public const string Create = ControllerBase;

            // GET api/v1/racks/{rackId:guid}
            // Sử dụng placeholder {rackId} và ràng buộc kiểu dữ liệu :guid
            public const string GetById = ControllerBase + "/{rackId:guid}";

            // PATCH hoặc PUT api/v1/racks/{rackId:guid}
            public const string Update = ControllerBase + "/{rackId:guid}";

            // DELETE api/v1/racks/{dcId:guid}
            public const string Delete = ControllerBase + "/{rackId:guid}";
        }

        public static class Device
        {
            // Sử dụng tên resource số nhiều và làm base cho controller này
            private const string ControllerBase = Base + "/devices"; // Sẽ là "api/v1/devices"

            // GET api/v1/devices
            public const string GetAll = ControllerBase;

            // POST api/v1/devices
            public const string Create = ControllerBase;

            // GET api/v1/devices/{deviceId:guid}
            // Sử dụng placeholder {deviceId} và ràng buộc kiểu dữ liệu :guid
            public const string GetById = ControllerBase + "/{deviceId:guid}";

            // PATCH hoặc PUT api/v1/devices/{deviceId:guid}
            public const string Update = ControllerBase + "/{deviceId:guid}";

            // DELETE api/v1/devices/{deviceId:guid}
            public const string Delete = ControllerBase + "/{deviceId:guid}";
        }

        public static class ConfigurationItem
        {
            private const string ControllerBase = Base + "/configuration-items";

            public const string GetAll = ControllerBase;
            public const string Create = ControllerBase;
            public const string GetById = ControllerBase + "/{configItemId:guid}";
            public const string Update = ControllerBase + "/{configItemId:guid}";
            public const string Delete = ControllerBase + "/{configItemId:guid}";
        }

        public static class Account
        {
            public const string Login = Base + "/login";
            public const string Register = Base + "/register";

            public const string RefreshToken = Base + "/refresh-token";
        }

        public static class Role
        {
            private const string ControllerBase = Base + "/roles"; // Sẽ là "api/v1/roles"

            // GET api/v1/roles
            public const string GetAll = ControllerBase;

            // POST api/v1/roles
            public const string Create = ControllerBase;

            // GET api/v1/roles/{roleId:guid}
            // Sử dụng placeholder {roles} và ràng buộc kiểu dữ liệu :guid
            public const string GetById = ControllerBase + "/{roleId:guid}";

            // PATCH hoặc PUT api/v1/roles/{roleId:guid}
            public const string Update = ControllerBase + "/{roleId:guid}";

            // DELETE api/v1/roles/{roleId:guid}
            public const string Delete = ControllerBase + "/{roleId:guid}";
        }

        public static class Customer
        {
            private const string ControllerBase = Base + "/customers"; // "api/v1/customers"
            public const string GetAll = ControllerBase;
            public const string Create = ControllerBase;
            public const string GetById = ControllerBase + "/{customerId:guid}";
            public const string Update = ControllerBase + "/{customerId:guid}";
            public const string Delete = ControllerBase + "/{customerId:guid}";
        }

        public static class ServerRental
        {
            private const string ControllerBase = Base + "/serverrentals"; // "api/v1/serverrentals"
            public const string GetAll = ControllerBase;
            public const string Create = ControllerBase;
            public const string GetById = ControllerBase + "/{rentalId:guid}";
            public const string Update = ControllerBase + "/{rentalId:guid}";
            public const string Delete = ControllerBase + "/{rentalId:guid}";
        }
    }
}