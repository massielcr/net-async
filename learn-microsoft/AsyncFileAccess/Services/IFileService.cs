namespace AsyncFileAccess.Services
{
    public interface IFileService
    {
        Task<(bool, string)> ReadTextFileAsync(string path);
        Task<(bool, string)> ReadTextFileStreamAsync(string path);

        Task WriteTextFileAsync(string path, string content);
        Task WriteTextFileStreamAsync(string path, string content);        

        Task WriteTextFileParallelAsync(string folderPath);
        Task WriteTextFileStreamParallelAsync(string folderPath);
    }
}
