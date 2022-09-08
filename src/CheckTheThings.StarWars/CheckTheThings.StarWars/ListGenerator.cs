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
            _maxId = JsonFile.GetNumberOfFiles() - 1;
        }

        public async Task CreateList(string title, IEnumerable<Media> mediaList)
        {
            var fileName = $@"{JsonFile.OutputDirectory}/{title}.json";
            var checklist = await JsonFile.ReadChecklistAsync(fileName);

            var checklistItems = from m in mediaList
                                 join t in _todos on m.Link equals t.Link
                                 select new ChecklistItem(t.Id, t.Title);

            if (checklist == null)
            {
                checklist = new Checklist(GetNextId(), title, checklistItems.ToArray());
                await JsonFile.WriteChecklistAsync(fileName, checklist);
            }
        }

        private int GetNextId() => ++_maxId;
    }
}
