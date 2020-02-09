using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Table("notifications")]
    public class Notification
    {
        public Notification()
        {
            NotificationDate = DateTime.Now;
        }
        [Required()]
        [Column("notification_id", Order = 0)]
        [Key()]
        public int NotificationID { get; set; }

        [Required()]
        [Column("user_id", Order = 1)]
        [ForeignKey("User")]
        public int UserID { get; set; }
        public virtual User User { get; set; }

        [Required()]
        [Column("notification_text")]
        [StringLength(500)]
        public string Text { get; set; }

        [Required()]
        [Column("generated_by")]
        public int GeneratedBy { get; set; }


        [Required()]
        [Column("notification_date")]
        public DateTime NotificationDate { get; set; }

        [Required()]
        [Column("notification_url")]
        [StringLength(100)]
        public string URL { get; set; }
    }
}