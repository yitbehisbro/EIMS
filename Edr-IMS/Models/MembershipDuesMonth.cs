using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class MembershipDuesMonth
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [DisplayName("Name")]
    public string Name { get; set; }

    [DisplayName("Membership Dues Year")]
    public int MembershipDuesYearId { get; set; }

    [DisplayName("Is Active")]
    public bool IsActive { get; set; }

    [ScaffoldColumn(false)]
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; }

    [InverseProperty("MembershipDuesMonth")]
    public virtual ICollection<MembershipDue> MembershipDues { get; set; } = new List<MembershipDue>();

    [InverseProperty("MembershipDuesMonths")]
    public virtual ICollection<MembershipDuesPayment> MembershipDuesPayments { get; set; } = new List<MembershipDuesPayment>();

    [ForeignKey("MembershipDuesYearId")]
    [InverseProperty("MembershipDuesMonths")]
    public virtual MembershipDuesYear MembershipDuesYear { get; set; }
}
