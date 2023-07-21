using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class RelativesRelation
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Member")]
    public int MemberId { get; set; }

    [DisplayName("Relation Type")]
    public int RelationTypeId { get; set; }

    [DisplayName("Is Active")]
    public bool IsActive { get; set; }

    [ScaffoldColumn(false)]
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; }

    [ForeignKey("MemberId")]
    [InverseProperty("RelativesRelations")]
    public virtual Member Member { get; set; }

    [InverseProperty("RelativesRelation")]
    public virtual ICollection<MembersRelative> MembersRelatives { get; set; } = new List<MembersRelative>();

    [ForeignKey("RelationTypeId")]
    [InverseProperty("RelativesRelations")]
    public virtual RelationType RelationType { get; set; }
}
