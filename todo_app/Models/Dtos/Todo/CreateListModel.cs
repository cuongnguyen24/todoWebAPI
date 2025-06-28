namespace todo_app.Models.Dtos.Todo
{
    public class CreateListModel
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
