namespace LupercaliaMGCore.model;

public sealed class IgnoredLogger: AbstractDebugLogger
{
    public override int DebugLogLevel => 0;
    public override bool PrintToAdminClientsConsole => false;
    public override string RequiredFlagForPrintToConsole => "";
    public override string LogPrefix => "";
}