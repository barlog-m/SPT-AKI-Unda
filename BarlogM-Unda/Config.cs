namespace BarlogM_Unda;

public class Config
{
    public int MaxPmcGroupSize { get; set; } = 3;
    public int MaxScavGroupSize { get; set; } = 3;
    public string PmcBotDifficulty { get; set; } = "normal";
    public int ChanceForQuietRaid { get; set; } = 30;
    public bool Debug { get; set; } = true;
}
