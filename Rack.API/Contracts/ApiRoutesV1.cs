namespace Rack.API.Contracts;

public static class ApiRoutesV1
{
    public const string Root = "api";
    public const string Version = "v1";

    // Bao gồm cả Version trong Base path
    public const string Base = Root + "/" + Version; // Sẽ là "api/v1"

    public static class Component
    {
        // Sử dụng tên resource số nhiều và làm base cho controller này
        private const string ControllerBase = Base + "/components"; // Sẽ là "api/v1/components"

        // GET api/v1/components
        public const string GetAll = ControllerBase;

        // POST api/v1/components
        public const string Create = ControllerBase;

        // GET api/v1/components/{componentId:guid}
        // Sử dụng placeholder {componentId} và ràng buộc kiểu dữ liệu :guid
        public const string GetById = ControllerBase + "/{componentId:guid}";

        // PATCH hoặc PUT api/v1/components/{componentId:guid}
        public const string Update = ControllerBase + "/{componentId:guid}";

        // DELETE api/v1/components/{componentId:guid}
        public const string Delete = ControllerBase + "/{componentId:guid}";
    }
    public static class PortConnection
    {
        private const string ControllerBase = Base + "/port-connections";
        public const string GetById = ControllerBase + "/{portConnectionId:guid}";
    }
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

        // DELETE api/v1/racks/{rackId:guid}
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
        public const string GetByDeviceId = ControllerBase + "/{deviceId:guid}";
        public const string Update = ControllerBase + "/{configItemId:guid}";
        public const string Delete = ControllerBase + "/{configItemId:guid}";
    }

    public static class Auth
    {
        public const string Login = Base + "/login";
        public const string Register = Base + "/register";

        public const string RefreshToken = Base + "/refresh-token";
    }

    public static class Account
    {
        private const string ControllerBase = Base + "/accounts"; // "api/v1/accounts"

        // GET api/v1/accounts
        public const string GetAll = ControllerBase;

        // GET api/v1/accounts/profile
        public const string GetProfile = ControllerBase + "/profile";

        // POST api/v1/accounts
        public const string Create = ControllerBase;

        // GET api/v1/accounts/{accountId:guid}
        public const string GetById = ControllerBase + "/{accountId:guid}";

        // PUT api/v1/accounts/{accountId:guid}
        public const string Update = ControllerBase + "/{accountId:guid}";

        // DELETE api/v1/accounts/{accountId:guid}
        public const string Delete = ControllerBase + "/{accountId:guid}";
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

    public static class Port
    {
        private const string ControllerBase = Base + "/ports";

        public const string GetAll = ControllerBase;
        public const string GetById = ControllerBase + "/{portId:guid}";
        public const string Create = ControllerBase;
        public const string Update = ControllerBase + "/{portId:guid}";
        public const string Delete = ControllerBase + "/{portId:guid}";
    }

    // public static class Card
    // {
    //     private const string Base = $"{ApiBase}/cards";

    //     public const string GetAll = Base;
    //     public const string GetById = $"{Base}/{{cardId:guid}}";
    //     public const string Create = Base;
    //     public const string Update = $"{Base}/{{cardId:guid}}";
    //     public const string Delete = $"{Base}/{{cardId:guid}}";
    // }
}