using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using NoFences.Model;
using NoFences.UI;

namespace NoFences.Components
{
    public class CalendarComponent : IComponent
    {
        public string Id { get; private set; }
        public ComponentType Type => ComponentType.Calendar;
        public Rectangle Bounds { get; set; }
        public bool Visible { get; set; } = true;

        private DateTime selectedDate = DateTime.Today;
        private DateTime viewMonth = DateTime.Today;
        private int lastClickDay = -1;
        private DateTime lastClickDate = DateTime.MinValue;
        private List<TodoItem> todoItems = new List<TodoItem>();
        private Color themeColor = Color.FromArgb(0, 120, 215);

        private Font monthFont;
        private Font dayFont;
        private Font headerFont;
        private Size preferredSize = new Size(350, 320);

        private Rectangle prevMonthButton;
        private Rectangle nextMonthButton;
        private Rectangle calendarGridRect;

        private string dataFilePath;

        public CalendarComponent(string id, string dataDir)
        {
            Id = id;
            dataFilePath = Path.Combine(dataDir, "__calendar_data.xml");
        }

        public void Initialize()
        {
            var family = new FontFamily("Segoe UI");
            monthFont = new Font(family, 12, FontStyle.Bold);
            dayFont = new Font(family, 9);
            headerFont = new Font(family, 9, FontStyle.Bold);
            LoadState();
        }

        public void Render(Graphics g, Rectangle bounds)
        {
            Bounds = bounds;

            // Background
            g.FillRectangle(new SolidBrush(Color.FromArgb(30, 30, 30)), bounds);
            g.DrawRectangle(new Pen(themeColor, 2), bounds);

            // Header with month navigation
            RenderMonthHeader(g, bounds);

            // Calendar grid
            calendarGridRect = new Rectangle(bounds.X + 10, bounds.Y + 50, bounds.Width - 20, bounds.Height - 60);
            RenderCalendarGrid(g, calendarGridRect);
        }

        private void RenderMonthHeader(Graphics g, Rectangle bounds)
        {
            var headerRect = new Rectangle(bounds.X, bounds.Y, bounds.Width, 45);

            // Month/Year text
            var monthText = viewMonth.ToString("yyyy年 M月");
            var textSize = g.MeasureString(monthText, monthFont);
            var textPos = new PointF(headerRect.X + headerRect.Width / 2 - textSize.Width / 2, headerRect.Y + 10);

            g.DrawString(monthText, monthFont, Brushes.White, textPos);

            // Navigation buttons
            prevMonthButton = new Rectangle(headerRect.X + 10, headerRect.Y + 10, 25, 25);
            nextMonthButton = new Rectangle(headerRect.Right - 35, headerRect.Y + 10, 25, 25);

            g.DrawString("<", monthFont, Brushes.White, prevMonthButton.X + 5, prevMonthButton.Y + 3);
            g.DrawString(">", monthFont, Brushes.White, nextMonthButton.X + 5, nextMonthButton.Y + 3);
        }

        private void RenderCalendarGrid(Graphics g, Rectangle bounds)
        {
            // Day headers
            var dayHeaders = new[] { "日", "一", "二", "三", "四", "五", "六" };
            var cellWidth = bounds.Width / 7;
            var cellHeight = (bounds.Height - 20) / 6;
            var centerFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

            for (int i = 0; i < 7; i++)
            {
                var headerRect = new Rectangle(bounds.X + i * cellWidth, bounds.Y, cellWidth, 20);
                g.DrawString(dayHeaders[i], headerFont, new SolidBrush(Color.FromArgb(180, 180, 180)), headerRect, centerFormat);
            }

            // Calculate first day of month
            var firstOfMonth = new DateTime(viewMonth.Year, viewMonth.Month, 1);
            var firstDayOfWeek = (int)firstOfMonth.DayOfWeek;
            var daysInMonth = DateTime.DaysInMonth(viewMonth.Year, viewMonth.Month);

            int dayCounter = 1;
            for (int row = 0; row < 6; row++)
            {
                for (int col = 0; col < 7; col++)
                {
                    var cellRect = new Rectangle(
                        bounds.X + col * cellWidth + 2,
                        bounds.Y + 25 + row * cellHeight + 2,
                        cellWidth - 4,
                        cellHeight - 4
                    );

                    if (row == 0 && col < firstDayOfWeek)
                    {
                        // Empty cells before first day
                        continue;
                    }

                    if (dayCounter > daysInMonth)
                        break;

                    var currentDay = new DateTime(viewMonth.Year, viewMonth.Month, dayCounter);
                    var isToday = currentDay.Date == DateTime.Today;
                    var isSelected = currentDay.Date == selectedDate.Date;
                    var hasTodos = HasTodosOnDate(currentDay);

                    // Draw cell background
                    if (isToday)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(50, themeColor)), cellRect);
                    }

