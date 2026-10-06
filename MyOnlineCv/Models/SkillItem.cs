using System.ComponentModel.DataAnnotations;

namespace MyOnlineCv.Models
{
    public class SkillItem
    {
        [Key]
        public int Id { get; set; }
        public string Category { get; set; }
        public string Name { get; set; } 
    }
}
