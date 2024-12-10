using System.Globalization;

namespace ModularDiscordBot.Structures;

public sealed class ByondStatus
{
    public string Version { get; set; } = string.Empty;
    public string Mode { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string RoundId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public string Map { get; set; } = string.Empty;
    public string SecurityLevel { get; set; } = string.Empty;
    public bool Respawn { get; set; }
    public bool Enter { get; set; }
    public bool Vote { get; set; }
    public bool Ai { get; set; }
    public int Players { get; set; }
    public int Admins { get; set; }
    public int PopCap { get; set; }
    public int SoftPopCap { get; set; }
    public int HardPopCap { get; set; }
    public int ExtremePopCap { get; set; }
    public DateTime RevisionDate { get; set; }
    public GameState GameState { get; set; }
    public TimeSpan RoundDuration { get; set; }
    public TimeSpan TimeDilationCurrent { get; set; }
    public TimeSpan TimeDilationAverage { get; set; }
    public TimeSpan TimeDilationAverageSlow { get; set; }
    public TimeSpan TimeDilationAverageFast { get; set; }
}

public static class ByoundStatusHelper
{
    public static ByondStatus FromQuery(Dictionary<string, string> query)
    {
        return new ByondStatus
        {
            Version = 
                query.TryGetValue("version", out var version) 
                    ? version 
                    : string.Empty,
            Mode = 
                query.TryGetValue("mode", out var mode) 
                    ? mode 
                    : string.Empty,
            Host = 
                query.TryGetValue("host", out var host) 
                    ? host 
                    : string.Empty,
            RoundId = 
                query.TryGetValue("round_id", out var roundId) 
                    ? roundId 
                    : string.Empty,
            Revision = 
                query.TryGetValue("revision", out var revision) 
                    ? revision 
                    : string.Empty,
            Map = 
                query.TryGetValue("map_name", out var map) 
                    ? map 
                    : string.Empty,
            SecurityLevel = 
                query.TryGetValue("security_level", out var securityLevel) 
                    ? securityLevel 
                    : string.Empty,
            Respawn = 
                query.TryGetValue("respawn", out var respawn) 
                && respawn == "1",
            Enter = 
                query.TryGetValue("enter", out var enter) 
                && enter == "1",
            Vote = 
                query.TryGetValue("vote", out var vote) 
                && vote == "1",
            Ai = 
                query.TryGetValue("ai", out var ai) 
                && ai == "1",
            Players = 
                query.TryGetValue("players", out var players) 
                    ? int.Parse(players) 
                    : -1,
            Admins = 
                query.TryGetValue("admins", out var admins) 
                    ? int.Parse(admins) 
                    : -1,
            PopCap = 
                query.TryGetValue("popcap", out var popCap) 
                    ? int.Parse(popCap) 
                    : -1,
            SoftPopCap = 
                query.TryGetValue("soft_popcap", out var softPopCap) 
                    ? int.Parse(softPopCap) 
                    : -1,
            HardPopCap = 
                query.TryGetValue("hard_popcap", out var hardPopCap) 
                    ? int.Parse(hardPopCap) 
                    : -1,
            ExtremePopCap = 
                query.TryGetValue("extreme_popcap", out var extremePopCap) 
                    ? int.Parse(extremePopCap) 
                    : -1,
            RevisionDate = 
                query.TryGetValue("revision_date", out var revisionDate) 
                    ? DateTime.Parse(revisionDate) 
                    : DateTime.MinValue,
            GameState = 
                query.TryGetValue("gamestate", out var gameState) 
                    ? Enum.Parse<GameState>(gameState) 
                    : GameState.Unknown,
            RoundDuration = 
                query.TryGetValue("round_duration", out var roundDuration) 
                    ? TimeSpan.FromSeconds(int.Parse(roundDuration)) 
                    : TimeSpan.Zero,
            TimeDilationCurrent = 
                query.TryGetValue("time_dilation_current", out var timeDilationCurrent) 
                    ? TimeSpan.FromSeconds(double.Parse(timeDilationCurrent, CultureInfo.InvariantCulture)) 
                    : TimeSpan.Zero,
            TimeDilationAverage = 
                query.TryGetValue("time_dilation_avg", out var timeDilationAverage) 
                    ? TimeSpan.FromSeconds(double.Parse(timeDilationAverage, CultureInfo.InvariantCulture)) 
                    : TimeSpan.Zero,
            TimeDilationAverageSlow = 
                query.TryGetValue("time_dilation_avg_slow", out var timeDilationAverageSlow) 
                    ? TimeSpan.FromSeconds(double.Parse(timeDilationAverageSlow, CultureInfo.InvariantCulture)) 
                    : TimeSpan.Zero,
            TimeDilationAverageFast = 
                query.TryGetValue("time_dilation_avg_fast", out var timeDilationAverageFast) 
                    ? TimeSpan.FromSeconds(double.Parse(timeDilationAverageFast, CultureInfo.InvariantCulture)) 
                    : TimeSpan.Zero
        };
    }
}