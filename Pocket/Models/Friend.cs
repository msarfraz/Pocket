using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pocket.Common;

namespace Pocket.Models
{
    [Table("friends")]
    public class Friend
    {
        [Required()]
        [Key, Column("friend_id", Order = 1)]
        public int FriendID { get; set; }

        [Required()]
        [Key, Column("user_id", Order = 0)]
        public int UserID { get; set; }
        
        public FriendStatus Status { get; set; }

        
        //public virtual User User { get; set; }
        public virtual User UserFriend { get; set; }
        //public virtual ICollection<User> UserFriends { get; set; }
        //public virtual List<Event> Events { get; set; }

    }
}