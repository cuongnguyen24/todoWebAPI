namespace todo_app.Models.Entities
{
    public class Todo
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ListId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public User User { get; set; }
        public List List { get; set; }
        public ICollection<TodoTag> TodoTags { get; set; }
    }
}
