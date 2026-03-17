using TaskManagerAPI.Models.Entities;

namespace TaskManagerAPI.Services.Strategies
{
    public class SortByDateStrategy : ITaskSortStrategy
    {
        public IQueryable<TaskItem> Sort(IQueryable<TaskItem> tasks)
            => tasks.OrderByDescending(t => t.CreatedAt);
    }

    public class SortByTitleStrategy : ITaskSortStrategy
    {
        public IQueryable<TaskItem> Sort(IQueryable<TaskItem> tasks)
            => tasks.OrderByDescending(t => t.Title);
    }

    public class SortByDueDateStrategy : ITaskSortStrategy
    {
        public IQueryable<TaskItem> Sort(IQueryable<TaskItem> tasks)
            => tasks.OrderByDescending(t => t.DueDate);
    }

    public class SortByCompletedStrategy : ITaskSortStrategy
    {
        public IQueryable<TaskItem> Sort(IQueryable<TaskItem> tasks)
            => tasks.OrderByDescending(t => t.IsCompleted);
    }
}