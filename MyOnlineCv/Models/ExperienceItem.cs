using System.ComponentModel.DataAnnotations;

namespace MyOnlineCv.Models
{
    public class ExperienceItem
    {
        [Key]
        public int Id { get; set; }
        public string Role { get; set; }
        public string Company { get; set; }
        public string Period { get; set; }
        public string Description { get; set; }
    }
}
