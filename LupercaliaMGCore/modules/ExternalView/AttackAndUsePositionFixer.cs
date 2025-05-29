using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using Microsoft.Extensions.Logging;
using TNCSSPluginFoundation.Models.Plugin;
using static CounterStrikeSharp.API.Core.BasePlugin;

namespace LupercaliaMGCore.modules.ExternalView
{
    /// <summary>
    /// Fixes the issue with attack and use position while using custom view entity.
    /// 
    /// Requires ExternalViewHelper metamod plugin to be installed.
    /// See: https://github.com/spitice/cs2-external-view
    /// 
    /// This fixer enforces all players will be treated as if they are not using
    /// custom view entity during server's ENTITY_THINK.
    /// So, all attack and use actions will be performed from the correct location
    /// (i.e., the front of the player's pawn)
    /// 
    /// Our small metamod plugin makes it easy to hook EntityThink games system events
    /// from CSSharp plugin.
    /// </summary>
    internal class AttackAndUsePositionFixer : IDisposable
    {
        private static readonly string InterfaceName = "ExternalViewHelper v1.0";
        private static readonly int OnPreEntityThinkVtableOffset = 0;
        private static readonly int OnPostEntityThinkVtableOffset = 1;

        public delegate void AddTimerFn(float Interval, Action callback);
        public delegate IEnumerable<CCSPlayerController?> GetPlayersFn();

        private delegate void EntityThinkCallback();
        private delegate void UnhookCallback();

        private ILogger Logger;
        private AddTimerFn _AddTimer;
        private GetPlayersFn _GetPlayers;

        private nint? ExternalViewHelperPtr;

        private static readonly int MaxNumRetries = 10;
        private static readonly float RetryInterval = 3.0f;
        private int NumRetries = 0;

        private bool IsLoaded => ExternalViewHelperPtr.HasValue;

        private UnhookCallback? _UnhookPreThink;
        private UnhookCallback? _UnhookPostThink;

        public AttackAndUsePositionFixer(ILogger logger, AddTimerFn addTimer, GetPlayersFn getPlayers)
        {
            Logger = logger;
            _AddTimer = addTimer;
            _GetPlayers = getPlayers;

            TryToLoadHelperPlugin();
        }

        void IDisposable.Dispose()
        {
            Unload();
        }

        public void Unload()
        {
            if (_UnhookPreThink != null)
            {
                _UnhookPreThink();
                _UnhookPreThink = null;
            }
            if (_UnhookPostThink != null)
            {
                _UnhookPostThink();
                _UnhookPostThink = null;
            }
        }

        private void TryToLoadHelperPlugin()
        {
            if (IsLoaded)
                return;

            ExternalViewHelperPtr = Utilities.MetaFactory(InterfaceName);

            if (!IsLoaded)
            {
                if (NumRetries >= MaxNumRetries)
                {
                    Logger.LogError("[ExternalView] Failed to load ExternalViewHelper. AttackAndUsePositionFixer is disabled.");
                    Logger.LogError("[ExternalView] Please install ExternalViewHelper from https://github.com/spitice/cs2-external-view");
                    return;
                }

                //
                // At the time OnMetamodAllPluginsLoaded gets called, we still cannot retrieve
                // ExternalViewHelper via MetaFactory.
                //
                // So let's just retry MetaFactory few times.
                //
                NumRetries++;

                _AddTimer(RetryInterval, TryToLoadHelperPlugin);

                return;
            }

            Logger.LogInformation("[ExternalView] ExternalViewHelper has been found.");

            SetupHooks();
        }

        private void SetupHooks()
        {
            if (!ExternalViewHelperPtr.HasValue)
                return;

            Unload();

            _UnhookPreThink = HookEntityThink(ExternalViewHelperPtr.Value, OnPreEntityThinkVtableOffset, false, OnPreEntityThink);
            _UnhookPostThink = HookEntityThink(ExternalViewHelperPtr.Value, OnPostEntityThinkVtableOffset, true, OnPostEntityThink);
        }

        private UnhookCallback HookEntityThink(nint pVtable, int vtableOffset, bool isPost, EntityThinkCallback handler)
        {
            var retType = (int)DataType.DATA_TYPE_VOID;
            var arguments = new object[] { DataType.DATA_TYPE_POINTER };
            var numArgs = 1;
            var funcPtr = NativeAPI.CreateVirtualFunction(pVtable, vtableOffset, numArgs, retType, arguments);
            var funcRef = FunctionReference.Create(handler);

            NativeAPI.HookFunction(funcPtr, funcRef, isPost);
            return () => NativeAPI.UnhookFunction(funcPtr, funcRef, isPost);
        }

        private record FixedPlayer(
            CCSPlayerController Controller,
            uint LastViewEntityHandleRaw
        );

        private List<FixedPlayer> _FixedPlayers = new();

        private void OnPreEntityThink()
        {
            foreach (CCSPlayerController? controller in _GetPlayers())
            {
                if (controller == null)
                    continue;

                var viewEntityHandleRaw = controller.PlayerPawn.Value?.CameraServices?.ViewEntity.Raw;
                if (!viewEntityHandleRaw.HasValue)
                    continue;

                if (viewEntityHandleRaw == uint.MaxValue)
                    continue;

                _FixedPlayers.Add(new FixedPlayer(controller, viewEntityHandleRaw.Value));

                controller.PlayerPawn.Value!.CameraServices!.ViewEntity.Raw = uint.MaxValue;
            }
        }

        private void OnPostEntityThink()
        {
            foreach (var player in _FixedPlayers)
            {
                if (!player.Controller.IsValid)
                    continue;

                player.Controller.PlayerPawn.Value!.CameraServices!.ViewEntity.Raw = player.LastViewEntityHandleRaw;
            }
            _FixedPlayers.Clear();
        }
    }
}
