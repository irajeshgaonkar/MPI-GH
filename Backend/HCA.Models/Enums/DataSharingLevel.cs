namespace HCA.Models.Enums
{
    /// <summary>
    /// Data sharing access level
    /// </summary>
    public enum DataSharingLevel
    {
        /// <summary>
        /// For existence of the identities
        /// </summary>
        Existence,

        /// <summary>
        /// Identity data except the protected population
        /// </summary>
        ExceptProtectedPopulation,

        /// <summary>
        /// Full data access
        /// </summary>
        Full
    }
}
