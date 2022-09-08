using System.Text.Json;
using CheckTheThings.StarWars.Wookieepedia;

namespace CheckTheThings.StarWars
{
    internal class Program
    {
        const string TodosFileName = @$"{JsonFile.OutputDirectory}\todos.json";

        private static async Task Main(string[] args)
        {

            var todos = await ReadTodosFromJsonAsync(TodosFileName);
            int maxId = todos.Any() ? todos.Max(x => x.Id) : 0;

            var updatedMedia = await GetMedia();
            var newTodos = (from m in updatedMedia
                            join _ in todos on m.Link equals _.Link into gj
                            from t in gj.DefaultIfEmpty()
                            where t == null
                            select new Todo(GetNextTodoId(), m.Title, m.Link)).ToList();

            todos = todos.Union(newTodos).ToList();
            await SaveTodosJsonAsync(TodosFileName, todos);

            var listGenerator = new ListGenerator(todos);
            await listGenerator.CreateList("Skywalker Saga", updatedMedia.Where(x => x.IsMovie() && x.Title.StartsWith("Star Wars: Episode")));

            int GetNextTodoId() => ++maxId;
        }

        static async Task<List<Todo>> ReadTodosFromJsonAsync(string fileName)
        {
            if (!File.Exists(fileName))
                return new(0);

            using var inputStream = File.OpenRead(fileName);
            return await JsonSerializer.DeserializeAsync<List<Todo>>(inputStream) ?? new(0);
        }

        static async Task SaveTodosJsonAsync(string fileName, List<Todo> todos)
        {
            using var stream = File.OpenWrite(fileName);
            await JsonSerializer.SerializeAsync(stream, todos, JsonFile.JsonSerializerOptions);
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
                .OrderBy(x => x.ReleaseDate)
                .ThenBy(x => x.Name);
            return updatedMedia;
        }
    }
}
