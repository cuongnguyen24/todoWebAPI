namespace todo_app.Models.Dtos.Todo
{
    public class AssignTagsModel
    {
        public int TodoId { get; set; }
        public List<int> TagIds { get; set; }
    }
}
