using System.Collections.Generic;

namespace Pocket.ViewModels
{
    public class FriendResource : IEqualityComparer<FriendResource> 
    {
        public string FriendID { get; set; }
        public string FriendName { get; set; }
        public bool Shared { get; set; }
    
        public bool Equals(FriendResource x, FriendResource y)
        {
 	        return x.FriendID == y.FriendID;
        }

        public int GetHashCode(FriendResource obj)
        {
            return obj.FriendID.GetHashCode();
        }
}
}