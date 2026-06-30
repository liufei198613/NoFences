using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Xml.Serialization;
using NoFences.Model;

namespace NoFences.Components
{
    public class TodoComponent : IComponent
    {
        public string Id { get; private set; }
        public ComponentType Type => ComponentType.TodoList;
        public Rectangle Bounds { get; set; }
        public bool Visible { get; set; } = true;

        private List<TodoItem> todos = new List<TodoItem>();
        private TodoFilter filter = TodoFilter.All;
        private string dataFilePath;

        private Font titleFont;
        private Font todoFont;
        private Size preferredSize = new Size(280, 350);

        private Rectangle addButtonRect;
        private List<Rectangle> todoCheckBoxRects = new List<Rectangle>();
        private List<Rectangle> todoTextRects = new List<Rectangle>();
        private List<string> visibleTodoIds = new List<string>();

        private const int TodoItemHeight = 30;
        private const int MaxVisibleTodos = 8;

        public TodoComponent(string id, string dataDir)
        {
            Id = id;
            dataFilePath = Path.Combine(dataDir, "__todo_list_data.xml");
        }

        public void Initialize()
        {
            var family = new FontFamily("Segoe UI");
            titleFont = new Font(family, 12, FontStyle.Bold);
            todoFont = new Font(family, 9);
            LoadState();
        }

        public void Render(Graphics g, Rectangle bounds)
        {
            Bounds = bounds;

            // Background
            g.FillRectangle(new SolidBrush(Color.FromArgb(30, 30, 30)), bounds);
            g.DrawRectangle(new Pen(Color.FromArgb(0, 120, 215), 2), bounds);

            // Header
            RenderHeader(g, bounds);

            // Add button
            addButtonRect = new Rectangle(bounds.Right - 35, bounds.Y + 10, 25, 25);
            g.DrawString("+", titleFont, Brushes.White, addButtonRect.X + 5, addButtonRect.Y + 3);

            // Todo list
            RenderTodoList(g, bounds);
        }

        private void RenderHeader(Graphics g, Rectangle bounds)
        {
            var headerRect = new Rectangle(bounds.X + 10, bounds.Y + 10, bounds.Width - 50, 30);
            g.DrawString("Todos", titleFont, Brushes.White, headerRect);
        }

        private void RenderTodoList(Graphics g, Rectangle bounds)
        {
            var startY = bounds.Y + 50;
            todoCheckBoxRects.Clear();
            todoTextRects.Clear();
            visibleTodoIds.Clear();

            var filteredTodos = GetFilteredTodos();
            var displayCount = Math.Min(filteredTodos.Count, MaxVisibleTodos);

            for (int i = 0; i < displayCount; i++)
            {
                var todo = filteredTodos[i];
                var y = startY + i * TodoItemHeight;

                // Checkbox rectangle
                var checkBoxRect = new Rectangle(bounds.X + 15, y + 8, 16, 16);
                todoCheckBoxRects.Add(checkBoxRect);
                visibleTodoIds.Add(todo.Id);

                // Draw checkbox
                g.DrawRectangle(new Pen(Color.White, todo.IsCompleted ? 2 : 1), checkBoxRect);

                if (todo.IsCompleted)
                {
                    // Draw checkmark
                    g.DrawLine(new Pen(Color.White, 2),
                        checkBoxRect.X + 3, checkBoxRect.Y + 8,
                        checkBoxRect.X + 7, checkBoxRect.Y + 12);
                    g.DrawLine(new Pen(Color.White, 2),
                        checkBoxRect.X + 7, checkBoxRect.Y + 12,
                        checkBoxRect.X + 13, checkBoxRect.Y + 3);
                }

                // Priority indicator
                Color priorityColor = GetPriorityColor(todo.Priority);
                var priorityRect = new Rectangle(bounds.X + 40, y + 8, 6, 6);
                g.FillEllipse(new SolidBrush(priorityColor), priorityRect);

                // Todo text
                var textRect = new Rectangle(bounds.X + 55, y, bounds.Width - 70, TodoItemHeight);
                todoTextRects.Add(textRect);

                var textColor = todo.IsCompleted ? Color.FromArgb(120, 120, 120) : Color.White;
                var textFormat = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter
                };

                g.DrawString(todo.Title, todoFont, new SolidBrush(textColor), textRect, textFormat);
            }

