
namespace WebApi.Models
{
    public class AnimeSearchResponse
    {
        public AnimeData[] data { get; set; }
    }

    public class AnimeData
    {
        public int mal_id { get; set; }
        public string title { get; set; }
        public float score { get; set; }
    }


    public class AnimeFullResponse
    {
        public AnimeFullData data { get; set; }
    }

    public class AnimeFullData
    {
        public string title_japanese { get; set; }
        public string status { get; set; }
        public string synopsis { get; set; }
        public Studio[] studios { get; set; }
    }

    public class Studio
    {
        public string name { get; set; }
    }

    public class AnimeCharactersResponse
    {
        public CharacterRole[] data { get; set; }
    }

    public class CharacterRole
    {
        public string role { get; set; }
        public Character character { get; set; }
    }

    public class Character
    {
        public string name { get; set; }
    }

    public class MangaSearchResponse
    {
        public MangaData[] data { get; set; }
    }

    public class MangaData
    {
        public int mal_id { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public float? score { get; set; }
        public int? chapters { get; set; }
    }

    public class AnimeThemesResponse
    {
        public ThemeInfo data { get; set; }
    }

    public class ThemeInfo
    {
        public string[] openings { get; set; }
        public string[] endings { get; set; }
    }
}
