namespace todo_app.Models.Entities
{
    public class List
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public User User { get; set; }
        public ICollection<Todo> Todos { get; set; }
    }
}
