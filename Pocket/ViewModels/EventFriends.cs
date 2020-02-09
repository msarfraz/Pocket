using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Pocket.Models;

namespace Pocket.ViewModels
{
    public class EventFriends
    {
        public EventFriends(QDbContext db, int UserID, int EventID)
       {
           User u = db.Users.Find(UserID);
           List<Friend> friends = u.Friends;
           List<EventUser> efriends = u.Events.Find(e=>e.EventID==EventID).SharedFriends;
           Friends = new List<User>();
           EFriends = new List<User>();
           foreach (EventUser eu in efriends)
           {
               EFriends.Add(eu.User);
           }
           foreach (Friend f in friends)
           {
               Friends.Add(f.UserFriend);
           }
           Friends = Friends.Except(EFriends).ToList();
           
            
 
            this.EventID = EventID;
        }
        public int EventID { get; set; }
        public List<User> EFriends { get; set; }
        public List<User> Friends { get; set; }

        public string FriendsString()
        {
            return string.Join(",", Friends.Select(f => f.FirstName + " " + f.LastName));
        }
        public string EFriendsString()
        {
            return string.Join(",", EFriends.Select(f => f.FirstName + " " + f.LastName));
        }

        public void SplitFriends(QDbContext db)
        {
            if (Friends==null)
            {
                Friends = new List<User>();
            }
            for(int i=0;i<Friends.Count;i++)
            {
                var friend = Friends[i];
                if (!EFriends.Contains(friend))
                {
                    Friends.Remove(friend);
                }
            }
            /*foreach (var friendId in SelectedFriendIDs)
            {
                Friend fr = db.Users.Find(UserID).Friends.Find(fri => fri.FriendID == friendId);
                Friends.Add(fr);
            }*/
        }


        internal void SetSelectedFriends(QDbContext db)
        {
            //foreach (var friend in Friends)
            //{
            //    //SelectedFriendIDs.Add(friend.FriendID);
            //}
        }
    }
}