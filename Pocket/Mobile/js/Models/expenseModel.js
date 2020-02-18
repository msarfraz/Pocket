define(['app'], function (app) {

    function Expense(values) {
        values = values || {};

        this.ExpenseID = values['ExpenseID'] || 0;
        this.AccountID = values['AccountID'] || '';
        
        this.Amount = values['Amount'] || 0;
        this.Description = values['Description'] || '';
        this.EventID = values['EventID'] || '';
        
        this.ExpenseDate = values['ExpenseDate'] || '';
        this.PayeeID = values['PayeeID'] || '';
        
        this.SubcategoryID = values['SubcategoryID'] || '';
        
        this.CategoryID = values['CategoryID'] || '0';
        
        this.VendorID = values['VendorID'] || '';
        
        this.Repeat = values['Repeat'] || '0';
        
    }

    Expense.prototype.setValues = function (formInput) {
        if (formInput) {
            this.ExpenseID = formInput['ExpenseID'];
            this.AccountID = formInput['ExpenseAccountID'];
            this.Amount = formInput['ExpenseAmount'];
            this.Description = formInput['ExpenseDescription'];
            this.EventID = formInput['ExpenseEventID'];
            this.ExpenseDate = formInput['ExpenseDate'];
            this.PayeeID = formInput['ExpensePayeeID'];
            this.SubcategoryID = formInput['ExpenseSubcategoryID'];
            this.VendorID = formInput['ExpenseVendorID'];
            this.Repeat = formInput['ExpenseRepeat'];
        }

    };

    Expense.prototype.validate = function () {
        var result = true;
        if (!this.AccountID || !this.Amount || !this.ExpenseDate || !this.SubcategoryID) {
            result = false;
        }
        return result;
    };

    return Expense;
});

