using System.ComponentModel.DataAnnotations;

namespace BulkyWeb.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; } //if name is Id, then EF will know this is PK
        [Required]
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
    }
}
