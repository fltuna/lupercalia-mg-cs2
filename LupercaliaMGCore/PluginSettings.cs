using System.Runtime.CompilerServices;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Cvars.Validators;
using Microsoft.Extensions.Logging;

namespace LupercaliaMGCore;

public class PluginSettings
{
    private static PluginSettings? settingsInstance;

    public const string ConfigFolder = "csgo/cfg/lupercalia/";
    private const string ConfigFile = "mgcore.cfg";

    public static PluginSettings GetInstance
    {
        get
        {
            if (settingsInstance == null)
                throw new InvalidOperationException("Settings instance is not initialized yet.");

            return settingsInstance;
        }
    }

    /*
     *   Plugin debugging
     */

    public readonly FakeConVar<int> m_CVPluginDebugLevel = new("lp_mg_debug_level",
        "0: Nothing, 1: Print debug message, 2: Print debug, trace message", 0, ConVarFlags.FCVAR_NONE,
        new RangeValidator<int>(0, 2));

    public readonly FakeConVar<bool> m_CVPluginDebugShowClientConsole =
        new("lp_mg_debug_show_console", "Debug message shown in client console?", false);


    /*
     * Hide Legs
     */

    /*
     * External view
     */

    /*
     *   For debugging purpose
     */

    /*
     *   Misc commands
     */
    
    
    /*
     *   Course Weapons
     */

    /*
     *   Rocket commands
     */

    private LupercaliaMGCore m_CSSPlugin;

    public PluginSettings(LupercaliaMGCore plugin)
    {
        plugin.Logger.LogDebug("Setting the instance info");
        settingsInstance = this;
        plugin.Logger.LogDebug("Setting the plugin instance");
        m_CSSPlugin = plugin;
        plugin.Logger.LogDebug("Initializing the settings");
        initializeSettings();
        plugin.Logger.LogDebug("Registering the fake convar");
        m_CSSPlugin.RegisterFakeConVars(typeof(PluginSettings), this);
    }

    public bool initializeSettings()
    {
        m_CSSPlugin.Logger.LogDebug("Generate path to config folder");
        string configFolder = Path.Combine(Server.GameDirectory, ConfigFolder);

        m_CSSPlugin.Logger.LogDebug("Checking existence of config folder");
        if (!Directory.Exists(configFolder))
        {
            m_CSSPlugin.Logger.LogInformation($"Failed to find the config folder. Trying to generate...");

            Directory.CreateDirectory(configFolder);

            if (!Directory.Exists(configFolder))
            {
                m_CSSPlugin.Logger.LogError($"Failed to generate the Config folder!");
                return false;
            }
        }

        m_CSSPlugin.Logger.LogDebug("Generate path to config file");
        string configLocation = Path.Combine(configFolder, ConfigFile);

        m_CSSPlugin.Logger.LogDebug("Checking existence of config file");
        if (!File.Exists(configLocation))
        {
            m_CSSPlugin.Logger.LogInformation($"Failed to find the config file. Trying to generate...");

            try
            {
                generateCFG(configLocation);
            }
            catch (Exception e)
            {
                m_CSSPlugin.Logger.LogError($"Failed to generate config file!\n{e.StackTrace}");
                return false;
            }

            m_CSSPlugin.Logger.LogInformation($"Config file created.");
        }

        m_CSSPlugin.Logger.LogDebug("Executing config");
        Server.ExecuteCommand($"exec lupercalia/{ConfigFile}");
        return true;
    }

