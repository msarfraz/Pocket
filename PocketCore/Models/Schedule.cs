using Pocket.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pocket.Models
{

    [Table("schedules")]
    public class Schedule
    {
        [Required()]
        [Column("schdule_id", Order = 0)]
        [Key()]
        public int ScheduleID { get; set; }

        [Required()]
        [Column("user_id")]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Column("schedule_name")]
        public string Name { get; set; }

        [Required()]
        [Column("last_run")]
        public DateTime LastRunDate { get; set; }

        [Required()]
        [Column("next_run")]
        public DateTime NextRunDate { get; set; }

        [Required()]
        [Column("status")]
        public ScheduleStatus Status { get; set; }

        [Required()]
        [Column("date_created")]
        public DateTime CreateDate { get; set; }

        [Required()]
        [Column("schedule_type")]
        public ScheduleType Type { get; set; }
        
    }
}