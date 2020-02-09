using Pocket.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Pocket.Models
{
    [KnownType(typeof(User))]
    [Table("targets")]
    public class Target
    {
        public Target()
        {

        }
        [Required()]
        [Column("target_id", Order = 0)]
        [Key()]
        public int TargetID { get; set; }

        [Required()]
        [Column("target_amount")]
        public double TargetAmount { get; set; }

        [Required()]
        [Column("expected_date")]
        public DateTime ExpectedDate { get; set; }

        [Required()]
        [Column("target_date")]
        public DateTime TargetDate { get; set; }

        [Required()]
        [Display(Name = "Target Name", Description = "Name of Target")]
        [Column("target_name")]
        [StringLength(500)]
        public string Name { get; set; }

        [Required()]
        [Display(Name = "Status", Description = "Status of Target")]
        [Column("target_status")]
        public TargetStatus Status { get; set; }

        [Required()]
        [Column("user_id")]
        [ForeignKey("User")]
        public int UserID { get; set; }

        
        public virtual User User { get; set; }
    }
}