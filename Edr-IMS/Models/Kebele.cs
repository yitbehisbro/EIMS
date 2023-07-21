using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class Kebele
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [DisplayName("Name")]
    public string Name { get; set; }

    [DisplayName("Sub City")]
    public int SubCityId { get; set; }

    [DisplayName("Is Active")]
    public bool IsActive { get; set; }

    [ScaffoldColumn(false)]
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; }

    [InverseProperty("Kebele")]
    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    [InverseProperty("Kebele")]
    public virtual ICollection<MembersAdoptedChildren> MembersAdoptedChildren { get; set; } = new List<MembersAdoptedChildren>();

    [InverseProperty("Kebele")]
    public virtual ICollection<MembersChildren> MembersChildren { get; set; } = new List<MembersChildren>();

    [InverseProperty("Kebele")]
    public virtual ICollection<MembersElderlyParent> MembersElderlyParents { get; set; } = new List<MembersElderlyParent>();

    [InverseProperty("Kebele")]
    public virtual ICollection<MembersRelative> MembersRelatives { get; set; } = new List<MembersRelative>();

    [InverseProperty("Kebele")]
    public virtual ICollection<MembersSpouseRelative> MembersSpouseRelatives { get; set; } = new List<MembersSpouseRelative>();

    [InverseProperty("Kebele")]
    public virtual ICollection<MembersWard> MembersWards { get; set; } = new List<MembersWard>();

    [ForeignKey("SubCityId")]
    [InverseProperty("Kebeles")]
    public virtual SubCity SubCity { get; set; }
}
