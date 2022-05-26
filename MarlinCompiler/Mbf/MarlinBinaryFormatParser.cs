namespace MarlinCompiler.Mbf;

public sealed class MarlinBinaryFormatParser
{
    public MarlinBinaryFormatParser(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File doesn't exist");
        }
        
        _fileStream = new FileStream(filePath, FileMode.Open);
    }

    private readonly FileStream _fileStream;
}