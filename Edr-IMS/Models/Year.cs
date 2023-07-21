using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class Year
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [DisplayName("Name")]
    public string Name { get; set; }

    [DisplayName("Is Active")]
    public bool IsActive { get; set; }

    [ScaffoldColumn(false)]
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; }

    [InverseProperty("Year")]
    public virtual ICollection<Month> Months { get; set; } = new List<Month>();
}
