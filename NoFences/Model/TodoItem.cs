using System;
using System.Xml.Serialization;

namespace NoFences.Model
{
    [Serializable]
    public class TodoItem
    {
        [XmlElement]
        public string Id { get; set; }

        [XmlElement]
        public string Title { get; set; }

        [XmlElement]
        public string Description { get; set; }

        [XmlElement]
        public DateTime DueDate { get; set; }

        [XmlElement]
        public bool IsCompleted { get; set; }

        [XmlElement]
        public int Priority { get; set; }

        public TodoItem()
        {
            Id = Guid.NewGuid().ToString();
            DueDate = DateTime.Today;
            Priority = 1;
        }

        public TodoItem(string title, string description, DateTime dueDate, int priority = 1)
        {
            Id = Guid.NewGuid().ToString();
            Title = title;
            Description = description;
            DueDate = dueDate;
            Priority = priority;
            IsCompleted = false;
        }
    }

    [Serializable]
    public enum TodoFilter
    {
        All,
        Active,
        Completed,
        ByPriority,
        ByDate
    }

    [Serializable]
    public class TodoList
    {
        [XmlElement("Todo")]
        public System.Collections.Generic.List<TodoItem> Items { get; set; } = new System.Collections.Generic.List<TodoItem>();
    }
}
