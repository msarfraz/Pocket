using Pocket.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pocket.Models
{
    [Table("friends")]
    public class Friend
    {
        [Required()]
        [Key, Column("friend_id", Order = 1)]
        public string FriendID { get; set; }

        [Required()]
        [Key, Column("user_id", Order = 0)]
        public string UserID { get; set; }
        
        public FriendStatus Status { get; set; }

        
        //public virtual ApplicationUser User { get; set; }
        public virtual ApplicationUser UserFriend { get; set; }
        //public virtual ICollection<User> UserFriends { get; set; }
        //public virtual List<Event> Events { get; set; }

    }
}