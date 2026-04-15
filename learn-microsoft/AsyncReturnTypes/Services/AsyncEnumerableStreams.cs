namespace AsyncReturnTypes.Services
{
    public static class AsyncEnumerableStreams
    {
        public static async IAsyncEnumerable<string> ReadWordsFromStreamAsync()
        {
            string data =
                @"This is a line of text.
              Here is the second line of text.
              And there is one more for good measure.
              Wait, that was the penultimate line.";

            using var readStream = new StringReader(data);

            string? line = await readStream.ReadLineAsync();
            while (line != null)
            {
                foreach (string word in line.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    await Task.Delay(100);
                    yield return word;
                }

                line = await readStream.ReadLineAsync();
            }
        }
    }
}
