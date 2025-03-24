using CounterStrikeSharp.API.Core;

namespace LupercaliaMGCore;

public interface IOmikujiEvent
{
    string EventName { get; }

    OmikujiType OmikujiType { get; }

    OmikujiCanInvokeWhen OmikujiCanInvokeWhen { get; }

    void execute(CCSPlayerController client);

    void initialize();

    double getOmikujiWeight();
}