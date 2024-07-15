namespace MotoRentalService.Api.Abstractions
{
    internal static class ApiRoutes
    {
        public const string Auth = "token/{role}";
        public const string BaseWithGuidId = "{id:guid}";

        internal static class DeliveryPerson
        {
            public const string UpdateCnh = "cnh";
        }

        internal static class Rental
        {
            public const string CalculateValue = "{id:guid}/calc";
        }
    }
}
