using CheckTheThings.StarWars.Wookieepedia;

namespace CheckTheThings.StarWars
{
    internal static class MediaExtensions
    {
        public static bool IsCanon(this Media media) => media.Tags.Contains("Canon");
        public static bool IsLegends(this Media media) => media.Tags.Contains("Legends");
        public static bool IsMovie(this Media media) => media.Classes.Contains("film");
        public static bool IsNovel(this Media media) => media.Classes.Contains("novel");
    }
}
