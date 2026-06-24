// Source - https://stackoverflow.com/a/37100583
// Posted by Sean Ed-Man
// Retrieved 2026-06-24, License - CC BY-SA 3.0
//This library maps persian characters to unicode and makes them rtl, in order to use in softwares like Unity3D
//Author: Shayan Edalatmanesh
//=============================================================================================================================
//Me: Thanks to Shayan Edalatmanesh for implementing this class — now I don't have to dive deep into Unicode for the time being

using System.Text;

namespace TelltaleLocalizationTool.Localize;

public static class PersianMap
{
    private static readonly string PersianChars = "ئءؤرلایيةوزژظشسيبپلاتنمكکگطضصثقفغعهخحچجدذْلآآلأأـلإإًٌٍَُِّْٰت";
    private static readonly string Tashkil = "ًٌٍَُِّْٰ";

    private static readonly Dictionary<char, char[]> Map = new()
    {
        {'آ', new[] {'ﺁ', 'ﺁ', 'ﺂ', 'ﺂ'}},
        {'ا', new[] {'ﺍ', 'ﺍ', 'ﺎ', 'ﺎ'}},
        {'أ', new[] {'ﺃ', 'ﺃ', 'ﺄ', 'ﺄ'}},
        {'إ', new[] {'ﺇ', 'ﺇ', 'ﺈ', 'ﺈ'}},
        {'ب', new[] {'ﺏ', 'ﺑ', 'ﺒ', 'ﺐ'}},
        {'پ', new[] {'ﭖ', 'ﭘ', 'ﭙ', 'ﭗ'}},
        {'ت', new[] {'ﺕ', 'ﺗ', 'ﺘ', 'ﺖ'}},
        {'ث', new[] {'ﺙ', 'ﺛ', 'ﺜ', 'ﺚ'}},
        {'ج', new[] {'ﺝ', 'ﺟ', 'ﺠ', 'ﺞ'}},
        {'چ', new[] {'ﭺ', 'ﭼ', 'ﭽ', 'ﭻ'}},
        {'ح', new[] {'ﺡ', 'ﺣ', 'ﺤ', 'ﺢ'}},
        {'خ', new[] {'ﺥ', 'ﺧ', 'ﺨ', 'ﺦ'}},
        {'د', new[] {'ﺩ', 'ﺩ', 'ﺪ', 'ﺪ'}},
        {'ذ', new[] {'ﺫ', 'ﺫ', 'ﺬ', 'ﺬ'}},
        {'ر', new[] {'ﺭ', 'ﺭ', 'ﺮ', 'ﺮ'}},
        {'ز', new[] {'ﺯ', 'ﺯ', 'ﺰ', 'ﺰ'}},
        {'ژ', new[] {'ﮊ', 'ﮊ', 'ﮋ', 'ﮋ'}},
        {'س', new[] {'ﺱ', 'ﺳ', 'ﺴ', 'ﺲ'}},
        {'ش', new[] {'ﺵ', 'ﺷ', 'ﺸ', 'ﺶ'}},
        {'ص', new[] {'ﺹ', 'ﺻ', 'ﺼ', 'ﺺ'}},
        {'ض', new[] {'ﺽ', 'ﺿ', 'ﻀ', 'ﺾ'}},
        {'ط', new[] {'ﻁ', 'ﻃ', 'ﻄ', 'ﻂ'}},
        {'ظ', new[] {'ﻅ', 'ﻇ', 'ﻈ', 'ﻆ'}},
        {'ع', new[] {'ﻉ', 'ﻋ', 'ﻌ', 'ﻊ'}},
        {'غ', new[] {'ﻍ', 'ﻏ', 'ﻐ', 'ﻎ'}},
        {'ف', new[] {'ﻑ', 'ﻓ', 'ﻔ', 'ﻒ'}},
        {'ق', new[] {'ﻕ', 'ﻗ', 'ﻘ', 'ﻖ'}},
        {'ک', new[] {'ﮎ', 'ﮐ', 'ﮑ', 'ﮏ'}},
        {'ك', new[] {'ﻙ', 'ﻛ', 'ﻜ', 'ﻚ'}},
        {'گ', new[] {'ﮒ', 'ﮔ', 'ﮕ', 'ﮓ'}},
        {'ل', new[] {'ﻝ', 'ﻟ', 'ﻠ', 'ﻞ'}},
        {'م', new[] {'ﻡ', 'ﻣ', 'ﻤ', 'ﻢ'}},
        {'ن', new[] {'ﻥ', 'ﻧ', 'ﻨ', 'ﻦ'}},
        {'و', new[] {'ﻭ', 'ﻭ', 'ﻮ', 'ﻮ'}},
        {'ؤ', new[] {'ﺅ', 'ﺅ', 'ﺆ', 'ﺆ'}},
        {'ه', new[] {'ﻩ', 'ﻫ', 'ﻬ', 'ﻪ'}},
        {'ة', new[] {'ﺓ', 'ﺓ', 'ﺔ', 'ﺔ'}},
        {'ی', new[] {'ﯼ', 'ﯾ', 'ﯿ', 'ﯽ'}},
        {'ي', new[] {'ﻱ', 'ﻳ', 'ﻴ', 'ﻲ'}},
        {'ئ', new[] {'ﺉ', 'ﺋ', 'ﺌ', 'ﺊ'}},
        {'ـ', new[] {'ـ', 'ـ', 'ـ', 'ـ'}},
    };

