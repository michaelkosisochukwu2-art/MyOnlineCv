using System.ComponentModel.DataAnnotations;

namespace MyOnlineCv.Models
{
   
    public class TaxCalculationResult
    {
        [Key]
        public int Id { get; set; }
        public decimal GrossIncome { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal EstimatedTax { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    }
}