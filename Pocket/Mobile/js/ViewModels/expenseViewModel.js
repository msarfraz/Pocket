define(['app', "Models/expenseModel"], function (app, Expense) {

    function ExpenseVM(values) {
        values = values || {};

        this.base = Expense;
        this.base(values);

        this.AccountText = values['AccountText'];
        this.ExpenseDateText = values['ExpenseDateText'];
        this.EventText = values['EventText'];
        this.PayeeText = values['PayeeText'];
        this.SubcategoryText = values['SubcategoryText'];
        this.CategoryText = values['CategoryText'];
        this.VendorText = values['VendorText'];
        this.RepeatText = values['RepeatText'];
        this.Comments = values['Comments'];
        this.Editable = values['Editable'];

		this.AccountGroups = [];
		this.CategoryGroups = [];
		this.EventGroups = [];
		this.Subcategories = [];
		this.RepeatTypes = [];
		this.Payees = [];
		this.Vendors = [];
	}

	return ExpenseVM;
});