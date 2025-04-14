namespace MosefakApp.Domains.Enums
{
    public enum UserType
    {
        [EnumMember(Value = "Doctor")]
        Doctor,

        [EnumMember(Value = "PendingDoctor")]
        PendingDoctor,

        [EnumMember(Value = "Patient")]
        Patient
    }
}