    private static readonly Dictionary<string, char[]> DoubleMap = new()
    {
        {"لآ", new[] {'ﻵ', 'ﻵ', 'ﻶ', 'ﻶ'}},
        {"لا", new[] {'ﻻ', 'ﻻ', 'ﻼ', 'ﻼ'}},
        {"لأ", new[] {'ﻷ', 'ﻷ', 'ﻸ', 'ﻸ'}},
        {"لإ", new[] {'ﻹ', 'ﻹ', 'ﻺ', 'ﻺ'}},
    };

    // Letters after which the next letter can stick.
    private static readonly HashSet<char> AfterStick =
    [
        'ب','پ','ت','ث','ج','چ','ح','خ','س','ش','ص','ض','ط','ظ','ع','غ','ف','ق','ک','ك','گ',
        'ل','م','ن','ه','ی','ي','ئ','ـ'
    ];

    private static bool IsPersianChar(char c) => PersianChars.IndexOf(c) >= 0;
    private static bool IsTashkil(char c) => Tashkil.IndexOf(c) >= 0;
    private static bool ContainsPersian(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            if (IsPersianChar(s[i]))
                return true;
        }
        return false;
    }

    // 0: Isolated
    // 1: Begin
    // 2: Middle
    // 3: End
    private static int GetGlyphType(char chr, char? before, char? after)
    {
        bool canJoinPrev = before.HasValue && AfterStick.Contains(before.Value);
        bool canJoinNext = AfterStick.Contains(chr) && after.HasValue && Map.ContainsKey(after.Value);

        if (canJoinPrev && canJoinNext) return 2;
        if (canJoinNext) return 1;
        if (canJoinPrev) return 3;
        return 0;
    }

    private static string ConvertWord(string word)
    {
        if (string.IsNullOrEmpty(word))
            return word;

        var result = new StringBuilder(word.Length);

        for (int i = 0; i < word.Length; i++)
        {
            char current = word[i];

            if (!Map.TryGetValue(current, out var forms))
            {
                result.Append(current);
                continue;
            }

            char? before = null, after = null;

            for (int k = i - 1; k >= 0; k--)
            {
                if (!IsTashkil(word[k]))
                {
                    before = word[k];
                    break;
                }
            }

            for (int k = i + 1; k < word.Length; k++)
            {
                if (!IsTashkil(word[k]))
                {
                    after = word[k];
                    break;
                }
            }

            int glyph = GetGlyphType(current, before, after);

            if (after.HasValue)
            {
                string pair = new([current, after.Value]);
                if (DoubleMap.TryGetValue(pair, out var ligatureForms))
                {
                    result.Append(ligatureForms[glyph]);
                    i++; // Skip the next letter.
                    continue;
                }
            }

            result.Append(forms[glyph]);
        }

        char[] chars = result.ToString().ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    private static string ConvertLine(string line)
    {
        if (string.IsNullOrEmpty(line) || !ContainsPersian(line))
            return line;

        var parts = new List<(bool Persian, string Text)>();

        int i = 0;
        while (i < line.Length)
        {
            bool persian = IsPersianChar(line[i]);
            int start = i;

            i++;
            while (i < line.Length && IsPersianChar(line[i]) == persian)
                i++;

            parts.Add((persian, line[start..i]));
        }

        var res = new StringBuilder(line.Length);

        for (int p = parts.Count - 1; p >= 0; p--)
        {
            if (parts[p].Persian)
                res.Append(ConvertWord(parts[p].Text));
            else
                res.Append(parts[p].Text);
        }

        return res.ToString();
    }

    public static string ConvertPersian(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        str = str.Replace("\r\n", "\n");
        string[] lines = str.Split('\n');

        for (int i = 0; i < lines.Length; i++)
            lines[i] = ConvertLine(lines[i]);

        return string.Join("\n", lines);
    }

    public static string FixPersian(this string str)
    {
        return ConvertPersian(str);
    }

    private static string GetIsolatedChar(char ch)
    {
        foreach (var pair in Map)
        {
            if (Array.IndexOf(pair.Value, ch) >= 0)
                return pair.Key.ToString();
        }

        foreach (var pair in DoubleMap)
        {
            if (Array.IndexOf(pair.Value, ch) >= 0)
                return pair.Key;
        }

        return ch.ToString();
    }

    private static bool IsPresentationForm(char ch)
    {
        foreach (var pair in Map)
        {
            if (Array.IndexOf(pair.Value, ch) >= 0)
                return true;
        }

        foreach (var pair in DoubleMap)
        {
            if (Array.IndexOf(pair.Value, ch) >= 0)
                return true;
        }

        return false;
    }

    private static readonly HashSet<char> OpeningWrappers = new()
{
    '{', '[', '(', '<', '«'
};

    private static readonly HashSet<char> ClosingWrappers = new()
{
    '}', ']', ')', '>', '»'
};

    private static bool IsOpeningWrapper(char ch) => OpeningWrappers.Contains(ch);
    private static bool IsClosingWrapper(char ch) => ClosingWrappers.Contains(ch);

    private static string NormalizePersianLine(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        int start = 0;
        int end = str.Length;

        var prefix = new StringBuilder();
        var suffix = new StringBuilder();

        while (start < end && IsOpeningWrapper(str[start]))
        {
            prefix.Append(str[start]);
            start++;
        }

        while (end > start && IsClosingWrapper(str[end - 1]))
        {
            suffix.Insert(0, str[end - 1]);
            end--;
        }

        string core = str[start..end];
        if (core.Length == 0)
            return prefix + suffix.ToString();

        var segments = new List<(bool Persian, string Text)>();

        int i = 0;
        while (i < core.Length)
        {
            bool isPersian = IsPresentationForm(core[i]);
            int segmentStart = i;

            while (i < core.Length && IsPresentationForm(core[i]) == isPersian)
                i++;

            segments.Add((isPersian, core[segmentStart..i]));
        }

        var result = new StringBuilder(core.Length);

        for (int s = segments.Count - 1; s >= 0; s--)
        {
            var segment = segments[s];

            if (!segment.Persian)
            {
                result.Append(segment.Text);
                continue;
            }

            char[] chars = segment.Text.ToCharArray();
            Array.Reverse(chars);

            foreach (char ch in chars)
                result.Append(GetIsolatedChar(ch));
        }

        return prefix + result.ToString() + suffix;
    }

    public static string NormalizePersian(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        str = str.Replace("\r\n", "\n");
        string[] lines = str.Split('\n');

        for (int i = 0; i < lines.Length; i++)
            lines[i] = NormalizePersianLine(lines[i]);

        return string.Join("\n", lines);
    }
}