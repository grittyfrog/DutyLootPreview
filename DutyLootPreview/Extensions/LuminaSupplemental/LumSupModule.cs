namespace DutyLootPreview.Extensions.LuminaSupplemental;

public class LumSupModule {
    public DungeonBossSheet DungeonBoss { get; } = new();
    public DungeonBossDropSheet DungeonBossDrop { get; } = new();
    public DungeonBossChestSheet DungeonBossChest { get; } = new();
    public DungeonChestSheet DungeonChest { get; } = new();
    public DungeonChestItemSheet DungeonChestItem { get; } = new();

    public void Prewarm() {
        DungeonBoss.Prewarm();
        DungeonBossDrop.Prewarm();
        DungeonBossChest.Prewarm();
        DungeonChest.Prewarm();
        DungeonChestItem.Prewarm();
    }
}
