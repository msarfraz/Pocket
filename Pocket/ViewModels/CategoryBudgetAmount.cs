using Pocket.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pocket.ViewModels
{
    public interface IBudgetAmount
    {
        string Name { get; set; }
        double Budget { get; set; }
        double Amount { get; set; }
        
    }
    public class CategoryBudgetAmount : IBudgetAmount
    {
        public CategoryBudgetAmount()
        {
            Subcategories = new List<SubcategoryBudgetAmount>();
        }
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public DisplaySetting Display { get; set; }
        public double Budget { get; set; }
        public double Amount { get; set; }
        public bool Shared { get; set; }
        public bool Editable { get; set; }
        public string UserName { get; set; }

        public List<SubcategoryBudgetAmount> Subcategories { get; set; }
        public string BugetStatus { get
            {
                return Util.GetBudgetStatus(Budget, Amount);
            }
        }
        public int BudgetPercentage
        {
            get{
                var diff = Budget - Amount;
                return (int)((diff / (Budget == 0 ? 1 : Budget)) * 100);
            }
        }
        public void Calculate()
        {
            Budget = 0;
            Amount = 0;
            foreach (var scat in Subcategories)
            {
                Budget += scat.Budget;
                Amount += scat.Amount;
            }
        }
    }
    public class SubcategoryBudgetAmount : IBudgetAmount
    {
        public int SubcategoryID { get; set; }
        public string Name { get; set; }
        public double Budget { get; set; }
        public double Amount { get; set; }
        public RepeatPattern BudgetDuration { get; set; }
        public string BugetStatus
        {
            get
            {
                return Util.GetBudgetStatus(Budget, Amount);
            }
        }
        public int BudgetPercentage
        {
            get
            {
                var diff = Budget - Amount;
                return (int)((diff / (Budget == 0 ? 1 : Budget)) * 100);
            }
        }
    }
    public class EventBudgetAmount : IBudgetAmount
    {
        public string Name { get; set; }
        public double Budget { get; set; }
        public double Amount { get; set; }
        public string BugetStatus
        {
            get
            {
                return Util.GetBudgetStatus(Budget, Amount);
            }
        }
        public int BudgetPercentage
        {
            get
            {
                var diff = Budget - Amount;
                return (int)((diff / (Budget == 0 ? 1 : Budget)) * 100);
            }
        }
    }
    public class TargetBudgetAmount : IBudgetAmount
    {
        public string Name { get; set; }
        public double Budget { get; set; }
        public double Amount { get; set; }
        public string BugetStatus
        {
            get
            {
                return Util.GetBudgetStatus(Budget, Amount);
            }
        }
        public int BudgetPercentage
        {
            get
            {
                var diff = Budget - Amount;
                return (int)((diff / (Budget == 0 ? 1 : Budget)) * 100);
            }
        }
    }
}