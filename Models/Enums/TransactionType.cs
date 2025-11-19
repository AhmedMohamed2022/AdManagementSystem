namespace AdManagementSystem.Models.Enums
{
    public enum TransactionType
    {
        Impression = 0,
        Click = 1,
        ManualCredit = 10,
        ManualDebit = 11,
        Adjustment = 20,
        Withdrawal,
        ManualDebitReversal // optional
    }
}
