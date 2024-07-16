using MotoRentalService.Domain.Entities;

namespace MotoRentalService.Application.Utils.CustomMessages
{
    public class MotoMessages : BaseMessages<Moto>
    {
        public override string AlreadyExists(string plate) => $"A motorcycle with the plate '{plate}' is already registered.";
        public string RentalAlreadyExists(Guid id) => $"Moto with Id '{id}' has an existing rental.";
    }

    public class DeliveryPersonMessages : BaseMessages<DeliveryPerson>
    {
        public override string AlreadyExists(string arg1) => $"The CNPJ '{arg1}' is already registered.";
        public string CnhAlreadyExists(string cnhNumber, string cnhType) => $"The CNH '{cnhNumber}-{cnhType}' is already registered.";
        public string UploadImageGenericError(Guid id) => $"An error occurred while uploading the image for ID '{id}'.";
        public string UploadImageNullError(Guid id) => $"The image provided for Id '{id}' is null, and the upload could not be processed.";
        public string UploadImageSuccessfully(string cnhNumber, string cnhType) => $"Image of the CNH '{cnhNumber}-{cnhType}' for the delivery person was uploaded successfully.";
    }

    public class RentalMessages : BaseMessages<Rental>
    {
        public override string AlreadyExists(string arg1) => $"A rental with Id '{arg1}' already exists.";
        public string DeliveryPersonNotFound(Guid id) => $"Delivery person with Id '{id}' not found.";
        public string DeliveryPersonNoLicenseTypeA(Guid id) => $"Delivery person with Id '{id}' does not have a type A license.";
        public string MotoNotFound(Guid id) => $"Moto with ID '{id}' not found.";
        public string RentalCreatedSuccessfully(Guid id, string name) => $"Rental created successfully for delivery person '{name}' with ID '{id}'.";
        public string RentalValueCalculatedSuccessfully(Guid rentalId, decimal totalCost) => $"Rental value for Id '{rentalId}' successfully calculated: {totalCost}.";
        public string RentalCalculationError(Guid id) => $"An error occurred while calculating the rental value for Id '{id}'.";
        public string EndDateBeforeStartDate(DateOnly endDate, DateOnly startDate) => $"End date '{endDate.ToString("dd/MM/yyyy")}' is before the start date '{startDate.ToString("dd/MM/yyyy")}'.";
        internal string InvalidRentalPlanType(DateOnly startDate, DateOnly endDate) => $"The provided rental period from {startDate.ToString("dd/MM/yyyy")} to {endDate.ToString("dd/MM/yyyy")} does not match any valid rental plan type.";
    }
}
