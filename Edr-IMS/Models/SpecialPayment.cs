using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class SpecialPayment
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    [DisplayName("Amount")]
    public decimal Amount { get; set; }

    [DisplayName("Currency")]
    public int CurrencyId { get; set; }

    [DisplayName("Month")]
    public int MonthId { get; set; }

    [DisplayName("Is Active")]
    public bool IsActive { get; set; }

    [ScaffoldColumn(false)]
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("SpecialPayments")]
    public virtual Currency Currency { get; set; }

    [ForeignKey("MonthId")]
    [InverseProperty("SpecialPayments")]
    public virtual Month Month { get; set; }
}