    private void generateCFG(string configPath)
    {
        StreamWriter config = File.CreateText(configPath);

        /*
         *   Team based body color
         */
        // writeConVarConfig(config, m_CVIsTeamColorEnabled);
        // writeConVarConfig(config, m_CVTeamColorCT);
        // writeConVarConfig(config, m_CVTeamColorT);
        // config.WriteLine("\n");
        //
        //
        // /*
        //  *   Team scramble
        //  */
        // writeConVarConfig(config, m_CVIsScrambleEnabled);
        // config.WriteLine("\n");
        //
        //
        // /*
        //  *   Vote round/map restart
        //  */
        // writeConVarConfig(config, m_CVVoteMapRestartAllowedTime);
        // writeConVarConfig(config, m_CVVoteMapRestartThreshold);
        // writeConVarConfig(config, m_CVVoteMapRestartRestartTime);
        // // writeConVarConfig(config, m_CVVoteRoundRestartThreshold);
        // // writeConVarConfig(config, m_CVVoteRoundRestartRestartTime);
        // config.WriteLine("\n");
        //
        //
        // /*
        //  *   Round end enhancement
        //  */
        // writeConVarConfig(config, m_CVIsRoundEndDamageImmunityEnabled);
        // writeConVarConfig(config, m_CVIsRoundEndWeaponStripEnabled);
        // writeConVarConfig(config, m_CVIsRoundEndDeathMatchEnabled);
        // config.WriteLine("\n");
        //
        //
        // /*
        //  *   Scheduled shutdown
        //  */
        // writeConVarConfig(config, m_CVScheduledShutdownTime);
        // writeConVarConfig(config, m_CVScheduledShutdownWarningTime);
        // writeConVarConfig(config, m_CVScheduledShutdownRoundEnd);
        // config.WriteLine("\n");
        //
        //
        // /*
        //  *   Auto respawn
        //  */
        // writeConVarConfig(config, m_CVAutoRespawnEnabled);
        // writeConVarConfig(config, m_CVAutoRespawnSpawnKillingDetectionTime);
        // writeConVarConfig(config, m_CVAutoRespawnSpawnTime);
        // config.WriteLine("\n");
        //
        //
        // /*
        //  *   Anti camp
        //  */
        // writeConVarConfig(config, m_CVAntiCampEnabled);
        // writeConVarConfig(config, m_CVAntiCampDetectionTime);
        // writeConVarConfig(config, m_CVAntiCampDetectionRadius);
        // writeConVarConfig(config, m_CVAntiCampDetectionInterval);
        // writeConVarConfig(config, m_CVAntiCampMarkingTime);
        // config.WriteLine("\n");
        //
        //
        // /*
        //  *   Map config
        //  */
        // writeConVarConfig(config, m_CVMapConfigType);
        // writeConVarConfig(config, m_CVMapConfigExecutionTiming);
        // config.WriteLine("\n");
        //
        //
        // /*
        //  *   Plugin debug
        //  */
        // writeConVarConfig(config, m_CVPluginDebugLevel);
        // writeConVarConfig(config, m_CVPluginDebugShowClientConsole);
        // config.WriteLine("\n");
        //
        // /*
        //  *   Omikuji
        //  */
        // writeConVarConfig(config, m_CVOmikujiEventWeightMisc);
        // writeConVarConfig(config, m_CVOmikujiEventWeightBad);
        // writeConVarConfig(config, m_CVOmikujiEventWeightLucky);
        // writeConVarConfig(config, m_CVOmikujiCommandCooldown);
        // writeConVarConfig(config, m_CVOmikujiCommandExecutionDelayMin);
        // writeConVarConfig(config, m_CVOmikujiCommandExecutionDelayMax);
        // config.WriteLine("\n");
        //
        // /*
        //  *   Omikuji - Chicken
        //  */
        // writeConVarConfig(config, m_CVOmikujiEventChickenTime);
        // writeConVarConfig(config, m_CVOmikujiEventChickenBodyScale);
        // writeConVarConfig(config, m_CVOmikujiEventChickenSelectionWeight);
        // config.WriteLine("\n");
        //
        // /*
        //  *   Omikuji - Screen shake
        //  */
        // writeConVarConfig(config, m_CVOmikujiEventScreenShakeAmplitude);
        // writeConVarConfig(config, m_CVOmikujiEventScreenShakeDuration);
        // writeConVarConfig(config, m_CVOmikujiEventScreenShakeFrequency);
        // writeConVarConfig(config, m_CVOmikujiEventScreenShakeSelectionWeight);
        // config.WriteLine("\n");
        //
        // /*
        //  *   Omikuji - Player Heal
        //  */
        // writeConVarConfig(config, m_CVOmikujiEventPlayerHeal);
        // writeConVarConfig(config, m_CVOmikujiEventPlayerHealSelectionWeight);
        // config.WriteLine("\n");
        //
        // /*
        //  *   Omikuji - Gravity
        //  */
        // writeConVarConfig(config, m_CVOmikujiEventGravityMax);
        // writeConVarConfig(config, m_CVOmikujiEventGravityMin);
        // writeConVarConfig(config, m_CVOmikujiEventGravityRestoreTime);
        // writeConVarConfig(config, m_CVOmikujiEventGravitySelectionWeight);
        // config.WriteLine("\n");
        //
        // /*
        //  *   Omikuji - Freeze
        //  */
        // writeConVarConfig(config, m_CVOmikujiEventPlayerFreeze);
        // writeConVarConfig(config, m_CVOmikujiEventPlayerFreezeSelectionWeight);
        // config.WriteLine("\n");
        //
        // /*
        //  *   Omikuji - GiveRandomItem
        //  */
        // writeConVarConfig(config, m_CVOmikujiEventGiveRandomItemAvoidCount);
        // writeConVarConfig(config, m_CVOmikujiEventGiveRandomItemSelectionWeight);
        // config.WriteLine("\n");
        //
        // /*
        //  *   Omikuji - Player Slap
        //  */
        // writeConVarConfig(config, m_CVOmikujiEventPlayerSlapPowerMin);
        // writeConVarConfig(config, m_CVOmikujiEventPlayerSlapPowerMax);
        // writeConVarConfig(config, m_CVOmikujiEventPlayerSlapSelectionWeight);
        // config.WriteLine("\n");
        //
        // /*
        //  *   Omikuji - Selection weights
        //  */
        // writeConVarConfig(config, m_CVOmikujiEventNothingSelectionWeight);
        // writeConVarConfig(config, m_CVOmikujiEventPlayerWishingSelectionWeight);
        // writeConVarConfig(config, m_CVOmikujiEventPlayerLocationSwapSelectionWeight);
        // writeConVarConfig(config, m_CVOmikujiEventPlayerRespawnSelectionWeight);
        // writeConVarConfig(config, m_CVOmikujiEventAllPlayerRespawnSelectionWeight);
        // config.WriteLine("\n");
        //
        // /*
        //  * Hide legs
        //  */
        // writeConVarConfig(config, m_CVHideLegsEnabled);
        // config.WriteLine("\n");
        //
        // /*
        //  *   For debugging purpose
        //  */
        // writeConVarConfig(config, m_CVDebuggingEnabled);
        // config.WriteLine("\n");
        //
        // /*
        //  *   Misc commands
        //  */
        // writeConVarConfig(config, m_CVMiscCMDGiveKnifeEnabled);
        // config.WriteLine("\n");
        //
        // /*
        //  *   Course Weapon
        //  */
        // writeConVarConfig(config, m_CVCourseWeaponEnabled);
        // config.WriteLine("\n");
        //
        // /*
        //  *   Rocket commands
        //  */
        // writeConVarConfig(config, m_CVRocketEnabled);
        // config.WriteLine("\n");

        config.Close();
    }

    private static void writeConVarConfig<T>(StreamWriter configFile, FakeConVar<T> convar) where T : IComparable<T>
    {
        configFile.WriteLine($"// {convar.Description}");
        if (typeof(T) == typeof(bool))
        {
            var conValue = convar.Value;
            bool value = Unsafe.As<T, bool>(ref conValue);
            configFile.WriteLine($"{convar.Name} {Convert.ToInt32(value)}");
        }
        else
        {
            configFile.WriteLine($"{convar.Name} {convar.Value}");
        }

        configFile.WriteLine();
    }
}