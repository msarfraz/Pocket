using Newtonsoft.Json;
using Pocket.Common;
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
        public string UserID { get; set; }

        [JsonIgnoreAttribute]
        public virtual ApplicationUser User { get; set; }

        [Required()]
        [Display(Name = "Event Name", Description = "Name of Event")]
        [Column("event_name")]
        [StringLength(500)]
        public string Name { get; set; }

        [Required()]
        [Display(Name = "Event Date", Description = "Date of Event")]
        [Column("event_date")]
        public DateTime EventDate { get; set; }

        [Required()]
        [Column("amount")]
        public double Amount { get; set; }

        [Required()]
        [Display(Name = "Reminder", Description = "Reminder of Event")]
        [Column("reminder_date")]
        public DateTime ReminderDate { get; set; }

        [Required()]
        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

        [Required()]
        [Column("budget_id")]
        [ForeignKey("Budget")]
        public int BudgetID { get; set; }

        [JsonIgnoreAttribute]
        public virtual Budget Budget { get; set; }

        [Required()]
        [Column("event_status")]
        public EventStatus EventStatus { get; set; }

        [JsonIgnoreAttribute]
        [ForeignKey("EventID")]
        [Display(Name="Shared With Friends", Description="Shared With Friends")]
        public virtual List<EventUser> SharedFriends { get; set; }

        [Required()]
        [Column("budgeted")]
        public YesNoOptions Budgeted { get; set; }
    }
}