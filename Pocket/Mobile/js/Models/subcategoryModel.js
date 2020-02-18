define(['app'],function(app) {

	function Subcategory(values) {
		values = values || {};
		this.SubcategoryID = values['SubcategoryID'] || 0 ;
		this.Name = values['Name'] || '';
		this.BudgetAmount = values['Budget'] || '';
		this.BudgetDuration = values['BudgetDuration'] || 0;
		
		this.CategoryID = 0;
	}

	Subcategory.prototype.setValues = function (formInput) {
	    if (formInput) {
            this.CategoryID = formInput["SubcatCategoryID"]
	        this.SubcategoryID = formInput["SubcategoryID"];
	        this.Name = formInput["SubcategoryName"];
	        this.BudgetAmount = formInput['SubcategoryBudget'] || '';
	        this.BudgetDuration = formInput['SubcategoryBudgetDuration'] || '';
	    }
		    
	};

	Subcategory.prototype.validate = function () {
		var result = true;
		if (!this.Name || !this.BudgetDuration || !this.BudgetAmount) {
			result = false;
		}
		return result;
	};

	return Subcategory;
});