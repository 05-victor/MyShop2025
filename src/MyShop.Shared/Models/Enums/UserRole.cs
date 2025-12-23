namespace MyShop.Shared.Models.Enums
{
    /// <summary>
    /// User roles in the system.
    /// Determines access level and available features.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Regular customer - can browse products and place orders.
        /// </summary>
        Customer,

        /// <summary>
        /// Sales agent - can manage own orders and earn commissions.
        /// </summary>
        SalesAgent,

        /// <summary>
        /// Administrator - full access to all system features.
        /// </summary>
        Admin
    }
}
