namespace CheckTheThings.StarWars
{
    public record Todo(int Id, string Title, string Link);
    public record Checklist(int Id, string Title, ChecklistItem[] Items);
    public record ChecklistItem(int Id, string Title);
}
