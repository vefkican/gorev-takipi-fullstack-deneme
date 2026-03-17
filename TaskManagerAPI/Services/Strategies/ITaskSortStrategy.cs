using TaskManagerAPI.Models.Entities;

namespace TaskManagerAPI.Services.Strategies
{
    public interface ITaskSortStrategy
    {
        public IQueryable<TaskItem> Sort(IQueryable<TaskItem> tasks)
        => tasks.OrderBy(t => t.Title.ToLower());
    }
}