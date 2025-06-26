namespace Perform.Model;

public class Song
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public List<SongSection>? Lyrics {get; set; }

    public string? TimeSignature { get; set; } = null;

    public int? BPM { get; set; } = null;

    public List<ConsoleSettings> Consoles { get; set; } = [];
}

public class SongSection
{
    private string Name { get; set; } = string.Empty;

    private string Type { get; set; } = string.Empty;

    public List<SongLine> Lines { get; set; } = [];
}

public class SongLine
{
    public string Text { get; set; } = string.Empty;
 
    public int Bar { get; set; } = 0;
}