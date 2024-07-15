namespace MotoRentalService.Domain.Enums
{
    /// <summary>
    /// Enumeration that represents the available types of driving licenses.
    /// </summary>
    /// <remarks>
    /// The [Flags] attribute indicates that the enumeration values can be combined using bitwise operations.
    /// </remarks>
    [Flags]
    public enum LicenseType
    {
        /// <summary>
        /// No license type. Represents a default value of 0.
        /// </summary>
        None = 0,

        /// <summary>
        /// License type A, typically associated with motorcycles and two-wheeled vehicles.
        /// </summary>
        A = 1,

        /// <summary>
        /// License type B, typically associated with four-wheeled vehicles such as cars.
        /// </summary>
        B = 2,

        /// <summary>
        /// Combination of license types A and B. Allows driving vehicles of both types.
        /// </summary>
        AB = A | B,
    }
}
