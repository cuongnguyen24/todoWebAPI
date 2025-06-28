namespace todo_app.Models.Dtos.Todo
{
    public class CreateTodoModel
    {
        public int ListId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Priority { get; set; }
    }
}
