using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class Zone
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Region")]
    public int RegionId { get; set; }

    [Required]
    [StringLength(100)]
    [DisplayName("Name")]
    public string Name { get; set; }

    [Required]
    [DisplayName("Is Active")]
    public bool? IsActive { get; set; }

    [ScaffoldColumn(false)]
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; }

    [ForeignKey("RegionId")]
    [InverseProperty("Zones")]
    public virtual Region Region { get; set; }

    [InverseProperty("Zone")]
    public virtual ICollection<Woreda> Woreda { get; set; } = new List<Woreda>();
}
