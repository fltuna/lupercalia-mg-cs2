using CounterStrikeSharp.API.Core;

namespace LupercaliaMGCore.modules.omikuji.events;

public interface IOmikujiEvent
{
    string EventName { get; }

    OmikujiType OmikujiType { get; }

    OmikujiCanInvokeWhen OmikujiCanInvokeWhen { get; }

    void Execute(CCSPlayerController client);

    void Initialize();

    double GetOmikujiWeight();
}