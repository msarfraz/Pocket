using Pocket.Common;
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
            NotificationStatus = Common.NotificationStatus.Unread;
        }
        [Required()]
        [Column("notification_id", Order = 0)]
        [Key()]
        public int NotificationID { get; set; }

        [Required()]
        [Column("user_id", Order = 1)]
        [ForeignKey("User")]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required()]
        [Column("notification_title")]
        [StringLength(50)]
        public string Title { get; set; }

        [Required()]
        [Column("notification_text")]
        [StringLength(500)]
        public string Text { get; set; }

        [Required()]
        [Column("generated_by")]
        public string GeneratedBy { get; set; }


        [Required()]
        [Column("notification_date")]
        public DateTime NotificationDate { get; set; }

        [Required()]
        [Column("notification_url")]
        [StringLength(100)]
        public string URL { get; set; }

        [Required()]
        [Column("notification_mobile_url")]
        [StringLength(100)]
        public string MobileURL { get; set; }

        [Required()]
        [Column("notification_status")]
        public NotificationStatus NotificationStatus { get; set; }

    }
}