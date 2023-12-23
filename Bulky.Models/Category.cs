using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bulky.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; } //if name is just Id, then EF will know this is PK, otherwise data annotation of [Key] is needed

        [Required]
        [MaxLength(30)]
        [DisplayName("Category Name")]
        public string Name { get; set; }

        [Range(1, 100, ErrorMessage = "Must Be Between 1-100")]
        [DisplayName("Category Order")]
        public int DisplayOrder { get; set; }
    }
}
