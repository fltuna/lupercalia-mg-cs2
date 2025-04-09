using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using LupercaliaMGCore.model;

namespace LupercaliaMGCore.modules;

public class AntiCamp(IServiceProvider serviceProvider, bool hotReload) : PluginModuleBase(serviceProvider)
{
    public override string PluginModuleName => "AntiCamp";

    public override string ModuleChatPrefix => "[AntiCamp]";

    private CounterStrikeSharp.API.Modules.Timers.Timer timer = null!;

    private readonly Dictionary<CCSPlayerController, (CBaseModelEntity? glowEntity, CBaseModelEntity? relayEntity)> playerGlowingEntity = new();

    private readonly Dictionary<CCSPlayerController, float> playerCampingTime = new();

    private readonly Dictionary<CCSPlayerController, PlayerPositionHistory> playerPositionHistory = new();

    private readonly Dictionary<CCSPlayerController, float> playerGlowingTime = new();
    private readonly Dictionary<CCSPlayerController, bool> isPlayerWarned = new();

    private Dictionary<CCSPlayerController, CounterStrikeSharp.API.Modules.Timers.Timer> glowingTimer = new();

    private bool isRoundStarted = false;

    
    public readonly FakeConVar<bool> IsModuleEnabled = new("lp_mg_anti_camp_enabled",
        "Anti camp enabled", true);

    public readonly FakeConVar<float> CampDetectionTime = new("lp_mg_anti_camp_detection_time",
        "How long to take detected as camping in seconds.", 10.0F);

    public readonly FakeConVar<double> CampDetectionRadius = new("lp_mg_anti_camp_detection_radius",
        "Range of area for player should move for avoiding the detected as camping.", 200.0F);

    public readonly FakeConVar<float> CampDetectionInterval = new("lp_mg_anti_camp_detection_interval",
        "Interval to run camping check in seconds.", 0.1F);

    public readonly FakeConVar<float> CampMarkingTime = new("lp_mg_anti_camp_glowing_time",
        "How long to detected player keep glowing.", 10.0F);
    
