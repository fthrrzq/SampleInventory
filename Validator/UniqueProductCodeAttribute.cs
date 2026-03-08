using SampleInventory.Database;
using System.ComponentModel.DataAnnotations;

namespace SampleInventory.Validator
{
    public class UniqueProductCodeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Product code is required");
            }

            var code = value.ToString();
            var context = (ApplicationDbContext)validationContext.GetService(typeof(ApplicationDbContext));

            var existing = context.Products.Any(p => p.Code == code);

            if (existing)
            {
                return new ValidationResult($"Product code '{code}' already exists");
            }

            return ValidationResult.Success;
        }
    }
}
