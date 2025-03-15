using System.Runtime.CompilerServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore {
    public class PluginSettings {

        private static PluginSettings? settingsInstance;

        public const string CONFIG_FOLDER = "csgo/cfg/lupercalia/";
        private const string CONFIG_FILE = "mgcore.cfg";

        public static PluginSettings getInstance {
            get {
                if(settingsInstance == null)
                    throw new InvalidOperationException("Settings instance is not initialized yet.");
                
                return settingsInstance;
            }
        }

        /*
        *   Team based body color
        */
        public readonly FakeConVar<bool> m_CVIsTeamColorEnabled = new("lp_mg_teamcolor_enabled", "Should apply team color after respawn", true);
        public readonly FakeConVar<string> m_CVTeamColorCT  = new("lp_mg_teamcolor_ct", "Counter Terrorist's Body color. R, G, B", "0, 0, 255");
        public readonly FakeConVar<string> m_CVTeamColorT = new("lp_mg_teamcolor_t", "Terrorist's Body color. R, G, B", "255, 0, 0");

        /*
        *   Team scramble
        */
        public readonly FakeConVar<bool> m_CVIsScrambleEnabled = new("lp_mg_teamscramble_enabled", "Should team is scrambled after round end", true);

        /*
        *   Vote round/map restart
        */
        public readonly FakeConVar<double> m_CVVoteMapRestartAllowedTime = new("lp_mg_vmr_allowed_time", "How long allowed to use vote command after map loaded in seconds.", 60.0D);
        public readonly FakeConVar<float> m_CVVoteMapRestartThreshold = new("lp_mg_vmr_vote_threshold", "How percent of votes required to initiate the map restart.", 0.7F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1.0F));
        public readonly FakeConVar<float> m_CVVoteMapRestartRestartTime = new("lp_mg_vmr_restart_time", "How long to take restarting map after vote passed.", 10.0F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, float.MaxValue));
        public readonly FakeConVar<float> m_CVVoteRoundRestartThreshold = new("lp_mg_vrr_vote_threshold", "How percent of votes required to initiate the round restart.", 0.7F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, 1.0F));
        public readonly FakeConVar<float> m_CVVoteRoundRestartRestartTime = new("lp_mg_vrr_restart_time", "How long to take restarting round after vote passed.", 10.0F, ConVarFlags.FCVAR_NONE, new RangeValidator<float>(0.0F, float.MaxValue));

        /*
        *   Round end enhancement
        */
        public readonly FakeConVar<bool> m_CVIsRoundEndDamageImmunityEnabled = new("lp_mg_redi_enabled", "Should player grant damage immunity after round end until next round starts.", false);
        public readonly FakeConVar<bool> m_CVIsRoundEndWeaponStripEnabled = new("lp_mg_rews_enabled", "Should player's weapons are removed before new round starts.", true);
        public readonly FakeConVar<bool> m_CVIsRoundEndDeathMatchEnabled = new("lp_mg_redm_enabled", "Should enable round end death match?", true);

        /*
        *   Scheduled shutdown
        */
        public readonly FakeConVar<string> m_CVScheduledShutdownTime = new("lp_mg_scheduled_shutdown_time", "Server will be shutdown in specified time. Format is HHmm", "0500");
        public readonly FakeConVar<int> m_CVScheduledShutdownWarningTime = new("lp_mg_scheduled_shutdown_warn_time", "Show shutdown warning countdown if lp_mg_scheduled_shutdown_round_end is false.", 10);
        public readonly FakeConVar<bool> m_CVScheduledShutdownRoundEnd = new("lp_mg_scheduled_shutdown_round_end", "When set to true server will be shutdown after round end.", true);

        /*
        *   Auto respawn
        */
        public readonly FakeConVar<bool> m_CVAutoRespawnEnabled = new("lp_mg_auto_respawn_enabled", "Auto respawn feature is enabled", false);
        public readonly FakeConVar<float> m_CVAutoRespawnSpawnKillingDetectionTime = new("lp_mg_auto_respawn_repeat_kill_time", "Seconds to detect as spawn killing.", 1.0F);
        public readonly FakeConVar<float> m_CVAutoRespawnSpawnTime = new("lp_mg_auto_respawn_time", "How long to respawn after death.", 1.0F);

        /*
        *   Anti camp
        */
        public readonly FakeConVar<bool> m_CVAntiCampEnabled = new("lp_mg_anti_camp_enabled", "Anti camp enabled", true);
        public readonly FakeConVar<float> m_CVAntiCampDetectionTime = new("lp_mg_anti_camp_detection_time", "How long to take detected as camping in seconds.", 10.0F);
        public readonly FakeConVar<double> m_CVAntiCampDetectionRadius = new("lp_mg_anti_camp_detection_radius", "Range of area for player should move for avoiding the detected as camping.", 200.0F);
        public readonly FakeConVar<float> m_CVAntiCampDetectionInterval = new("lp_mg_anti_camp_detection_interval", "Interval to run camping check in seconds.", 0.1F);
        public readonly FakeConVar<float> m_CVAntiCampMarkingTime = new("lp_mg_anti_camp_glowing_time", "How long to detected player keep glowing.", 10.0F);

        /*
        *   Map config
        */
        public readonly FakeConVar<int> m_CVMapConfigExecutionTiming = new("lp_mg_mapcfg_execution_timing", "When configs are executed? 0: Does nothing, 1: Execute on map start, 2: Execute on every round start, 3: Execute on map transition and every round start", 1, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 3));
        public readonly FakeConVar<int> m_CVMapConfigType = new ("lp_mg_mapcfg_type", "Map configuration type. 0: disabled, 1: Exact match, 2: Partial Match", 1, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0, 2));

        /*
        *   Plugin debugging
        */

        public readonly FakeConVar<int> m_CVPluginDebugLevel = new("lp_mg_debug_level", "0: Nothing, 1: Print debug message, 2: Print debug, trace message", 0, ConVarFlags.FCVAR_NONE, new RangeValidator<int>(0,2));
        public readonly FakeConVar<bool> m_CVPluginDebugShowClientConsole = new("lp_mg_debug_show_console", "Debug message shown in client console?", false);

        /*
        *   Omikuji
        */
        public readonly FakeConVar<int> m_CVOmikujiEventWeightMisc = new("lp_mg_omikuji_event_weight_misc", "Weight of misc event. You can set to any value.", 90);
        public readonly FakeConVar<int> m_CVOmikujiEventWeightBad = new("lp_mg_omikuji_event_weight_bad", "Weight of bad event. You can set to any value.", 5);
        public readonly FakeConVar<int> m_CVOmikujiEventWeightLucky = new("lp_mg_omikuji_event_weight_lucky", "Weight of lucky event. You can set to any value.", 5);
        public readonly FakeConVar<double> m_CVOmikujiCommandCooldown = new("lp_mg_omikuji_command_cooldown", "Cooldown of omikuji command.", 60.0D);
        public readonly FakeConVar<int> m_CVOmikujiCommandExecutionDelayMin = new("lp_mg_omikuji_command_execution_delay_min", "Minimum time of omikuji event executed after execution of command.", 5);
        public readonly FakeConVar<int> m_CVOmikujiCommandExecutionDelayMax = new("lp_mg_omikuji_command_execution_delay_max", "Maximum time of omikuji event executed after execution of command.", 10);

        /*
        *   Omikuji - Chicken
        */
        public readonly FakeConVar<float> m_CVOmikujiEventChickenTime = new("lp_mg_omikuji_event_chicken_time", "How long that chicken alive?", 5.0F);
        public readonly FakeConVar<float> m_CVOmikujiEventChickenBodyScale = new("lp_mg_omikuji_event_chicken_body_scale", "Body size of the chicken. Default size is 1.0", 1.0F);
        public readonly FakeConVar<double> m_CVOmikujiEventChickenSelectionWeight = new("lp_mg_omikuji_event_chicken_selection_weight", "Selection weight of this event", 160.0D);

        /*
        *   Omikuji - Screen shake
        */
        public readonly FakeConVar<float> m_CVOmikujiEventScreenShakeAmplitude = new("lp_mg_omikuji_event_screen_shake_amplitude", "How far away from the normal position the camera will wobble. Should be a range between 0 and 16.", 1000.0F);
        public readonly FakeConVar<float> m_CVOmikujiEventScreenShakeDuration = new("lp_mg_omikuji_event_screen_shake_duration", "The length of time in which to shake the player's screens.", 5.0F);
        public readonly FakeConVar<float> m_CVOmikujiEventScreenShakeFrequency = new("lp_mg_omikuji_event_screen_shake_frequency", "How many times per second to change the direction of the camera wobble. 40 is generally enough; values higher are hardly distinguishable.", 1000.0F);
        public readonly FakeConVar<double> m_CVOmikujiEventScreenShakeSelectionWeight = new("lp_mg_omikuji_event_screen_shake_selection_weight", "Selection weight of this event", 30.0D);

        /*
        *   Omikuji - Player Heal
        */
        public readonly FakeConVar<int> m_CVOmikujiEventPlayerHeal = new("lp_mg_omikuji_event_player_heal_amount", "How many health healed when event occur", 100);
        public readonly FakeConVar<double> m_CVOmikujiEventPlayerHealSelectionWeight = new("lp_mg_omikuji_event_player_heal_selection_weight", "Selection weight of this event", 30.0D);

        /*
        *   Omikuji - Gravity
        */
        public readonly FakeConVar<int> m_CVOmikujiEventGravityMax = new("lp_mg_omikuji_event_gravity_max", "Maximum value of sv_gravity", 800);
        public readonly FakeConVar<int> m_CVOmikujiEventGravityMin = new("lp_mg_omikuji_event_gravity_min", "Minimal value of sv_gravity", 100);
        public readonly FakeConVar<float> m_CVOmikujiEventGravityRestoreTime = new("lp_mg_omikuji_event_gravity_restore_time", "How long to take gravity restored in seconds.", 10.0F);
        public readonly FakeConVar<double> m_CVOmikujiEventGravitySelectionWeight = new("lp_mg_omikuji_event_gravity_selection_weight", "Selection weight of this event", 30.0D);

        /*
        *   Omikuji - Freeze
        */
        public readonly FakeConVar<float> m_CVOmikujiEventPlayerFreeze = new("lp_mg_omikuji_event_player_freeze_time", "How long to player freeze in seconds.", 3.0F);
        public readonly FakeConVar<double> m_CVOmikujiEventPlayerFreezeSelectionWeight = new("lp_mg_omikuji_event_player_freeze_selection_weight", "Selection weight of this event", 30.0D);

        /*
        *   Omikuji - GiveRandomItem
        */
        public readonly FakeConVar<int> m_CVOmikujiEventGiveRandomItemAvoidCount = new("lp_mg_omikuji_event_give_random_item_avoid_duplication_history", "How many histories save to avoid give duplicated item.", 10);
        public readonly FakeConVar<double> m_CVOmikujiEventGiveRandomItemSelectionWeight = new("lp_mg_omikuji_event_give_random_item_selection_weight", "Selection weight of this event", 30.0D);

        /*
        *   Omikuji - Player Slap
        */
        public readonly FakeConVar<int> m_CVOmikujiEventPlayerSlapPowerMin = new("lp_mg_omikuji_event_player_slap_power_min", "Minimal power of slap.", 0);
        public readonly FakeConVar<int> m_CVOmikujiEventPlayerSlapPowerMax = new("lp_mg_omikuji_event_player_slap_power_max", "Maximum power of slap.", 30000);
        public readonly FakeConVar<double> m_CVOmikujiEventPlayerSlapSelectionWeight = new("lp_mg_omikuji_event_player_slap_item_selection_weight", "Selection weight of this event", 30.0D);

        /*
        *   Omikuji - Selection weights
        */
        public readonly FakeConVar<double> m_CVOmikujiEventNothingSelectionWeight = new("lp_mg_omikuji_event_nothing_swap_selection_weight", "Selection weight of this event", 30.0D);
        public readonly FakeConVar<double> m_CVOmikujiEventPlayerWishingSelectionWeight = new("lp_mg_omikuji_event_player_wishing_selection_weight", "Selection weight of this event", 30.0D);
        public readonly FakeConVar<double> m_CVOmikujiEventPlayerLocationSwapSelectionWeight = new("lp_mg_omikuji_event_player_location_swap_selection_weight", "Selection weight of this event", 30.0D);
        public readonly FakeConVar<double> m_CVOmikujiEventPlayerRespawnSelectionWeight = new("lp_mg_omikuji_event_player_respawn_selection_weight", "Selection weight of this event", 30.0D);
        public readonly FakeConVar<double> m_CVOmikujiEventAllPlayerRespawnSelectionWeight = new("lp_mg_omikuji_event_all_player_respawn_selection_weight", "Selection weight of this event", 30.0D);

        /*
         * Hide Legs
         */
        public readonly FakeConVar<bool> m_CVHideLegsEnabled = new("lp_mg_hide_legs_enabled", "Hide legs feature is enabled", false);

        /*
         * External view
         */
        public readonly FakeConVar<bool> m_CVExternalViewEnabled = new("lp_mg_external_view_enabled", "External view feature is enabled", false);

        /*
        *   For debugging purpose
        */
        public readonly FakeConVar<bool> m_CVDebuggingEnabled = new("lp_mg_debug_enabled", "Enable debugging feature?", false);

        /*
        *   Misc commands
        */
        public readonly FakeConVar<bool> m_CVMiscCMDGiveKnifeEnabled = new("lp_mg_misc_cmd_give_knife", "Is give knife command enabled?", false);

        private LupercaliaMGCore m_CSSPlugin;

        public PluginSettings(LupercaliaMGCore plugin) {
            plugin.Logger.LogDebug("Setting the instance info");
            settingsInstance = this;
            plugin.Logger.LogDebug("Setting the plugin instance");
            m_CSSPlugin = plugin;
            plugin.Logger.LogDebug("Initializing the settings");
            initializeSettings();
            plugin.Logger.LogDebug("Registering the fake convar");
            m_CSSPlugin.RegisterFakeConVars(typeof(PluginSettings), this);
        }

        public bool initializeSettings() {
            m_CSSPlugin.Logger.LogDebug("Generate path to config folder");
            string configFolder = Path.Combine(Server.GameDirectory, CONFIG_FOLDER);

            m_CSSPlugin.Logger.LogDebug("Checking existence of config folder");
            if(!Directory.Exists(configFolder)) {
                m_CSSPlugin.Logger.LogInformation($"Failed to find the config folder. Trying to generate...");

                Directory.CreateDirectory(configFolder);

                if(!Directory.Exists(configFolder)) {
                    m_CSSPlugin.Logger.LogError($"Failed to generate the Config folder!");
                    return false;
                }
            }

            m_CSSPlugin.Logger.LogDebug("Generate path to config file");
            string configLocation = Path.Combine(configFolder, CONFIG_FILE);

            m_CSSPlugin.Logger.LogDebug("Checking existence of config file");
            if(!File.Exists(configLocation)) {
                m_CSSPlugin.Logger.LogInformation($"Failed to find the config file. Trying to generate...");

                try {
                    generateCFG(configLocation);
                } catch(Exception e) {
                    m_CSSPlugin.Logger.LogError($"Failed to generate config file!\n{e.StackTrace}");
                    return false;
                }
                
                m_CSSPlugin.Logger.LogInformation($"Config file created.");
            }

            m_CSSPlugin.Logger.LogDebug("Executing config");
            Server.ExecuteCommand($"exec lupercalia/{CONFIG_FILE}");
            return true;
        }

        private void generateCFG(string configPath) {
            StreamWriter config = File.CreateText(configPath);

            /*
            *   Team based body color
            */
            writeConVarConfig(config, m_CVIsTeamColorEnabled);
            writeConVarConfig(config, m_CVTeamColorCT);
            writeConVarConfig(config, m_CVTeamColorT);
            config.WriteLine("\n");


            /*
            *   Team scramble
            */
            writeConVarConfig(config, m_CVIsScrambleEnabled);
            config.WriteLine("\n");


            /*
            *   Vote round/map restart
            */
            writeConVarConfig(config, m_CVVoteMapRestartAllowedTime);
            writeConVarConfig(config, m_CVVoteMapRestartThreshold);
            writeConVarConfig(config, m_CVVoteMapRestartRestartTime);
            writeConVarConfig(config, m_CVVoteRoundRestartThreshold);
            writeConVarConfig(config, m_CVVoteRoundRestartRestartTime);
            config.WriteLine("\n");


            /*
            *   Round end enhancement
            */
            writeConVarConfig(config, m_CVIsRoundEndDamageImmunityEnabled);
            writeConVarConfig(config, m_CVIsRoundEndWeaponStripEnabled);
            writeConVarConfig(config, m_CVIsRoundEndDeathMatchEnabled);
            config.WriteLine("\n");


            /*
            *   Scheduled shutdown
            */
            writeConVarConfig(config, m_CVScheduledShutdownTime);
            writeConVarConfig(config, m_CVScheduledShutdownWarningTime);
            writeConVarConfig(config, m_CVScheduledShutdownRoundEnd);
            config.WriteLine("\n");


            /*
            *   Auto respawn
            */
            writeConVarConfig(config, m_CVAutoRespawnEnabled);
            writeConVarConfig(config, m_CVAutoRespawnSpawnKillingDetectionTime);
            writeConVarConfig(config, m_CVAutoRespawnSpawnTime);
            config.WriteLine("\n");



            /*
            *   Anti camp
            */
            writeConVarConfig(config, m_CVAntiCampEnabled);
            writeConVarConfig(config, m_CVAntiCampDetectionTime);
            writeConVarConfig(config, m_CVAntiCampDetectionRadius);
            writeConVarConfig(config, m_CVAntiCampDetectionInterval);
            writeConVarConfig(config, m_CVAntiCampMarkingTime);
            config.WriteLine("\n");


            /*
            *   Map config
            */
            writeConVarConfig(config, m_CVMapConfigType);
            writeConVarConfig(config, m_CVMapConfigExecutionTiming);
            config.WriteLine("\n");


            /*
            *   Plugin debug
            */
            writeConVarConfig(config, m_CVPluginDebugLevel);
            writeConVarConfig(config, m_CVPluginDebugShowClientConsole);
            config.WriteLine("\n");

            /*
            *   Omikuji
            */
            writeConVarConfig(config, m_CVOmikujiEventWeightMisc);
            writeConVarConfig(config, m_CVOmikujiEventWeightBad);
            writeConVarConfig(config, m_CVOmikujiEventWeightLucky);
            writeConVarConfig(config, m_CVOmikujiCommandCooldown);
            writeConVarConfig(config, m_CVOmikujiCommandExecutionDelayMin);
            writeConVarConfig(config, m_CVOmikujiCommandExecutionDelayMax);
            config.WriteLine("\n");

            /*
            *   Omikuji - Chicken
            */
            writeConVarConfig(config, m_CVOmikujiEventChickenTime);
            writeConVarConfig(config, m_CVOmikujiEventChickenBodyScale);
            writeConVarConfig(config, m_CVOmikujiEventChickenSelectionWeight);
            config.WriteLine("\n");

            /*
            *   Omikuji - Screen shake
            */
            writeConVarConfig(config, m_CVOmikujiEventScreenShakeAmplitude);
            writeConVarConfig(config, m_CVOmikujiEventScreenShakeDuration);
            writeConVarConfig(config, m_CVOmikujiEventScreenShakeFrequency);
            writeConVarConfig(config, m_CVOmikujiEventScreenShakeSelectionWeight);
            config.WriteLine("\n");

            /*
            *   Omikuji - Player Heal
            */
            writeConVarConfig(config, m_CVOmikujiEventPlayerHeal);
            writeConVarConfig(config, m_CVOmikujiEventPlayerHealSelectionWeight);
            config.WriteLine("\n");

            /*
            *   Omikuji - Gravity
            */
            writeConVarConfig(config, m_CVOmikujiEventGravityMax);
            writeConVarConfig(config, m_CVOmikujiEventGravityMin);
            writeConVarConfig(config, m_CVOmikujiEventGravityRestoreTime);
            writeConVarConfig(config, m_CVOmikujiEventGravitySelectionWeight);
            config.WriteLine("\n");

            /*
            *   Omikuji - Freeze
            */
            writeConVarConfig(config, m_CVOmikujiEventPlayerFreeze);
            writeConVarConfig(config, m_CVOmikujiEventPlayerFreezeSelectionWeight);
            config.WriteLine("\n");

            /*
            *   Omikuji - GiveRandomItem
            */ 
            writeConVarConfig(config, m_CVOmikujiEventGiveRandomItemAvoidCount);
            writeConVarConfig(config, m_CVOmikujiEventGiveRandomItemSelectionWeight);
            config.WriteLine("\n");

            /*
            *   Omikuji - Player Slap
            */
            writeConVarConfig(config, m_CVOmikujiEventPlayerSlapPowerMin);
            writeConVarConfig(config, m_CVOmikujiEventPlayerSlapPowerMax);
            writeConVarConfig(config, m_CVOmikujiEventPlayerSlapSelectionWeight);
            config.WriteLine("\n");

            /*
            *   Omikuji - Selection weights
            */
            writeConVarConfig(config, m_CVOmikujiEventNothingSelectionWeight);
            writeConVarConfig(config, m_CVOmikujiEventPlayerWishingSelectionWeight);
            writeConVarConfig(config, m_CVOmikujiEventPlayerLocationSwapSelectionWeight);
            writeConVarConfig(config, m_CVOmikujiEventPlayerRespawnSelectionWeight);
            writeConVarConfig(config, m_CVOmikujiEventAllPlayerRespawnSelectionWeight);
            config.WriteLine("\n");

            /*
             * Hide legs
             */
            writeConVarConfig(config, m_CVHideLegsEnabled);
            config.WriteLine("\n");

            /*
            *   For debugging purpose
            */
            writeConVarConfig(config, m_CVDebuggingEnabled);
            config.WriteLine("\n");

            /*
            *   Misc commands
            */
            writeConVarConfig(config, m_CVMiscCMDGiveKnifeEnabled);
            config.WriteLine("\n");

            config.Close();
        }

        private static void writeConVarConfig<T>(StreamWriter configFile, FakeConVar<T> convar)where T : IComparable<T>{
            configFile.WriteLine($"// {convar.Description}");
            if(typeof(T) == typeof(bool)) {
                var conValue = convar.Value;
                bool value = Unsafe.As<T, bool>(ref conValue);
                configFile.WriteLine($"{convar.Name} {Convert.ToInt32(value)}");
            } else {
                configFile.WriteLine($"{convar.Name} {convar.Value}");
            }
            configFile.WriteLine();
        }
    }
}