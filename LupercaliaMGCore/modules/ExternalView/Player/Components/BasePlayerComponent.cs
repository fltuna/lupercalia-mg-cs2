using LupercaliaMGCore.modules.ExternalView.API;

namespace LupercaliaMGCore.modules.ExternalView.Player.Components
{
    public abstract class BasePlayerComponent
    {
        protected readonly IExternalViewCsPlayer Player;
        protected bool IsEnabled { get; private set; }

        public BasePlayerComponent(IExternalViewCsPlayer player)
        {
            Player = player;
        }

        public void Enable()
        {
            if (IsEnabled)
                return;

            IsEnabled = true;
            OnEnable();
        }

        public void Disable()
        {
            if (!IsEnabled)
                return;

            IsEnabled = false;
            OnDisable();
        }

        public void Update()
        {
            if (!IsEnabled)
                return;

            OnUpdate();
        }

        public virtual void OnEnable()
        {
        }

        public virtual void OnDisable()
        {
        }

        public virtual void OnUpdate()
        {
        }
    }
}
