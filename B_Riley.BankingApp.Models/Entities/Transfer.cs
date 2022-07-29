using B_Riley.BankingApp.Models.Entities.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace B_Riley.BankingApp.Models.Entities
{
    public class Transfer: BaseEntity, IValidatableObject
    {
        [ForeignKey("FromAccount")]
        [Display(Name = "From Account")]
        public int FromAccountId { get; set; }

        [Display(Name = "From Account")]
        public Account? FromAccount { get; set; }


        [ForeignKey("ToAccount")]
        [Display(Name = "To Account")]
        public int ToAccountId { get; set; }

        [Display(Name = "To Account")]
        public Account? ToAccount { get; set; }


        [DataType(DataType.Currency)]
        [Range(1, 10000)]
        [Display(Name = "Amount to transfer")]
        public double Amount { get; set; } = 0;


        [DataType(DataType.Currency)]
        [Display(Name = "From Account Balance")]
        public double FromAccountBalance { get; set; }


        [DataType(DataType.Currency)]
        [Display(Name = "To Account Balance")]
        public double ToAccountBalance { get; set; }


        [Display(Name = "Transaction Time")]
        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy HH:mm:ss}")]
        public DateTime TransactionTime { get; set; }


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (FromAccountId < 1)
                yield return new ValidationResult("Invalid From-Account.", new[] { nameof(FromAccountId) });

            if (ToAccountId < 1)
                yield return new ValidationResult("Invalid To-Account.", new[] { nameof(ToAccountId) });

            if (FromAccountId == ToAccountId)
                yield return new ValidationResult("Self Transfer is not allowed.", new[] { nameof(ToAccountId) });

        }
    }
}
