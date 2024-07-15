﻿using MotoRentalService.Domain.Entities;

namespace MotoRentalService.Application.Utils.CustomMessages
{
    public class MotoMessages : BaseMessages<Moto>
    {
        public override string AlreadyExists(string plate) => $"A motorcycle with the plate '{plate}' is already registered.";
        public string RentalAlreadyExists(Guid id) => $"Moto with ID '{id}' has an existing rental.";
    }

    public class DeliveryPersonMessages : BaseMessages<DeliveryPerson>
    {
        public override string AlreadyExists(string arg1) => $"The CNPJ '{arg1}' is already registered.";
        public string CnhAlreadyExists(string cnhNumber, string cnhType) => $"The CNH '{cnhNumber}-{cnhType}' is already registered.";
        public string UploadImageGenericError(Guid id) => $"An error occurred while uploading the image for ID '{id}'.";
        public string UploadImageNullError(Guid id) => $"The image provided for ID '{id}' is null, and the upload could not be processed.";
        public string UploadImageSuccessfully(string cnhNumber, string cnhType) => $"Image of the CNH '{cnhNumber}-{cnhType}' for the delivery person was uploaded successfully.";
    }

    public class RentalMessages : BaseMessages<Rental>
    {
        public override string AlreadyExists(string arg1) => $"A rental with ID '{arg1}' already exists.";
        public string DeliveryPersonNotFound(Guid id) => $"Delivery person with ID '{id}' not found.";
        public string DeliveryPersonNoLicenseTypeA(Guid id) => $"Delivery person with ID '{id}' does not have a type A license.";
        public string MotoNotFound(Guid id) => $"Moto with ID '{id}' not found.";
        public string RentalCreatedSuccessfully(Guid id, string name) => $"Rental created successfully for delivery person '{name}' with ID '{id}'.";
        public string RentalValueCalculatedSuccessfully(Guid rentalId, decimal totalCost) => $"Rental value for ID '{rentalId}' successfully calculated: {totalCost}.";
        public string RentalCalculationError(Guid id) => $"An error occurred while calculating the rental value for ID '{id}'.";
        public string EndDateBeforeStartDate(DateTime endDate, DateTime startDate) => $"End date '{endDate}' is before the start date '{startDate}'.";
        internal string InvalidRentalPlanType(DateTime startDate, DateTime endDate) => $"The provided rental period from {startDate.ToShortDateString()} to {endDate.ToShortDateString()} does not match any valid rental plan type.";
    }
}
