using B_Riley.BankingApp.Models.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace B_Riley.BankingApp.Models.Entities
{
    public class Account: BaseEntity
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Account Name")]        
        public string AccountName { get; set; }


        [Range(0, int.MaxValue)]
        [DataType(DataType.Currency)]
        [Display(Name = "Balance")]
        public double Balance { get; set; }


        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy HH:mm:ss}")]
        public DateTime DateCreated { get; set; }


        [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy HH:mm:ss}")]
        public DateTime DateModified { get; set; }
    }
}