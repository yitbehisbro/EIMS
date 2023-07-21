using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class Woreda
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Zone")]
    public int ZoneId { get; set; }

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

    [ForeignKey("ZoneId")]
    [InverseProperty("Woreda")]
    public virtual Zone Zone { get; set; }
}
