namespace LupercaliaMGCore.modules.AntiCamp.Models;

public record AntiCampSettings
{
    public float DetectionInterval { get; set; }
    public double DetectionRadius { get; set; }
    public float DetectionTime { get; set; }
    public float GlowingTime { get; set; }
}