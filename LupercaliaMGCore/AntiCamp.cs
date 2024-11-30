using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore {
    public class AntiCamp {
        private LupercaliaMGCore m_CSSPlugin;
        private CounterStrikeSharp.API.Modules.Timers.Timer timer;
        private Dictionary<CCSPlayerController, (CBaseModelEntity? glowEntity, CBaseModelEntity? relayEntity)> playerGlowingEntity = new Dictionary<CCSPlayerController, (CBaseModelEntity? glowEntity, CBaseModelEntity? relayEntity)>();
        private Dictionary<CCSPlayerController, float> playerCampingTime = new Dictionary<CCSPlayerController, float>();
        private Dictionary<CCSPlayerController, PlayerPositionHistory> playerPositionHistory= new Dictionary<CCSPlayerController, PlayerPositionHistory>();
        private Dictionary<CCSPlayerController, float> playerGlowingTime = new Dictionary<CCSPlayerController, float>();
        private Dictionary<CCSPlayerController, bool> isPlayerWarned = new Dictionary<CCSPlayerController, bool>();
        private Dictionary<CCSPlayerController, CounterStrikeSharp.API.Modules.Timers.Timer> glowingTimer = new Dictionary<CCSPlayerController, CounterStrikeSharp.API.Modules.Timers.Timer>();

        private bool isRoundStarted = false;

        public AntiCamp(LupercaliaMGCore plugin, bool hotReload) {
            m_CSSPlugin = plugin;

            m_CSSPlugin.RegisterEventHandler<EventPlayerConnect>(onPlayerConnect, HookMode.Pre);
            m_CSSPlugin.RegisterEventHandler<EventPlayerConnectFull>(onPlayerConnectFull, HookMode.Pre);
            m_CSSPlugin.RegisterListener<Listeners.OnClientPutInServer>(OnClientPutInServer);

            m_CSSPlugin.RegisterEventHandler<EventRoundFreezeEnd>(onRoundFeezeEnd, HookMode.Post);
            m_CSSPlugin.RegisterEventHandler<EventRoundEnd>(onRoundEnd, HookMode.Post);


            if(hotReload) {
                bool isFreezeTimeEnded = Utilities.FindAllEntitiesByDesignerName<CCSGameRulesProxy>("cs_gamerules").First().GameRules?.FreezePeriod ?? false;

                if(isFreezeTimeEnded) {
                    isRoundStarted = true;
                }

                foreach(var client in Utilities.GetPlayers()) {
                    if(!client.IsValid || client.IsBot || client.IsHLTV)
                        continue;

                    initClientInformation(client);
                }
            }

            timer = m_CSSPlugin.AddTimer(PluginSettings.getInstance.m_CVAntiCampDetectionInterval.Value, checkPlayerIsCamping, TimerFlags.REPEAT);
        }

        private void checkPlayerIsCamping() {
            if(!isRoundStarted || !PluginSettings.getInstance.m_CVAntiCampEnabled.Value)
                return;

            foreach(var client in Utilities.GetPlayers()) {
                if(!client.IsValid || client.IsBot || client.IsHLTV)
                    continue;

                if(client.Team == CsTeam.None || client.Team == CsTeam.Spectator)
                    continue;

                if(client.PlayerPawn.Value == null || client.PlayerPawn.Value.LifeState != (byte)LifeState_t.LIFE_ALIVE)
                    continue;

                if(!isClientInformationAccessible(client))
                    continue;

                Vector? clientOrigin = client.PlayerPawn.Value!.AbsOrigin;
                
                if(clientOrigin == null)
                    continue;

                playerPositionHistory[client].Update(new Vector(clientOrigin.X, clientOrigin.Y, clientOrigin.Z));

                TimedPosition? lastLocation = playerPositionHistory[client].GetOldestPosition();

                if(lastLocation == null)
                    continue;

                double distance = calculateDistance(lastLocation.vector, clientOrigin);

                if(distance <= PluginSettings.getInstance.m_CVAntiCampDetectionRadius.Value) {
                    playerCampingTime[client] += PluginSettings.getInstance.m_CVAntiCampDetectionInterval.Value;
                    // string msg = $"You have been camping for {playerCampingTime[client]:F2} | secondsGlowingTime: {playerGlowingTime[client]:F2} \nCurrent Location: {clientOrigin.X:F2} {clientOrigin.Y:F2} {clientOrigin.Z:F2} | Compared Location: {lastLocation.vector.X:F2} {lastLocation.vector.Y:F2} {lastLocation.vector.Z:F2} \nLocation captured time {lastLocation.time:F2} | Difference: {distance:F2}";
                    // client.PrintToCenterHtml(msg);
                } else {
                    playerCampingTime[client] = 0.0F;
                }

                if(playerCampingTime[client] >= PluginSettings.getInstance.m_CVAntiCampDetectionTime.Value) {

                    if(playerGlowingTime[client] <= 0.0 && !isPlayerWarned[client]) {
                        startPlayerGlowing(client);
                        recreateGlowingTimer(client);
                    }

                    playerGlowingTime[client] = PluginSettings.getInstance.m_CVAntiCampMarkingTime.Value;
                }
            }
        }

        private HookResult onSwitchTeam(EventPlayerTeam @event, GameEventInfo info) {
            CCSPlayerController? client = @event.Userid;

            if(client == null) 
                return HookResult.Continue;

            if(!client.IsValid || client.IsBot || client.IsHLTV)
                return HookResult.Continue;

            stopPlayerGlowing(client);

            return HookResult.Continue;
        }

        private HookResult onPlayerDeath(EventPlayerDeath @event, GameEventInfo info) {
            CCSPlayerController? client = @event.Userid;

            if(client == null) 
                return HookResult.Continue;

            if(!client.IsValid || client.IsBot || client.IsHLTV)
                return HookResult.Continue;

            stopPlayerGlowing(client);
            
            return HookResult.Continue;
        }

        private HookResult onRoundFeezeEnd(EventRoundFreezeEnd @event, GameEventInfo info) {
            isRoundStarted = true;
            foreach(CCSPlayerController client in Utilities.GetPlayers()) {
                if(isClientInformationAccessible(client))
                    continue;
                
                initClientInformation(client);
            }
            return HookResult.Continue;
        }

        private HookResult onRoundEnd(EventRoundEnd @event, GameEventInfo info) {
            isRoundStarted = false;
            return HookResult.Continue;
        }

        private HookResult onPlayerConnect(EventPlayerConnect @event, GameEventInfo info) {
            CCSPlayerController? client = @event.Userid;

            if(client == null) 
                return HookResult.Continue;

            if(!client.IsValid || client.IsBot || client.IsHLTV)
                return HookResult.Continue;

            initClientInformation(client);

            return HookResult.Continue;
        }

        private HookResult onPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info) {
            CCSPlayerController? client = @event.Userid;

            if(client == null) 
                return HookResult.Continue;

            if(!client.IsValid || client.IsBot || client.IsHLTV)
                return HookResult.Continue;
            
            if(isClientInformationAccessible(client))
                return HookResult.Continue;

            initClientInformation(client);
            return HookResult.Continue;
        }

        private void OnClientPutInServer(int clientSlot) {
            CCSPlayerController? client = Utilities.GetPlayerFromSlot(clientSlot);

            if(client == null)
                return;

            if(!client.IsValid || client.IsBot || client.IsHLTV)
                return;
            
            if(isClientInformationAccessible(client))
                return;
            
            initClientInformation(client);
        }

        private bool isClientInformationAccessible(CCSPlayerController client) {
            return playerPositionHistory.ContainsKey(client) && playerCampingTime.ContainsKey(client) && playerGlowingTime.ContainsKey(client) && isPlayerWarned.ContainsKey(client);
        }

        private void initClientInformation(CCSPlayerController client) {
            SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Initializing the client information.");
            playerPositionHistory[client] = new PlayerPositionHistory((int)(PluginSettings.getInstance.m_CVAntiCampDetectionTime.Value / PluginSettings.getInstance.m_CVAntiCampDetectionInterval.Value));
            playerCampingTime[client] = 0.0F;
            playerGlowingTime[client] = 0.0F;
            isPlayerWarned[client] = false;
            SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Initialized.");
        }

        private void recreateGlowingTimer(CCSPlayerController client) {
            float timerInterval = PluginSettings.getInstance.m_CVAntiCampDetectionInterval.Value;
            isPlayerWarned[client] = true;
            SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Warned as camping.");
            client.PrintToCenterAlert(m_CSSPlugin.Localizer["AntiCamp.Notification.DetectedAsCamping"]);
            glowingTimer[client] = m_CSSPlugin.AddTimer(timerInterval, () => {
                // Fixing spitice#3 : KeyNotFoundException by using try-catch
                // `playerGlowingTime[client] <= 0.0F` seems to be problematic but not 100% sure
                try
                {
                    if (playerGlowingTime[client] <= 0.0F)
                    {
                        stopPlayerGlowing(client);
                        isPlayerWarned[client] = false;
                        glowingTimer[client].Kill();
                    }
                    playerGlowingTime[client] -= timerInterval;
                }
                catch (Exception e)
                {
                    SimpleLogging.LogDebug($"[Anti Camp] Failed to update Glowing Timer. Terminating...");
                    glowingTimer[client].Kill();
                    isPlayerWarned[client] = false;
                    playerGlowingTime[client] = 0.0F;
                    return;
                }
            }, TimerFlags.REPEAT);
        }

        private void startPlayerGlowing(CCSPlayerController client) {
            SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Start player glow");
            playerGlowingTime[client] = 0.0F;
            CCSPlayerPawn playerPawn = client.PlayerPawn.Value!;

            SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Creating overlay entity.");
            CBaseModelEntity? modelGlow = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");
            CBaseModelEntity? modelRelay = Utilities.CreateEntityByName<CBaseModelEntity>("prop_dynamic");
            
            if(modelGlow == null || modelRelay == null) {
                SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Failed to create glowing entity!");
                return;
            }


            string playerModel = getPlayerModel(client);

            SimpleLogging.LogTrace($"[Anti Camp] [Player {client.PlayerName}] player model: {playerModel}");
            if(playerModel == "")
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
            modelGlow.DispatchSpawn();

            SimpleLogging.LogTrace($"[Anti Camp] [Player {client.PlayerName}] Changing overlay entity's render mode.");
            if(client.Team == CsTeam.Terrorist) {
                modelGlow.Glow.GlowColorOverride = Color.Red;
            }
            else {
                modelGlow.Glow.GlowColorOverride = Color.Blue;
            }
            modelGlow.Glow.GlowRange = 5000;
            modelGlow.Glow.GlowTeam = -1;
            modelGlow.Glow.GlowType = 3;
            modelGlow.Glow.GlowRangeMin = 100;

            modelRelay.AcceptInput("FollowEntity", playerPawn, modelRelay, "!activator");
            modelGlow.AcceptInput("FollowEntity", modelRelay, modelGlow, "!activator");

            playerGlowingEntity[client] = (modelGlow, modelRelay);
        }

        private void stopPlayerGlowing(CCSPlayerController client) {
            SimpleLogging.LogDebug($"[Anti Camp] [Player {client.PlayerName}] Glow removed");

            if(!playerGlowingEntity.TryGetValue(client, out var entities))
                return;

            if(entities.glowEntity != null && entities.glowEntity.IsValid) {
                playerGlowingEntity[client].glowEntity!.Remove();
            }
            if(entities.relayEntity != null && entities.relayEntity.IsValid) {
                playerGlowingEntity[client].relayEntity?.Remove();
            }
        }


        private static double calculateDistance(Vector vec1, Vector vec2) {
            double deltaX = vec1.X - vec2.X;
            double deltaY = vec1.Y - vec2.Y;
            double deltaZ = vec1.Z - vec2.Z;

            double distanceSquared = Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2) + Math.Pow(deltaZ, 2);
            return Math.Sqrt(distanceSquared);
        }

        private static string getPlayerModel(CCSPlayerController client) {
            if(client.PlayerPawn.Value == null)
                return "";

            CCSPlayerPawn playerPawn = client.PlayerPawn.Value;

            if(playerPawn.CBodyComponent == null)
                return "";

            if(playerPawn.CBodyComponent.SceneNode == null)
                return "";

            return playerPawn.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.ModelName;
        }

        private enum PlayerGlowStatus {
            GLOWING,
            NOT_GLOWING,
        }
    }
}