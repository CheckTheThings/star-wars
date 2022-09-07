record Todo(int Id, string Title, string Link);
record Checklist(int Id, string Title, ChecklistItem[] Items);
record ChecklistItem(int Id, string Title);
