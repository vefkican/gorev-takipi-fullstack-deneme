using System.ComponentModel.DataAnnotations;

namespace TaskManagerAPI.Models
{
    public class CreateTaskDto
    {
        [Required(ErrorMessage = "Title boş olamaz!")]
        [MinLength(3, ErrorMessage = "Title en az 3 karakter olmalı!")]
        [MaxLength(100, ErrorMessage = "Title en fazla 100 karakter olabilir!")]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime? DueDate { get; set; }
    }
}