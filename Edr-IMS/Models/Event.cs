﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EdrIMS.Models;

public partial class Event
{
    [Key]
    [DisplayName("Id")]
    public int Id { get; set; }

    [DisplayName("Event Type")]
    public int EventTypeId { get; set; }

    [DisplayName("Location")]
    public string Location { get; set; }

    [DisplayName("Event Weight")]
    public int EventWeightId { get; set; }

    [DisplayName("Event Attendance Mode")]
    public int EventAttendanceModeId { get; set; }

    [DisplayName("Date Of Event")]
    public DateTime DateOfEvent { get; set; }

    [Required]
    [DisplayName("Description")]
    public string Description { get; set; }

    [DisplayName("Is Active")]
    public bool IsActive { get; set; }

    [ScaffoldColumn(false)]
    [DisplayName("Is Deleted")]
    public bool IsDeleted { get; set; }

    [ForeignKey("EventAttendanceModeId")]
    [InverseProperty("Events")]
    public virtual EventAttendanceMode EventAttendanceMode { get; set; }

    [ForeignKey("EventTypeId")]
    [InverseProperty("Events")]
    public virtual EventType EventType { get; set; }

    [ForeignKey("EventWeightId")]
    [InverseProperty("Events")]
    public virtual EventWeight EventWeight { get; set; }
}
