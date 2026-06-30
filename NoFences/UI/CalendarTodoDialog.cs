using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NoFences.Model;
using NoFences.Components;

namespace NoFences.UI
{
    public partial class CalendarTodoDialog : Form
    {
        private readonly CalendarComponent _calendar;
        private readonly DateTime _date;
        private List<TodoItem> _todos;

        public CalendarTodoDialog(CalendarComponent calendar, DateTime date)
        {
            _calendar = calendar;
            _date = date;
            _todos = _calendar.GetTodosForDate(date);
            InitializeComponent();
            Text = $"待办事项 - {date:yyyy-MM-dd}";
            UpdateTodoList();
        }

        private void UpdateTodoList()
        {
            listBoxTodos.Items.Clear();
            foreach (var todo in _todos)
            {
                var displayText = todo.IsCompleted ? $"✓ {todo.Title}" : todo.Title;
                listBoxTodos.Items.Add(displayText);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                var todo = new TodoItem
                {
                    Title = txtTitle.Text,
                    Description = txtDescription.Text,
                    DueDate = _date,
                    IsCompleted = false
                };
                _calendar.AddTodoItem(todo);
                _todos = _calendar.GetTodosForDate(_date);
                UpdateTodoList();
                txtTitle.Clear();
                txtDescription.Clear();
            }
        }

        private void btnToggle_Click(object sender, EventArgs e)
        {
            if (listBoxTodos.SelectedIndex >= 0)
            {
                var selectedTodo = _todos[listBoxTodos.SelectedIndex];
                _calendar.ToggleTodoItem(selectedTodo.Id);
                _todos = _calendar.GetTodosForDate(_date);
                UpdateTodoList();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listBoxTodos.SelectedIndex >= 0)
            {
                var selectedTodo = _todos[listBoxTodos.SelectedIndex];
                _calendar.RemoveTodoItem(selectedTodo.Id);
                _todos = _calendar.GetTodosForDate(_date);
                UpdateTodoList();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
