namespace MosefakApp.API.Filters.Authentication
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowAnonymousPermissionAttribute : Attribute { }

}
