namespace PalWorld.Networking;

using Models;

/// <summary>
/// A helper class for parsing RCON responses.
/// </summary>
public interface IResponseParser
{
    /// <summary>
    /// Gets a list of players currently in the game.
    /// </summary>
    /// <returns>The currently active players</returns>
    Task<PalPlayer[]> GetPlayers();

    /// <summary>
    /// Gets the version information of the server
    /// </summary>
    /// <returns>The server version information</returns>
    Task<ServerInfo?> GetInfo();
}

internal class ResponseParser(IRconSender _rcon) : IResponseParser
{
    public async Task<PalPlayer[]> GetPlayers()
    {
        var response = await _rcon.SendShowPlayers();
        var lines = response.Content
            .Replace("\r", "")
            .Replace("\0", "")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length < 2) return [];

        return lines
            .Skip(1)
            .Select(t => t.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Where(t => t.Length == 3)
            .Select(t => new PalPlayer
            {
                Name = t[0].Trim(),
                Id = t[1].Trim(),
                SteamId = t[2].Trim()
            })
            .ToArray();
    }

    public async Task<ServerInfo?> GetInfo()
    {
        var regex = new Regex("\\[(?'ver'.*)\\] (?'name'.*)", RegexOptions.Compiled);
        var response = await _rcon.SendInfo();
        
        var match = regex.Match(response.Content);
        if (!match.Success) return null;

        return new ServerInfo
        {
            Name = match.Groups["name"].Value.Trim(),
            Version = match.Groups["ver"].Value.Trim()
        };
    }
}
