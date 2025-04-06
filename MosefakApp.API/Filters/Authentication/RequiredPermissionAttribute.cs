namespace MosefakApp.API.Filters.Authentication
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public class RequiredPermissionAttribute : Attribute
    {
        public string Permission { get; }

        public RequiredPermissionAttribute(string permission)
        {
            Permission = permission;
        }
    }

}
