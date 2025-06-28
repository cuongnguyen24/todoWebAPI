namespace todo_app.Models
{
    public class CreateListModel
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
