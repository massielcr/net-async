using System.Text;

namespace AsyncFileAccess.Services
{
    public class FileService : IFileService
    {
        public async Task<(bool,string)> ReadTextFileAsync(string path)
        {            
            try
            {
                if (File.Exists(path) != false)
                {
                    string text = await File.ReadAllTextAsync(path);
                    return (true, text);
                }
                else
                {
                    return (false, $"file not found: {path}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"file not found: {ex.Message}");
            }
        }

        public async Task<(bool, string)> ReadTextFileStreamAsync(string path)
        {
            try
            {
                if (File.Exists(path) != false)
                {
                    string text = await ReadFileStreamAsync(path);
                    return (true, text);
                }
                else
                {
                    return (false, $"file not found: {path}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"file not found: {ex.Message}");
            }
        }

        private async Task<string> ReadFileStreamAsync(string path)
        {
            using var sourceStream =
                new FileStream(
                    path,
                    FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: 4096, useAsync: true);

            var sb = new StringBuilder();

            using var reader = new StreamReader(sourceStream, Encoding.UTF8, true);
            string text = await reader.ReadToEndAsync();

            sb.Append(text);

            return sb.ToString();
        }

        public async Task WriteTextFileAsync(string path, string content)
        {
            await File.WriteAllTextAsync(path, content);
        }

        public async Task WriteTextFileStreamAsync(string path, string content)
        {
            byte[] encodedText = Encoding.Unicode.GetBytes(content);

            using var sourceStream =
                new FileStream(
                    path,
                    FileMode.Create, FileAccess.Write, FileShare.None,
                    bufferSize: 4096, useAsync: true);

            await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
        }

        public async Task WriteTextFileParallelAsync(string folderPath)
        {
            string folder = Directory.CreateDirectory(folderPath).Name;
            IList<Task> writeTaskList = [];

            for (int index = 11; index <= 20; ++index)
            {
                string fileName = $"file-{index:00}.txt";
                string filePath = $"{folder}/{fileName}";
                string text = $"In file {index}{Environment.NewLine}";

                writeTaskList.Add(File.WriteAllTextAsync(filePath, text));
            }

            await Task.WhenAll(writeTaskList);
        }

        public async Task WriteTextFileStreamParallelAsync(string folderPath)
        {
            IList<FileStream> sourceStreams = [];

            try
            {
                string folder = Directory.CreateDirectory(folderPath).Name;

                IList<Task> writeTaskList = [];

                for (int index = 1; index <= 10; ++index)
                {
                    string fileName = $"filestream-{index:00}.txt";
                    string filePath = $"{folder}/{fileName}";

                    string text = $"In file Stream {index}{Environment.NewLine}";
                    byte[] encodedText = Encoding.Unicode.GetBytes(text);

                    var sourceStream =
                        new FileStream(
                            filePath,
                            FileMode.Create, FileAccess.Write, FileShare.None,
                            bufferSize: 4096, useAsync: true);

                    Task writeTask = sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                    sourceStreams.Add(sourceStream);

                    writeTaskList.Add(writeTask);
                }

                await Task.WhenAll(writeTaskList);
            }
            finally
            {
                foreach (FileStream sourceStream in sourceStreams)
                {
                    sourceStream.Close();
                }
            }
        }        
    }
}
