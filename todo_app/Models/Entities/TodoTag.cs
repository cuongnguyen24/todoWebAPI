namespace todo_app.Models.Entities
{
    public class TodoTag
    {
        public int TodoId { get; set; }
        public int TagId { get; set; }
        public Todo Todo { get; set; }
        public Tag Tag { get; set; }
    }
}
