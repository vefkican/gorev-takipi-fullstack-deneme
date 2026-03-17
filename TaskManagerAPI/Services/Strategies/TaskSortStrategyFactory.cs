namespace TaskManagerAPI.Services.Strategies
{
    public static class TaskSortStrategyFactory
    {
        public static ITaskSortStrategy Create(string? sortBy)
        {
            return sortBy switch
            {
                "title" => new SortByTitleStrategy(),
                "dueDate" => new SortByDueDateStrategy(),
                "completed" => new SortByCompletedStrategy(),
                _ => new SortByDateStrategy()
            };
        }
    }
}