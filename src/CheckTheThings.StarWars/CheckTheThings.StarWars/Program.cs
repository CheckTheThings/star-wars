using CheckTheThings.StarWars.Wookieepedia;

namespace CheckTheThings.StarWars
{
    internal class Program
    {
        public const string OutputDirectory = @"..\..\..\..\..\..\data";
        public const string ChecklistDirectory = $@"{OutputDirectory}\lists";

        private static async Task Main(string[] args)
        {
            var updatedMedia = (await GetMedia()).ToList();
            var todos = await AddTodosAsync(updatedMedia);

            var listGenerator = new ListGenerator(todos);
            //await listGenerator.CreateList("Skywalker Saga", updatedMedia.Where(x => x.IsMovie() && x.Title.StartsWith("Star Wars: Episode")));

            await AddCanonLists(updatedMedia, listGenerator);
            await AddLegendsLists(updatedMedia, listGenerator);
        }

        private static async Task<List<Todo>> AddTodosAsync(List<Media> updatedMedia)
        {
            var filePath = Path.Combine(OutputDirectory, "todos.json");

            var todos = await JsonFile.ReadTodosAsync(filePath);
            int maxId = todos.Any() ? todos.Max(x => x.Id) : 0;

            var newTodos = (from m in updatedMedia
                            join _ in todos on m.Link equals _.Link into gj
                            from t in gj.DefaultIfEmpty()
                            where t == null
                            select new Todo(GetNextTodoId(), m.Title, m.Link)).ToList();

            todos = todos.Union(newTodos).ToList();
            await JsonFile.WriteTodosAsync(filePath, todos);

            return todos;

            int GetNextTodoId() => ++maxId;
        }

        private static async Task AddCanonLists(List<Media> updatedMedia, ListGenerator listGenerator)
        {
            var media = updatedMedia.Where(x => x.IsCanon()).ToList();
            await listGenerator.CreateList(GetTitle("Comics"), media.Where(x => x.IsComic()));
            await listGenerator.CreateList(GetTitle("Movies"), media.Where(x => x.IsMovie()));
            await listGenerator.CreateList(GetTitle("Novels"), media.Where(x => x.IsNovel()));
            await listGenerator.CreateList(GetTitle("Short Stories"), media.Where(x => x.IsShortStory()));

            static string GetTitle(string baseTitle) => $"{baseTitle} (Canon)";
        }

        private static async Task AddLegendsLists(List<Media> updatedMedia, ListGenerator listGenerator)
        {
            var media = updatedMedia.Where(x => x.IsLegends()).ToList();
            await listGenerator.CreateList(GetTitle("Comics"), media.Where(x => x.IsComic()));
            await listGenerator.CreateList(GetTitle("Movies"), media.Where(x => x.IsMovie()));
            await listGenerator.CreateList(GetTitle("Novels"), media.Where(x => x.IsNovel()));
            await listGenerator.CreateList(GetTitle("Short Stories"), media.Where(x => x.IsShortStory()));

            static string GetTitle(string baseTitle) => $"{baseTitle} (Legends)";
        }

        static async Task<IEnumerable<Media>> GetMedia()
        {
            var canonTask = TimelineParser.ParseCanonTimelineAsync();
            var legendsTask = TimelineParser.ParseLegendsTimelineAsync();

            await Task.WhenAll(canonTask, legendsTask);

            var updatedMedia = canonTask.Result.Union(legendsTask.Result);
            updatedMedia = updatedMedia.GroupBy(
                x => x.Link,
                (_, l) =>
                {
                    var result = l.First();
                    result.Tags = l.SelectMany(x => x.Tags).ToList();
                    return result;
                });
            updatedMedia = updatedMedia
                .Where(x => x.ReleaseDate != null)
                .Where(x => x.IsPublished())
                .OrderBy(x => x.ReleaseDate)
                .ThenBy(x => x.Name);
            return updatedMedia;
        }
    }
}
