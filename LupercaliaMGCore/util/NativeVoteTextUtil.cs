namespace LupercaliaMGCore;

public static class NativeVoteTextUtil
{
    public const string VoteDisplayString = "#SFUI_vote_passed_nextlevel";

    public static string GenerateReadableNativeVoteText(string text)
    {
        return $"■■■■■■■■■■■■■■■■■■■■■■■■■■■■ {text} ■■■■■■■■■■■■■■■■■■■■■■■■■■■■";
    }
}