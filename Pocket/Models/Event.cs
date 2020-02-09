using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Table("events")]
    public class Event
    {
        [Required()]
        [Column("event_id")]
        [Key()]
        public int EventID { get; set; }

        [Required()]
        [Column("user_id", Order = 1)]
        public int UserID { get; set; }
        public virtual User User { get; set; }

        [Required()]
        [Display(Name = "Event Name", Description = "Name of Event")]
        [Column("event_name")]
        [StringLength(500)]
        public string Name { get; set; }

        [Display(Name = "Event Date", Description = "Date of Event")]
        [Column("event_date")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Reminder", Description = "Reminder of Event")]
        [Column("reminder_date")]
        public DateTime ReminderDate { get; set; }

        [Column("budget_id")]
        [ForeignKey("Budget")]
        public int BudgetID { get; set; }

        public virtual Budget Budget { get; set; } 

        [ForeignKey("EventID")]
        [Display(Name="Shared With Friends", Description="Shared With Friends")]
        public virtual List<EventUser> SharedFriends { get; set; }
    }
}