define(['app'], function (app) {

	function Category(values) {
	    values = values || {};
	   

		this.CategoryID = values['CategoryID'] || 0 ;
		this.Name = values['Name'] || '';
	//	this.Display = values['Display'] || '';
		this.Budget = values['Budget'] || '';
	}

	Category.prototype.setValues = function (formInput) {
	    if (formInput) {
	        this.CategoryID = formInput["CategoryID"];
	        this.Name = formInput["CategoryName"];
	      //  this.Display = formInput['CategoryDisplay'][0] == 'on' ? 1 : 0 ; // switch data is stored in an array
	        this.Budget = formInput['CategoryBudget'] || '';
	    }
		    
	};

	Category.prototype.validate = function () {
		var result = true;
		if (!this.Name) {
			result = false;
		}
		return result;
	};

	return Category;
});