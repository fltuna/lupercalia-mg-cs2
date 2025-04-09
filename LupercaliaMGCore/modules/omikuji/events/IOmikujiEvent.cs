using CounterStrikeSharp.API.Core;

namespace LupercaliaMGCore;

public interface IOmikujiEvent
{
    string EventName { get; }

    OmikujiType OmikujiType { get; }

    OmikujiCanInvokeWhen OmikujiCanInvokeWhen { get; }

    void Execute(CCSPlayerController client);

    void Initialize();

    double GetOmikujiWeight();
}