    protected override void OnInitialize()
    {
        Plugin.RegisterEventHandler<EventPlayerConnect>(OnPlayerConnect, HookMode.Pre);
        Plugin.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull, HookMode.Pre);
        Plugin.RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);

        Plugin.RegisterEventHandler<EventRoundFreezeEnd>(OnRoundFreezeEnd, HookMode.Post);
        Plugin.RegisterEventHandler<EventRoundEnd>(OnRoundEnd, HookMode.Post);
        Plugin.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath, HookMode.Post);
        Plugin.RegisterEventHandler<EventPlayerTeam>(OnSwitchTeam, HookMode.Post);


        if (hotReload)
        {
            bool isFreezeTimeEnded = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules")
                .First().GameRules?.FreezePeriod ?? false;

            if (isFreezeTimeEnded)
            {
                isRoundStarted = true;
            }

            foreach (var client in Utilities.GetPlayers())
            {
                if (!client.IsValid || /*client.IsBot ||*/ client.IsHLTV)
                    continue;

                InitClientInformation(client);
            }
        }

        timer = Plugin.AddTimer(CampDetectionInterval.Value, checkPlayerIsCamping, TimerFlags.REPEAT);
    }

    protected override void OnUnloadModule()
    {
        Plugin.DeregisterEventHandler<EventPlayerConnect>(OnPlayerConnect, HookMode.Pre);
        Plugin.DeregisterEventHandler<EventPlayerConnectFull>(OnPlayerConnectFull, HookMode.Pre);
        Plugin.RemoveListener<Listeners.OnClientPutInServer>(OnClientPutInServer);

        Plugin.DeregisterEventHandler<EventRoundFreezeEnd>(OnRoundFreezeEnd, HookMode.Post);
        Plugin.DeregisterEventHandler<EventRoundEnd>(OnRoundEnd, HookMode.Post);
        Plugin.DeregisterEventHandler<EventPlayerDeath>(OnPlayerDeath, HookMode.Post);
        Plugin.DeregisterEventHandler<EventPlayerTeam>(OnSwitchTeam, HookMode.Post);
        timer.Kill();
    }

    private void checkPlayerIsCamping()
    {
        if (!isRoundStarted || !IsModuleEnabled.Value)
            return;

        foreach (var client in Utilities.GetPlayers())
        {
            if (!client.IsValid || /*client.IsBot ||*/ client.IsHLTV)
                continue;

            if (client.Team == CsTeam.None || client.Team == CsTeam.Spectator)
                continue;

            if (!PlayerUtil.IsPlayerAlive(client))
                continue;

            if (!IsClientInformationAccessible(client))
                continue;

            Vector? clientOrigin = client.PlayerPawn.Value!.AbsOrigin;

            if (clientOrigin == null)
                continue;

            playerPositionHistory[client].Update(new Vector(clientOrigin.X, clientOrigin.Y, clientOrigin.Z));

            TimedPosition? lastLocation = playerPositionHistory[client].GetOldestPosition();

            if (lastLocation == null)
                continue;

            double distance = CalculateDistance(lastLocation.vector, clientOrigin);

            if (distance <= CampDetectionRadius.Value)
            {
                playerCampingTime[client] += CampDetectionInterval.Value;
                // string msg = $"You have been camping for {playerCampingTime[client]:F2} | secondsGlowingTime: {playerGlowingTime[client]:F2} \nCurrent Location: {clientOrigin.X:F2} {clientOrigin.Y:F2} {clientOrigin.Z:F2} | Compared Location: {lastLocation.vector.X:F2} {lastLocation.vector.Y:F2} {lastLocation.vector.Z:F2} \nLocation captured time {lastLocation.time:F2} | Difference: {distance:F2}";
                // client.PrintToCenterHtml(msg);
            }
            else
            {
                playerCampingTime[client] = 0.0F;
            }

            if (playerCampingTime[client] >= CampDetectionTime.Value)
            {
                if (playerGlowingTime[client] <= 0.0 && !isPlayerWarned[client])
                {
                    StartPlayerGlowing(client);
                    RecreateGlowingTimer(client);
                }

                playerGlowingTime[client] = CampMarkingTime.Value;
            }
        }
    }

    private HookResult OnSwitchTeam(EventPlayerTeam @event, GameEventInfo info)
    {
        CCSPlayerController? client = @event.Userid;

        if (client == null)
            return HookResult.Continue;

        if (!client.IsValid || /*client.IsBot ||*/ client.IsHLTV)
            return HookResult.Continue;

        StopPlayerGlowing(client);

        return HookResult.Continue;
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? client = @event.Userid;

        if (client == null)
            return HookResult.Continue;

        if (!client.IsValid || /*client.IsBot ||*/ client.IsHLTV)
            return HookResult.Continue;

        StopPlayerGlowing(client);

        return HookResult.Continue;
    }

    private HookResult OnRoundFreezeEnd(EventRoundFreezeEnd @event, GameEventInfo info)
    {
        isRoundStarted = true;
        foreach (CCSPlayerController client in Utilities.GetPlayers())
        {
            if (IsClientInformationAccessible(client))
                continue;

            InitClientInformation(client);
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        isRoundStarted = false;
        return HookResult.Continue;
    }

    private HookResult OnPlayerConnect(EventPlayerConnect @event, GameEventInfo info)
    {
        CCSPlayerController? client = @event.Userid;

        if (client == null)
            return HookResult.Continue;

        if (!client.IsValid || /*client.IsBot ||*/ client.IsHLTV)
            return HookResult.Continue;

        InitClientInformation(client);

        return HookResult.Continue;
    }

    private HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? client = @event.Userid;

        if (client == null)
            return HookResult.Continue;

        if (!client.IsValid || /*client.IsBot ||*/ client.IsHLTV)
            return HookResult.Continue;

        if (IsClientInformationAccessible(client))
            return HookResult.Continue;

        InitClientInformation(client);
        return HookResult.Continue;
    }

    private void OnClientPutInServer(int clientSlot)
    {
        CCSPlayerController? client = Utilities.GetPlayerFromSlot(clientSlot);

        if (client == null)
            return;

        if (!client.IsValid || /*client.IsBot ||*/ client.IsHLTV)
            return;

        if (IsClientInformationAccessible(client))
            return;

        InitClientInformation(client);
    }

    private bool IsClientInformationAccessible(CCSPlayerController client)
    {
        return playerPositionHistory.ContainsKey(client) && playerCampingTime.ContainsKey(client) &&
               playerGlowingTime.ContainsKey(client) && isPlayerWarned.ContainsKey(client);
    }

    private void InitClientInformation(CCSPlayerController client)
    {
        SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Initializing the client information.");
        playerPositionHistory[client] = new PlayerPositionHistory(
            (int)(CampDetectionTime.Value /
                  CampDetectionInterval.Value));
        playerCampingTime[client] = 0.0F;
        playerGlowingTime[client] = 0.0F;
        isPlayerWarned[client] = false;
        SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Initialized.");
    }

    private void RecreateGlowingTimer(CCSPlayerController client)
    {
        float timerInterval = CampDetectionInterval.Value;
        isPlayerWarned[client] = true;
        SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Warned as camping.");
        client.PrintToCenterAlert(Plugin.Localizer["AntiCamp.Notification.DetectedAsCamping"]);

        void Check()
        {
            Plugin.AddTimer(timerInterval, () =>
            {
                if (playerGlowingTime[client] <= 0.0)
                {
                    isPlayerWarned[client] = false;
                    SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Glowing timer has ended.");
                    return;
                }

                playerGlowingTime[client] -= timerInterval;
                Check();
            }, TimerFlags.STOP_ON_MAPCHANGE);
        }

        ;
        Check();
    }

    private void StartPlayerGlowing(CCSPlayerController client)
    {
        SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Start player glow");
        playerGlowingTime[client] = 0.0F;
        CCSPlayerPawn playerPawn = client.PlayerPawn.Value!;

        SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Creating overlay entity.");
        CBaseModelEntity? modelGlow = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");
        CBaseModelEntity? modelRelay = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");

        if (modelGlow == null || modelRelay == null)
        {
            SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Failed to create glowing entity!");
            return;
        }


        string playerModel = GetPlayerModel(client);

        SimpleLogging.LogTrace($"[Anti Camp] [Player {client.PlayerName}] player model: {playerModel}");
        if (playerModel == string.Empty)
            return;

        // Code from Joakim in CounterStrikeSharp Discord
        // https://discord.com/channels/1160907911501991946/1235212931394834432/1245928951449387009
        SimpleLogging.LogTrace($"[Anti Camp] [Player {client.PlayerName}] Setting player model to overlay entity.");
        modelRelay.SetModel(playerModel);
        modelRelay.Spawnflags = 256u;
        modelRelay.RenderMode = RenderMode_t.kRenderNone;
        modelRelay.DispatchSpawn();

        modelGlow.SetModel(playerModel);
        modelGlow.Spawnflags = 256u;
        modelGlow.RenderMode = RenderMode_t.kRenderTransColor;
        modelGlow.Render = Color.FromArgb(1, 255, 255, 255);
        modelGlow.DispatchSpawn();

        SimpleLogging.LogTrace($"[Anti Camp] [Player {client.PlayerName}] Changing overlay entity's render mode.");
        if (client.Team == CsTeam.Terrorist)
        {
            modelGlow.Glow.GlowColorOverride = Color.Red;
        }
        else
        {
            modelGlow.Glow.GlowColorOverride = Color.Blue;
        }

        modelGlow.Glow.GlowRange = 5000;
        modelGlow.Glow.GlowTeam = -1;
        modelGlow.Glow.GlowType = 3;
        modelGlow.Glow.GlowRangeMin = 0;

        modelRelay.AcceptInput("FollowEntity", playerPawn, modelRelay, "!activator");
        modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");

        playerGlowingEntity[client] = (modelGlow, modelRelay);
    }

    private void StopPlayerGlowing(CCSPlayerController client)
    {
        SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Glow removed");

        if (!playerGlowingEntity.TryGetValue(client, out var entities))
            return;

        if (entities.glowEntity != null && entities.glowEntity.IsValid)
        {
            playerGlowingEntity[client].glowEntity!.Remove();
        }

        if (entities.relayEntity != null && entities.relayEntity.IsValid)
        {
            playerGlowingEntity[client].relayEntity?.Remove();
        }
    }


    private static double CalculateDistance(Vector vec1, Vector vec2)
    {
        double deltaX = vec1.X - vec2.X;
        double deltaY = vec1.Y - vec2.Y;
        double deltaZ = vec1.Z - vec2.Z;

        double distanceSquared = Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2);
        return Math.Sqrt(distanceSquared);
    }

    private static string GetPlayerModel(CCSPlayerController client)
    {
        if (client.PlayerPawn.Value == null)
            return string.Empty;

        CCSPlayerPawn playerPawn = client.PlayerPawn.Value;

        if (playerPawn.CBodyComponent == null)
            return string.Empty;

        if (playerPawn.CBodyComponent.SceneNode == null)
            return string.Empty;

        return playerPawn.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
    }

    private enum PlayerGlowStatus
    {
        GLOWING,
        NOT_GLOWING,
    }
}