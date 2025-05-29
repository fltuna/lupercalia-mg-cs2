namespace LupercaliaMGCore.modules.ExternalView.API
{
    public interface IExternalViewCsApi
    {
        float DeltaTime { get; }

        float CurrentTime { get; }

        IExternalViewConVars ConVars { get; }

        // Entities
        IEnumerable<IExternalViewCsPlayer> AllPlayers { get; }

        IExternalViewCsPlayer? GetPlayer(ulong id);
        IExternalViewCsPlayer? GetPlayerBySlot(int slot);

        IExternalViewCsEntity? CreateCameraEntity();

        IExternalViewCsEntity? CreatePreviewModelEntity(IExternalViewCsPlayer player);
    }
}
