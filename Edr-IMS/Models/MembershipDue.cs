using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class MembershipDue
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    [DisplayName("Amount")]
    public decimal Amount { get; set; }

    [DisplayName("Currency")]
    public int CurrencyId { get; set; }

    [DisplayName("Membership Dues Month")]
    public int MembershipDuesMonthId { get; set; }

    [DisplayName("Membership Dues Type")]
    public int MembershipDuesTypeId { get; set; }

    [DisplayName("Is Active")]
    public bool IsActive { get; set; }

    [ScaffoldColumn(false)]
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; }

    [ForeignKey("CurrencyId")]
    [InverseProperty("MembershipDues")]
    public virtual Currency Currency { get; set; }

    [ForeignKey("MembershipDuesMonthId")]
    [InverseProperty("MembershipDues")]
    public virtual MembershipDuesMonth MembershipDuesMonth { get; set; }

    [ForeignKey("MembershipDuesTypeId")]
    [InverseProperty("MembershipDues")]
    public virtual MembershipDuesType MembershipDuesType { get; set; }
}
