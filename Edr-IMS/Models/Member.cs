using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class Member
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Users")]
    public int? UsersId { get; set; }

    [DisplayName("File Number")]
    public int FileNumber { get; set; }

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
    [InverseProperty("Members")]
    public virtual City City { get; set; }

    [InverseProperty("Member")]
    public virtual ICollection<DeceasedMember> DeceasedMembers { get; set; } = new List<DeceasedMember>();

    [InverseProperty("Participant")]
    public virtual ICollection<EventParticipant> EventParticipants { get; set; } = new List<EventParticipant>();

    [ForeignKey("GenderId")]
    [InverseProperty("Members")]
    public virtual Gender Gender { get; set; }

    [ForeignKey("KebeleId")]
    [InverseProperty("Members")]
    public virtual Kebele Kebele { get; set; }

    [InverseProperty("Members")]
    public virtual ICollection<MembersAdoptedChildren> MembersAdoptedChildren { get; set; } = new List<MembersAdoptedChildren>();

    [InverseProperty("Members")]
    public virtual ICollection<MembersChildren> MembersChildren { get; set; } = new List<MembersChildren>();

    [InverseProperty("Members")]
    public virtual ICollection<MembersElderlyParent> MembersElderlyParents { get; set; } = new List<MembersElderlyParent>();

    [InverseProperty("Members")]
    public virtual ICollection<MembersSpouseRelative> MembersSpouseRelatives { get; set; } = new List<MembersSpouseRelative>();

    [InverseProperty("Members")]
    public virtual ICollection<MembersWard> MembersWards { get; set; } = new List<MembersWard>();

    [InverseProperty("Member")]
    public virtual ICollection<MembershipDuesPayment> MembershipDuesPayments { get; set; } = new List<MembershipDuesPayment>();

    [ForeignKey("RegisteredBy")]
    [InverseProperty("MemberRegisteredByNavigations")]
    public virtual User RegisteredByNavigation { get; set; }

    [InverseProperty("Member")]
    public virtual ICollection<RelativesRelation> RelativesRelations { get; set; } = new List<RelativesRelation>();

    [ForeignKey("SubCityId")]
    [InverseProperty("Members")]
    public virtual SubCity SubCity { get; set; }

    [ForeignKey("UsersId")]
    [InverseProperty("MemberUsers")]
    public virtual User Users { get; set; }
}
