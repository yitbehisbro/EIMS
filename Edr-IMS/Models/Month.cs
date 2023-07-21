using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class Month
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [DisplayName("Name")]
    public string Name { get; set; }

    [DisplayName("Year")]
    public int YearId { get; set; }

    [DisplayName("Is Active")]
    public bool IsActive { get; set; }

    [ScaffoldColumn(false)]
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; }

    [InverseProperty("Month")]
    public virtual ICollection<DeathNotificationsPayment> DeathNotificationsPayments { get; set; } = new List<DeathNotificationsPayment>();

    [InverseProperty("Month")]
    public virtual ICollection<MembersDeathNotificationsPayment> MembersDeathNotificationsPayments { get; set; } = new List<MembersDeathNotificationsPayment>();

    [InverseProperty("Month")]
    public virtual ICollection<MembersMembershipDuesPayment> MembersMembershipDuesPayments { get; set; } = new List<MembersMembershipDuesPayment>();

    [InverseProperty("Month")]
    public virtual ICollection<MembersSpecialPayment> MembersSpecialPayments { get; set; } = new List<MembersSpecialPayment>();

    [InverseProperty("Month")]
    public virtual ICollection<MembershipDue> MembershipDues { get; set; } = new List<MembershipDue>();

    [InverseProperty("Month")]
    public virtual ICollection<SpecialPayment> SpecialPayments { get; set; } = new List<SpecialPayment>();

    [ForeignKey("YearId")]
    [InverseProperty("Months")]
    public virtual MembershipDuesYear Year { get; set; }
}
