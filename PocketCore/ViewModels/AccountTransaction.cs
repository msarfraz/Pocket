using Pocket.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pocket.ViewModels
{
    public class AccountTransaction
    {
        public int TransactionID { get; set; }
        public string Name { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public TransactionType Type { get; set; }
        public double Amount { get; set; }
        public double Balance { get; set; }
        public ObjectType ObjectType { get; set; }

        public double Withdrawl
        {
            get
            {
                return this.Type == TransactionType.Credit ? Amount : 0;
            }
        }
        public double Deposit
        {
            get
            {
                return this.Type == TransactionType.Debit ? Amount : 0;
            }
        }
    }
}