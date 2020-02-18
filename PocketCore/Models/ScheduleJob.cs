using Pocket.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Table("schedule_jobs")]
    public class ScheduleJob
    {
        [Required()]
        [Column("schdule_job_id", Order = 0)]
        [Key()]
        public int ScheduleJobID { get; set; }

        [Required()]
        [Column("user_id")]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required()]
        [Column("status")]
        public ScheduleStatus Status { get; set; }

        [Required()]
        [Column("date_created")]
        public DateTime CreatedDate { get; set; }

        [Required()]
        [Column("jobs_processed")]
        public int JobsProcessed { get; set; }
    }
}