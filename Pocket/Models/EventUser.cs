using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Pocket.Models
{
    [Table("eventuser")]
    public class EventUser
    {
        [Required()]
        [Key, Column("event_id", Order = 1)]
        public int EventID { get; set; }

        [Required()]
        [Key, Column("user_id", Order = 0)]
        public int UserID { get; set; }

        public virtual User User { get; set; }
        public virtual Event Event { get; set; }
        
    }
}