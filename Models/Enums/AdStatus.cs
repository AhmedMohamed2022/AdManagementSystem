namespace AdManagementSystem.Models.Enums
{
    public enum AdStatus
    {
        Pending = 0,    // Ad submitted but not yet reviewed
        Approved = 1,   // Ad approved and can be displayed globally
        Rejected = 2,   // Ad rejected by admin
        Paused = 3,     // Temporarily paused by advertiser or admin
        Expired = 4     // Ad reached its end date or budget limit
    }
}
