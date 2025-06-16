namespace MosefakApp.Domains.Enums
{
    public enum PaymentMethod
    {
        [EnumMember(Value = "CreditCard")]
        CreditCard,

        [EnumMember(Value = "DebitCard")]
        DebitCard,

        [EnumMember(Value = "BankTransfer")]
        BankTransfer,

        [EnumMember(Value = "PayPal")]
        PayPal,

        [EnumMember(Value = "Wallet")]
        Wallet,

        [EnumMember(Value = "Cash")]
        Cash,

        [EnumMember(Value = "Stripe")] // ✅ تم إضافة هذا السطر
        Stripe
    }
}
