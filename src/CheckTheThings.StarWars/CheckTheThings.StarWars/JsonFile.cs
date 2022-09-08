using System.Text.Json;

namespace CheckTheThings.StarWars
{
    public static class JsonFile
    {
        public const string OutputDirectory = @"..\..\..\..\..\..\data";
        private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

        public static async Task<List<T>> ReadAsync<T>(string fileName)
        {
            if (!File.Exists(fileName))
                return new(0);

            using var inputStream = File.OpenRead(fileName);
            return await JsonSerializer.DeserializeAsync<List<T>>(inputStream) ?? new(0);
        }

        public static async Task<Checklist?> ReadChecklistAsync(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            using var stream = File.OpenRead(fileName);
            return await JsonSerializer.DeserializeAsync<Checklist>(stream);
        }

        public static async Task WriteAsync<T>(string fileName, object value)
        {
            using var stream = File.OpenWrite(fileName);
            await JsonSerializer.SerializeAsync(stream, value, JsonSerializerOptions);
        }

        public static async Task WriteChecklistAsync(string fileName, Checklist checklist)
        {
            using var stream = File.OpenWrite(fileName);
            await JsonSerializer.SerializeAsync(stream, checklist, JsonSerializerOptions);
        }

        public static int GetNumberOfFiles() => Directory.GetFiles(OutputDirectory).Length;
    }
}
