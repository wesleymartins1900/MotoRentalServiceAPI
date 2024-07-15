namespace MotoRentalService.Application.Utils.CustomMessages
{
    public interface ICustomMessages
    {
        string Registered(string arg1);
        string AlreadyExists(string arg1);
        string NotFound(Guid id);
        string Updated(string propertyName, string propertyValue);
        string Deleted(string propertyName, string propertyValue);
        string Published(string propertyName, string propertyValue);
        string CacheHit(string cacheKey, string cachedData);
    }
}
