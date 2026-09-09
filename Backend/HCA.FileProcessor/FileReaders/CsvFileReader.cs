namespace HCA.FileProcessor.FileReaders;

public class CsvFileReader : IFileReader
{
    public List<string> ReadLines(StreamReader streamReader)
    {
        List<string> lines = new List<string>();

        string? line;

        do
        {
            line = streamReader.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
                lines.Add(line);

        } while (line != null);

        return lines;
    }
}
