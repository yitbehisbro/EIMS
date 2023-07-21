using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class MembersRelative
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Relatives Relation")]
    public int RelativesRelationId { get; set; }

    [Required]
    [StringLength(50)]
    [DisplayName("First Name")]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    [DisplayName("Middle Name")]
    public string MiddleName { get; set; }

    [Required]
    [StringLength(50)]
    [DisplayName("Last Name")]
    public string LastName { get; set; }

    [DisplayName("Gender")]
    public int GenderId { get; set; }

    [Required]
    [StringLength(50)]
    [DisplayName("Birth Place")]
    public string BirthPlace { get; set; }

    [Column(TypeName = "datetime")]
    [DisplayName("Date Of Birth")]
    public DateTime DateOfBirth { get; set; }

    [DisplayName("City")]
    public int CityId { get; set; }

    [DisplayName("Sub City")]
    public int SubCityId { get; set; }

    [DisplayName("Kebele")]
    public int KebeleId { get; set; }

    [Required]
    [StringLength(20)]
    [DisplayName("Phone Number")]
    public string PhoneNumber { get; set; }

    [DisplayName("Photo Url")]
    public string PhotoUrl { get; set; }

    [DisplayName("Date Of Registration")]
    public DateTime? DateOfRegistration { get; set; }

    [DisplayName("Registered By")]
    public int? RegisteredBy { get; set; }

    [DisplayName("Is Alive")]
    public bool IsAlive { get; set; }

    [DisplayName("Is Active")]
    public bool IsActive { get; set; }

    [ScaffoldColumn(false)]
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; }

    [ForeignKey("CityId")]
    [InverseProperty("MembersRelatives")]
    public virtual City City { get; set; }

    [InverseProperty("MembersRelative")]
    public virtual ICollection<DeceasedMembersRelative> DeceasedMembersRelatives { get; set; } = new List<DeceasedMembersRelative>();

    [ForeignKey("GenderId")]
    [InverseProperty("MembersRelatives")]
    public virtual Gender Gender { get; set; }

    [ForeignKey("KebeleId")]
    [InverseProperty("MembersRelatives")]
    public virtual Kebele Kebele { get; set; }

    [ForeignKey("RegisteredBy")]
    [InverseProperty("MembersRelatives")]
    public virtual User RegisteredByNavigation { get; set; }

    [ForeignKey("RelativesRelationId")]
    [InverseProperty("MembersRelatives")]
    public virtual RelativesRelation RelativesRelation { get; set; }

    [ForeignKey("SubCityId")]
    [InverseProperty("MembersRelatives")]
    public virtual SubCity SubCity { get; set; }
}
