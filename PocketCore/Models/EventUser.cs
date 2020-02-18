using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string UserID { get; set; }

        public virtual ApplicationUser User { get; set; }
        public virtual Event Event { get; set; }
        
    }
}