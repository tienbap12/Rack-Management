namespace Rack.API.Contracts
{
    public static class ApiRoutesV1
    {
        public const string Root = "api";
        public const string Version = "v1";
        public const string Base = Root + "/";

        public static class DataCenter
        {
            public const string GetAll = Base + "DataCenter";
            public const string GetById = GetAll + "/DataCenterId";
            public const string Create = Base + "DataCenter";
            public const string Update = GetAll + "/DataCenterId";
            public const string Delete = GetAll + "/DataCenterId";
        }
        public static class Category
        {
            public const string GetAll = Base + "Category";
            public const string GetById = GetAll + "/categoryId";
            public const string Create = Base + "Category";
            public const string Update = Base + "Category";
            public const string Delete = Base + "Category";
        }
        public static class Account
        {
            public const string Login = Base + "Login";
            public const string Register = Base + "Register";
        }
        public static class Order
        {
            public const string GetAll = Base + "Order";
            public const string Create = Base + "Order";
            public const string GetDetail = Base + "OrderDetail" + "/OrderId";
        }

    }
}
