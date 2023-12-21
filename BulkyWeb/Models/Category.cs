using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BulkyWeb.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; } //if name is just Id, then EF will know this is PK, otherwise data annotation of [Key] is needed
        [Required]
        [DisplayName("Category Name")]
        public string Name { get; set; }
        [DisplayName("Category Order")]
        public int DisplayOrder { get; set; }
    }
}
