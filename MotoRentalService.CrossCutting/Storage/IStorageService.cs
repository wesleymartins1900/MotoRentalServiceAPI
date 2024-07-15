namespace MotoRentalService.CrossCutting.Storage
{
    public interface IStorageService
    {
        Task<StorageResult> UploadAsync(string fileName, byte[] data);
    }

    public class StorageResult
    {
        public bool IsSuccess { get; set; }
        public string? Url { get; set; }
        public string? ErrorMessage { get; set; }

        public static StorageResult Success(string url) => new StorageResult { IsSuccess = true, Url = url };
        public static StorageResult Failure(string errorMessage) => new StorageResult { IsSuccess = false, ErrorMessage = errorMessage };
    }
}
