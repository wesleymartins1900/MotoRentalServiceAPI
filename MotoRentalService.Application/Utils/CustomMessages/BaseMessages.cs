namespace MotoRentalService.Application.Utils.CustomMessages
{
    public abstract class BaseMessages<T> : ICustomMessages where T : class
    {
        protected string TypeName => typeof(T).Name;

        public virtual string Registered(string arg1) => $"{TypeName} with id '{arg1}' registered successfully.";
        public virtual string AlreadyExists(string arg1) => $"{TypeName} with id '{arg1}' already exists.";
        public virtual string NotFound(Guid id) => $"{TypeName} with id '{id}' not found.";
        public virtual string Updated(string propertyName, string propertyValue) => $"{TypeName} updated {propertyName}: {propertyValue}.";
        public virtual string Deleted(string data, string value) => $"{TypeName} with {data} '{value}' deleted successfully.";
        public virtual string Published(string propertyName, string propertyValue) => $"{TypeName} with {propertyName} '{propertyValue}' published for validation.";
        public string CacheHit(string cacheKey, string cachedData) => $"Cache hit for key: {cacheKey}. Cached data for {cachedData}.";
    }
}
