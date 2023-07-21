using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class SubCity
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [DisplayName("Name")]
    public string Name { get; set; }

    [DisplayName("City")]
    public int CityId { get; set; }

    [DisplayName("Is Active")]
    public bool IsActive { get; set; }

    [ScaffoldColumn(false)]
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; }

    [ForeignKey("CityId")]
    [InverseProperty("SubCities")]
    public virtual City City { get; set; }

    [InverseProperty("SubCity")]
    public virtual ICollection<Kebele> Kebeles { get; set; } = new List<Kebele>();

    [InverseProperty("SubCity")]
    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    [InverseProperty("SubCity")]
    public virtual ICollection<MembersAdoptedChildren> MembersAdoptedChildren { get; set; } = new List<MembersAdoptedChildren>();

    [InverseProperty("SubCity")]
    public virtual ICollection<MembersChildren> MembersChildren { get; set; } = new List<MembersChildren>();

    [InverseProperty("SubCity")]
    public virtual ICollection<MembersElderlyParent> MembersElderlyParents { get; set; } = new List<MembersElderlyParent>();

    [InverseProperty("SubCity")]
    public virtual ICollection<MembersRelative> MembersRelatives { get; set; } = new List<MembersRelative>();

    [InverseProperty("SubCity")]
    public virtual ICollection<MembersSpouseRelative> MembersSpouseRelatives { get; set; } = new List<MembersSpouseRelative>();

    [InverseProperty("SubCity")]
    public virtual ICollection<MembersWard> MembersWards { get; set; } = new List<MembersWard>();
}
