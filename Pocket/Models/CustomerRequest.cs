using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Pocket.Common;
using System;

namespace Pocket.Models
{
    [Table("customer_requests")]
    public class CustomerRequest
    {
        [Required()]
        [Column("customer_request_id", Order = 0)]
        [Key()]
        public int CustomerRequestID { get; set; }

        [Column("user_id")]
        public string UserID { get; set; }
        public virtual ApplicationUser User { get; set; }

        [Required()]
        [Column("date_created")]
        public DateTime CreatedDate { get; set; }

        [Required()]
        [Column("request_type")]
        public CustomerRequestType RequestType { get; set; }

        [Column("customer_name")]
        public string Name { get; set; }

        [Column("customer_email")]
        public string Email { get; set; }

        [Column("request_title")]
        public string Title { get; set; }

        [Column("request_body")]
        public string Body { get; set; }


    }
}