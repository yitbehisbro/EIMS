using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class Gender
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [Required]
    [StringLength(10)]
    [DisplayName("Name")]
    public string Name { get; set; }

    [DisplayName("Is Active")]
    public bool IsActive { get; set; }

    [ScaffoldColumn(false)]
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; }

    [InverseProperty("Gender")]
    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    [InverseProperty("Gender")]
    public virtual ICollection<MembersAdoptedChildren> MembersAdoptedChildren { get; set; } = new List<MembersAdoptedChildren>();

    [InverseProperty("Gender")]
    public virtual ICollection<MembersChildren> MembersChildren { get; set; } = new List<MembersChildren>();

    [InverseProperty("Gender")]
    public virtual ICollection<MembersElderlyParent> MembersElderlyParents { get; set; } = new List<MembersElderlyParent>();

    [InverseProperty("Gender")]
    public virtual ICollection<MembersRelative> MembersRelatives { get; set; } = new List<MembersRelative>();

    [InverseProperty("Gender")]
    public virtual ICollection<MembersSpouseRelative> MembersSpouseRelatives { get; set; } = new List<MembersSpouseRelative>();

    [InverseProperty("Gender")]
    public virtual ICollection<MembersWard> MembersWards { get; set; } = new List<MembersWard>();
}
