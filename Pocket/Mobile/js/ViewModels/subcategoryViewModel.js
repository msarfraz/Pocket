define(['app', "Models/subcategoryModel"], function (app, Subcategory) {

    function SubcategoryVM(values) {
        values = values || {};
        this.base = Subcategory;
        this.base(values);

        this.BudgetDurationText = values['BudgetDurationText'] || '';
        this.RepeatTypes = [];
	}

	

    return SubcategoryVM;
});