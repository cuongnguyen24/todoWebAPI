﻿namespace todo_app.Models.Entities
{
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<TodoTag> TodoTags { get; set; }
    }
}
