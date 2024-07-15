namespace MotoRentalService.Domain.Enums;

    /// <summary>
    /// Represents an user role.
    /// </summary>
    public enum EUserRole : short
    {
        /// <summary>
        /// Administrator of system.
        /// </summary>
        Admin = 1,
        
        /// <summary>
        /// Deliveryperson.
        /// </summary>
        User = 2,
    }