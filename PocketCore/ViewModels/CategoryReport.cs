using Pocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Pocket.ViewModels
{
    public class CategoryExpense
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public double Budget { get; set; }
        public double Amount { get; set; }
        public List<SubcategoryExpense> Subcategories { get; set; }

        public CategoryExpense()
        {
            Subcategories = new List<SubcategoryExpense>();
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

    public class SubcategoryExpense
    {
        public int SubcategoryID { get; set; }
        public string Name { get; set; }
        public double Budget { get; set; }
        public double Amount { get; set; }

        public List<Expense> Expenses { get; set; }
        public SubcategoryExpense()
        {
            Expenses = new List<Expense>();
        }
    }
}