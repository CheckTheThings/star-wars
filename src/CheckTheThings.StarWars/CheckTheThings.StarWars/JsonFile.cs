using System.Text.Json;

namespace CheckTheThings.StarWars
{
    public static class JsonFile
    {
        public const string OutputDirectory = @"..\..\..\..\..\..\data";
        public static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        public static async Task<List<Todo>> ReadTodosAsync(string fileName)
        {
            if (!File.Exists(fileName))
                return new(0);

            using var inputStream = File.OpenRead(fileName);
            return await JsonSerializer.DeserializeAsync<List<Todo>>(inputStream, JsonSerializerOptions) ?? new(0);
        }

        public static async Task<Checklist?> ReadChecklistAsync(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            using var stream = File.OpenRead(fileName);
            return await JsonSerializer.DeserializeAsync<Checklist>(stream, JsonSerializerOptions);
        }

        public static async Task WriteTodosAsync(string fileName, List<Todo> todos)
        {
            using var stream = File.OpenWrite(fileName);
            await JsonSerializer.SerializeAsync(stream, todos, JsonSerializerOptions);
        }

        public static async Task WriteChecklistAsync(string fileName, Checklist checklist)
        {
            using var stream = File.OpenWrite(fileName);
            await JsonSerializer.SerializeAsync(stream, checklist, JsonSerializerOptions);
        }

        public static int GetNumberOfFiles() => Directory.GetFiles(OutputDirectory).Length;
    }
}
