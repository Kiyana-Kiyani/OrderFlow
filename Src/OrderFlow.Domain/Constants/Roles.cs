namespace OrderFlow.Domain.Constants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Owner = "Owner";
        public const string Customer = "Customer";
        public const string Courier = "Courier";

        public static readonly string[] All =
        {
        Admin,
        Owner,
        Customer,
        Courier
         };
    }
}
