using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;

namespace MyOnlineCv.Pages
{
    public class TaxCalculatorModel : PageModel
    {
        [BindProperty]
        public decimal GrossAnnualIncome { get; set; }

        public decimal ConsolidatedRelief { get; set; } = 0m;
        public decimal TaxableIncome { get; set; }
        public decimal AnnualTax { get; set; }
        public decimal MonthlyTax { get; set; }
        public bool IsCalculated { get; set; } = false;

        public void OnGet()
        {
        }

        public void OnPost()
        {
            if (GrossAnnualIncome <= 0) return;

            // 1. No Relief Applied - Entire Gross Income is Taxable
            ConsolidatedRelief = 0m;
            TaxableIncome = GrossAnnualIncome;

            // 2. Compute Progressive PAYE Tax Brackets on Full Amount
            decimal tax = 0m;
            decimal remainder = TaxableIncome;

            if (remainder > 0)
            {
                decimal chunk = Math.Min(remainder, 300_000m);
                tax += chunk * 0.07m; // First 300k @ 7%
                remainder -= chunk;
            }
            if (remainder > 0)
            {
                decimal chunk = Math.Min(remainder, 300_000m);
                tax += chunk * 0.11m; // Next 300k @ 11%
                remainder -= chunk;
            }
            if (remainder > 0)
            {
                decimal chunk = Math.Min(remainder, 500_000m);
                tax += chunk * 0.15m; // Next 500k @ 15%
                remainder -= chunk;
            }
            if (remainder > 0)
            {
                decimal chunk = Math.Min(remainder, 500_000m);
                tax += chunk * 0.19m; // Next 500k @ 19%
                remainder -= chunk;
            }
            if (remainder > 0)
            {
                decimal chunk = Math.Min(remainder, 1_600_000m);
                tax += chunk * 0.21m; // Next 1.6M @ 21%
                remainder -= chunk;
            }
            if (remainder > 0)
            {
                tax += remainder * 0.24m; // Above 3.2M @ 24%
            }

            AnnualTax = tax;
            MonthlyTax = tax / 12m;
            IsCalculated = true;
        }
    }
}