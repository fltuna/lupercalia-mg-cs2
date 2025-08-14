using CounterStrikeSharp.API;
using LupercaliaMGCore.modules.ExternalView.API;
using LupercaliaMGCore.modules.ExternalView.Utils;
using System;

namespace LupercaliaMGCore.modules.ExternalView.Cameras
{
    public class WatchCamera : BaseChaseCamera
    {
        public IExternalViewCsPlayer? CurrentTarget;

        public WatchCamera(ICameraContext ctx, string? initialTarget) : base(ctx)
        {
            var candidates = Candidates;
            ObserverTarget? obsTarget = null;

            if (initialTarget != null)
            {
                obsTarget = ObserverTargetUtils.Find(candidates, initialTarget);
                if (obsTarget == null)
                {
                    Ctx.Player.PrintToChat("ExternalView.Watch.FailedToFindInitialTarget");
                }
            }
            if (obsTarget == null)
            {
                obsTarget = ObserverTargetUtils.SelectRandom(candidates);
            }

            if (obsTarget != null)
            {
                SetTargetByObserverTarget(obsTarget);
            }
            else
            {
                Ctx.Player.PrintToChat("ExternalView.Watch.EndedByNoCandidates");
            }
        }

        private IEnumerable<IExternalViewCsPlayer> ObservablePlayers => Ctx.Api.AllPlayers.Where(IsObservable);

        private IEnumerable<ObserverTarget> Candidates => ObservablePlayers.Select(player => new ObserverTarget(player.Slot, player.Name));

        protected override IExternalViewCsPlayer? Target => CurrentTarget;

        protected override float Distance => Consts.DefaultCameraDistance;

        protected override float YawOffset => 0;

        public override bool Update()
        {
            var target = CurrentTarget;

            if (target == null)
                return false;

            if (!IsObservable(target))
            {
                var obsTarget = ObserverTargetUtils.SelectRandom(Candidates);
                if (obsTarget == null)
                {
                    Ctx.Player.PrintToChat("ExternalView.Watch.EndedByNoCandidates");
                    return false;
                }

                SetTargetByObserverTarget(obsTarget);
            }

            if (Ctx.Player.Buttons.IsPressed(PlayerButtons.Attack))
            {
                var obsTarget = ObserverTargetUtils.Next(Candidates, target.Slot);
                if (obsTarget != null)
                {
                    SetTargetByObserverTarget(obsTarget);
                }
            }
            if (Ctx.Player.Buttons.IsPressed(PlayerButtons.Attack2))
            {
                var obsTarget = ObserverTargetUtils.Prev(Candidates, target.Slot);
                if (obsTarget != null)
                {
                    SetTargetByObserverTarget(obsTarget);
                }
            }

            return base.Update();
        }

        public bool FindTarget(string target)
        {
            var obsTarget = ObserverTargetUtils.Find(Candidates, target);
            if (obsTarget == null)
            {
                Ctx.Player.PrintToChat("ExternalView.Watch.FailedToFindTarget");
                return false;
            }

            SetTargetByObserverTarget(obsTarget);

            return true;
        }

        private bool IsObservable(IExternalViewCsPlayer player)
        {
            if (!player.IsValid)
                return false;

            if (player.IsSpectator)
                return false;

            if (player.TimeElapsedFromLastDeath > Consts.WatchCameraPostDeathWaitTime)
                return false;

            if (player.Equals(Ctx.Player))
                return false;

            return true;
        }

        private void SetTargetByObserverTarget(ObserverTarget obsTarget)
        {
            CurrentTarget = Ctx.Api.GetPlayerBySlot(obsTarget.Slot);
        }
    }
}
