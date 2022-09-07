using System.Text.Json;
using CheckTheThings.StarWars.Wookieepedia;

const string TodosFileName = @"..\..\..\..\..\..\data\todos.json";
var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

var todos = await ReadTodosFromJsonAsync(TodosFileName);
int maxId = todos.Any() ? todos.Max(x => x.Id) : 0;

var updatedMedia = await GetMedia();
var newTodos = (from m in updatedMedia
                join _ in todos on m.Link equals _.Link into gj
                from t in gj.DefaultIfEmpty()
                where t == null
                select new Todo(GetNextTodoId(), m.Title, m.Link)).ToList();

if (!newTodos.Any())
    return;

todos = todos.Union(newTodos).ToList();
await SaveTodosJsonAsync(TodosFileName, todos, jsonSerializerOptions);

int GetNextTodoId()
{
    return ++maxId;
}

static async Task<List<Todo>> ReadTodosFromJsonAsync(string fileName)
{
    if (!File.Exists(fileName))
        return new(0);

    using var inputStream = File.OpenRead(fileName);
    return await JsonSerializer.DeserializeAsync<List<Todo>>(inputStream) ?? new(0);
}

static async Task SaveTodosJsonAsync(string fileName, List<Todo> newTodos, JsonSerializerOptions jsonSerializerOptions)
{
    using var stream = File.OpenWrite(fileName);
    await JsonSerializer.SerializeAsync(stream, newTodos, jsonSerializerOptions);
}

static async Task<IEnumerable<Media>> GetMedia()
{
    var canonTask = Task.FromResult(new List<Media>(0));// TimelineParser.ParseCanonTimelineAsync();
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

record Todo(int Id, string Title, string Link);
record Checklist(int Id, string Title, ChecklistItem[] Items);
record ChecklistItem(int Id, string Title);
