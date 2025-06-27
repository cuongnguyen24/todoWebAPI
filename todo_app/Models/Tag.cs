namespace todo_app.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<TodoTag> TodoTags { get; set; }
    }
}
