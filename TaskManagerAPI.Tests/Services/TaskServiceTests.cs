using FluentAssertions;
using Moq;
using TaskManagerAPI.Models.DTOs;
using TaskManagerAPI.Models.Entities;
using TaskManagerAPI.Repositories;
using TaskManagerAPI.Services;

namespace TaskManagerAPI.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly Mock<ITaskRepository> _mockRepository;
        private readonly TaskService _taskService;

        public TaskServiceTests()
        {
            _mockRepository = new Mock<ITaskRepository>();
            _taskService = new TaskService(_mockRepository.Object);
        }

        [Fact]
        public async Task CreateAsync_ShouldReturnTaskWithCorrectValues()
        {
            // Arrange
            var userId = 1;
            var dto = new CreateTaskDto
            {
                Title = "Test Task",
                Description = "Test Description"
            };

            _mockRepository
                .Setup(r => r.CreateAsync(It.IsAny<TaskItem>()))
                .ReturnsAsync((TaskItem t) => t);

            // Act
            var result = await _taskService.CreateAsync(userId, dto);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Test Task");
            result.Description.Should().Be("Test Description");
            result.UserId.Should().Be(userId);
            result.IsCompleted.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WhenTaskNotFound_ShouldReturnFalse()
        {
            // Arrange
            _mockRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((TaskItem?)null);

            // Act
            var result = await _taskService.DeleteAsync(1, 1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public async Task DeleteAsync_WhenTaskFound_ShouldReturnTrue()
        {
            // Arrange
            var task = new TaskItem { Id = 1, Title = "Test", UserId = 1 };

            _mockRepository
                .Setup(r => r.GetByIdAsync(1, 1))
                .ReturnsAsync(task);

            _mockRepository
                .Setup(r => r.DeleteAsync(task))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _taskService.DeleteAsync(1, 1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task UpdateAsync_WhenTaskNotFound_ShouldReturnFalse()
        {
            // Arrange
            _mockRepository
                .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((TaskItem?)null);

            // Act
            var result = await _taskService.UpdateAsync(1, 1, new TaskItem());

            // Assert
            result.Should().BeFalse();
        }
    }
}