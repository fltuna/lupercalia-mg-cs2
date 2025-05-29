using CounterStrikeSharp.API.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LupercaliaMGCore.modules.ExternalView.CSSharp
{
    internal interface ILocalizer
    {
        string LocalizeForPlayer(CCSPlayerController controller, string message, params object[] args);
    }
}
