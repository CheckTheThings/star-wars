using CheckTheThings.StarWars.Wookieepedia;

namespace CheckTheThings.StarWars
{
    public class ListGenerator
    {
        private readonly List<Todo> _todos;
        private int _maxId;

        public ListGenerator(List<Todo> todos)
        {
            _todos = todos;
            _maxId = JsonFile.GetNumberOfFiles(Program.OutputDirectory) - 1;
        }

        public async Task CreateList(string title, IEnumerable<Media> mediaList)
        {
            var fileName = $@"{Program.ChecklistDirectory}/{title}.json";
            var checklist = await JsonFile.ReadChecklistAsync(fileName);
            var checklistItems = checklist?.Items ?? Array.Empty<ChecklistItem>();

            var desiredChecklistItems = from m in mediaList
                                        join t in _todos on m.Link equals t.Link
                                        select new ChecklistItem(t.Id, t.Title);

            var newChecklistItems = desiredChecklistItems.Except(checklistItems);
            checklistItems = checklistItems.Concat(newChecklistItems).ToArray();

            checklist = checklist == null
                ? new Checklist(GetNextId(), title, checklistItems.ToArray())
                : checklist with {  Items = checklistItems };
            await JsonFile.WriteChecklistAsync(fileName, checklist);
        }

        private int GetNextId() => ++_maxId;
    }
}
