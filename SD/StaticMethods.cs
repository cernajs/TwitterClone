namespace TwitterClone.SD;

public static class StaticMethods
{
    // public static string GetUserId(this ClaimsPrincipal user)
    // {
    //     return user.FindFirstValue(ClaimTypes.NameIdentifier);
    // }

    public static string ConvertToHtmlWithClickableHashtags(string tweetText)
    {
        var words = tweetText.Split(' ');
        for (int i = 0; i < words.Length; ++i)
        {
            if (words[i].StartsWith("#"))
            {
                var hashtag = words[i].Substring(1);
                words[i] = $"<a href=\"/Home/Search?searchQuery=%23{hashtag}\">{words[i]}</a>";
            }
        }
        return string.Join(' ', words);
    }

}