            // Show more indicator
            if (filteredTodos.Count > MaxVisibleTodos)
            {
                var moreY = startY + MaxVisibleTodos * TodoItemHeight;
                var moreText = $"...and {filteredTodos.Count - MaxVisibleTodos} more";
                g.DrawString(moreText, todoFont, new SolidBrush(Color.FromArgb(150, 150, 150)),
                    bounds.X + 15, moreY);
            }
        }

        private Color GetPriorityColor(int priority)
        {
            switch (priority)
            {
                case 3: return Color.FromArgb(255, 50, 50);    // High/Urgent - Red
                case 2: return Color.FromArgb(255, 165, 0);    // Medium - Orange
                case 1: return Color.FromArgb(50, 200, 50);    // Normal - Green
                default: return Color.FromArgb(150, 150, 150); // Low - Gray
            }
        }

        private List<TodoItem> GetFilteredTodos()
        {
            var result = new List<TodoItem>(todos);

            switch (filter)
            {
                case TodoFilter.Active:
                    result.RemoveAll(t => t.IsCompleted);
                    break;
                case TodoFilter.Completed:
                    result.RemoveAll(t => !t.IsCompleted);
                    break;
                case TodoFilter.ByPriority:
                    result.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                    break;
                case TodoFilter.ByDate:
                    result.Sort((a, b) => a.DueDate.CompareTo(b.DueDate));
                    break;
            }

            return result;
        }

        public void HandleMouseDown(Point location)
        {
            var relativePos = GetRelativePosition(location);

            // Check add button
            if (addButtonRect.Contains(relativePos))
            {
                // Trigger add todo dialog
                ShowAddTodoDialog();
                return;
            }

            // Check checkboxes
            for (int i = 0; i < todoCheckBoxRects.Count; i++)
            {
                if (todoCheckBoxRects[i].Contains(relativePos))
                {
                    ToggleTodo(visibleTodoIds[i]);
                    return;
                }
            }

            // Check double-click to edit
            for (int i = 0; i < todoTextRects.Count; i++)
            {
                if (todoTextRects[i].Contains(relativePos))
                {
                    // Can implement edit dialog here
                    var todo = todos.Find(t => t.Id == visibleTodoIds[i]);
                    if (todo != null)
                    {
                        ShowEditTodoDialog(todo);
                    }
                    return;
                }
            }
        }

        public void HandleMouseMove(Point location)
        {
            // Handle hover effects
        }

        public void HandleMouseUp(Point location)
        {
            // Handle drag release
        }

        public Size GetPreferredSize()
        {
            return preferredSize;
        }

        public void SaveState()
        {
            try
            {
                var todoList = new TodoList { Items = todos };
                var serializer = new XmlSerializer(typeof(TodoList));
                using (var writer = new StreamWriter(dataFilePath))
                {
                    serializer.Serialize(writer, todoList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving todo list data: {ex.Message}");
            }
        }

        public void LoadState()
        {
            try
            {
                if (File.Exists(dataFilePath))
                {
                    var serializer = new XmlSerializer(typeof(TodoList));
                    using (var reader = new StreamReader(dataFilePath))
                    {
                        var todoList = serializer.Deserialize(reader) as TodoList;
                        if (todoList != null)
                        {
                            todos = todoList.Items ?? new List<TodoItem>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading todo list data: {ex.Message}");
                todos = new List<TodoItem>();
            }
        }

        public void AddTodo(TodoItem todo)
        {
            todos.Add(todo);
            SaveState();
        }

        public void ToggleTodo(string todoId)
        {
            var todo = todos.Find(t => t.Id == todoId);
            if (todo != null)
            {
                todo.IsCompleted = !todo.IsCompleted;
                SaveState();
            }
        }

        public void DeleteTodo(string todoId)
        {
            todos.RemoveAll(t => t.Id == todoId);
            SaveState();
        }

        public void SetFilter(TodoFilter newFilter)
        {
            filter = newFilter;
        }

        public List<TodoItem> GetAllTodos()
        {
            return new List<TodoItem>(todos);
        }

        private void ShowAddTodoDialog()
        {
            // This would show a dialog to add a new todo item
            // For now, we'll add a sample todo
            var newTodo = new TodoItem("New Todo", "Description", DateTime.Today);
            AddTodo(newTodo);
        }

        private void ShowEditTodoDialog(TodoItem todo)
        {
            // This would show a dialog to edit the todo item
            // For now, just toggle completion
            ToggleTodo(todo.Id);
        }

        private Point GetRelativePosition(Point location)
        {
            return new Point(location.X - Bounds.X, location.Y - Bounds.Y);
        }
    }
}