                    if (isSelected)
                    {
                        g.DrawRectangle(new Pen(themeColor, 2), cellRect);
                    }

                    // Draw day number
                    g.DrawString(dayCounter.ToString(), dayFont, Brushes.White, cellRect, centerFormat);

                    // Draw todo indicator
                    if (hasTodos)
                    {
                        var todoDotRect = new Rectangle(cellRect.Right - 8, cellRect.Bottom - 8, 6, 6);
                        g.FillEllipse(new SolidBrush(themeColor), todoDotRect);
                    }

                    dayCounter++;
                }

                if (dayCounter > daysInMonth)
                    break;
            }
        }

        private bool HasTodosOnDate(DateTime date)
        {
            foreach (var todo in todoItems)
            {
                if (todo.DueDate.Date == date.Date && !todo.IsCompleted)
                    return true;
            }
            return false;
        }

        public void HandleMouseDown(Point location)
        {
            var relativePos = GetRelativePosition(location);

            if (prevMonthButton.Contains(relativePos))
            {
                viewMonth = viewMonth.AddMonths(-1);
            }
            else if (nextMonthButton.Contains(relativePos))
            {
                viewMonth = viewMonth.AddMonths(1);
            }
            else if (calendarGridRect.Contains(relativePos))
            {
                var clickedDate = GetDateFromClick(relativePos);
                if (clickedDate != DateTime.MinValue)
                {
                    // Check for double click
                    if (lastClickDay == clickedDate.Day && lastClickDate.Month == clickedDate.Month && lastClickDate.Year == clickedDate.Year)
                    {
                        // Double click - open todo dialog
                        OpenTodoDialog(clickedDate);
                        lastClickDay = -1;
                        lastClickDate = DateTime.MinValue;
                    }
                    else
                    {
                        // Single click - select date
                        selectedDate = clickedDate;
                        lastClickDay = clickedDate.Day;
                        lastClickDate = clickedDate;
                    }
                }
            }
        }

        private DateTime GetDateFromClick(Point location)
        {
            var cellWidth = calendarGridRect.Width / 7;
            var cellHeight = (calendarGridRect.Height - 20) / 6;

            var col = (location.X - calendarGridRect.X) / cellWidth;
            var row = (location.Y - (calendarGridRect.Y + 25)) / cellHeight;

            if (row < 0 || row >= 6 || col < 0 || col >= 7)
                return DateTime.MinValue;

            var firstOfMonth = new DateTime(viewMonth.Year, viewMonth.Month, 1);
            var firstDayOfWeek = (int)firstOfMonth.DayOfWeek;
            var daysInMonth = DateTime.DaysInMonth(viewMonth.Year, viewMonth.Month);

            var dayIndex = row * 7 + col - firstDayOfWeek + 1;

            if (dayIndex >= 1 && dayIndex <= daysInMonth)
            {
                return new DateTime(viewMonth.Year, viewMonth.Month, dayIndex);
            }

            return DateTime.MinValue;
        }

        private void OpenTodoDialog(DateTime date)
        {
            using (var dialog = new CalendarTodoDialog(this, date))
            {
                dialog.ShowDialog();
            }
        }

        public void HandleMouseMove(Point location)
        {
            // Handle hover effects if needed
        }

        public void HandleMouseUp(Point location)
        {
            // Handle drag release if needed
        }

        public Size GetPreferredSize()
        {
            return preferredSize;
        }

        public void SaveState()
        {
            try
            {
                var todoList = new TodoList { Items = todoItems };
                var serializer = new XmlSerializer(typeof(TodoList));
                using (var writer = new StreamWriter(dataFilePath))
                {
                    serializer.Serialize(writer, todoList);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving calendar data: {ex.Message}");
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
                            todoItems = todoList.Items ?? new List<TodoItem>();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading calendar data: {ex.Message}");
                todoItems = new List<TodoItem>();
            }
        }

        public void AddTodoItem(TodoItem item)
        {
            todoItems.Add(item);
            SaveState();
        }

        public void RemoveTodoItem(string todoId)
        {
            todoItems.RemoveAll(t => t.Id == todoId);
            SaveState();
        }

        public void ToggleTodoItem(string todoId)
        {
            var todo = todoItems.Find(t => t.Id == todoId);
            if (todo != null)
            {
                todo.IsCompleted = !todo.IsCompleted;
                SaveState();
            }
        }

        public List<TodoItem> GetTodosForDate(DateTime date)
        {
            var result = new List<TodoItem>();
            foreach (var todo in todoItems)
            {
                if (todo.DueDate.Date == date.Date)
                {
                    result.Add(todo);
                }
            }
            return result;
        }

        private Point GetRelativePosition(Point location)
        {
            return new Point(location.X - Bounds.X, location.Y - Bounds.Y);
        }
    }
}
