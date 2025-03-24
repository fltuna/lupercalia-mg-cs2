using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace LupercaliaMGCore;

public enum OmikujiType
{
    EVENT_BAD,
    EVENT_LUCKY,
    EVENT_MISC,
}

public enum OmikujiCanInvokeWhen
{
    PLAYER_DIED,
    PLAYER_ALIVE,
    ANYTIME,
}