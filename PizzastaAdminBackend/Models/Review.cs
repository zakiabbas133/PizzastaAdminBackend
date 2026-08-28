using System.ComponentModel.DataAnnotations;

namespace PizzastaAdminBackend.Models
{
    public class Review
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public bool Verified { get; set; }

        public bool IsPublished { get; set; } = true;
    }
}