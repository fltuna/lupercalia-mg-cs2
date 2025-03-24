using System.Reflection;
using System.Text.RegularExpressions;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;

namespace LupercaliaMGCore;

public static class ChatUtil
{
    public static string ReplaceColorStrings(string text)
    {
        var colorFields = typeof(ChatColors)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(char));

        foreach (var field in colorFields)
        {
            string colorName = field.Name;
            text = Regex.Replace(text, "{" + Regex.Escape(colorName) + "}", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, "{/" + Regex.Escape(colorName) + "}", "", RegexOptions.IgnoreCase);
        }

        return text;
    }
}