using MotoRentalService.CrossCutting.Storage;

namespace MotoRentalService.Infrastructure.Storage
{
    public class LocalStorageService(string storagePath) : IStorageService
    {
        private readonly string _storagePath = storagePath;

        /// <summary>
        /// Uploads a file to the local storage.
        /// </summary>
        /// <param name="fileName">The name of the file to upload.</param>
        /// <param name="data">The file data in bytes.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="StorageResult"/> with the URL of the uploaded file or an error message.</returns>
        public async Task<StorageResult> UploadAsync(string fileName, byte[] data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    return StorageResult.Failure("File name cannot be null or empty.");

                if (data == null || data.Length == 0)
                    return StorageResult.Failure("File data cannot be null or empty.");

                var newFileName = string.Concat(Guid.NewGuid().ToString(), Path.GetExtension(fileName));
                var fileDir = Path.Combine(Environment.CurrentDirectory, _storagePath.Trim('/'));

                if (!Directory.Exists(fileDir))
                    Directory.CreateDirectory(fileDir);

                var filePath = Path.Combine(fileDir, newFileName);

                await File.WriteAllBytesAsync(filePath, data);
                var fileUrl = $"file://{filePath.Replace("\\", "/")}";

                return StorageResult.Success(fileUrl);
            }
            catch (Exception ex)
            {
                return StorageResult.Failure($"Failed to upload file: {ex.Message}");
            }
        }
    }
}
