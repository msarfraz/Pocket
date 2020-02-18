define(['app'],function(app) {

	function IncomeSource(values) {
		values = values || {};
		this.SourceID = values['SourceID'] || 0 ;

		this.Name = values['Name'] || '';
	}

	IncomeSource.prototype.setValues = function (formInput) {
	    if (formInput) {
	        this.SourceID = formInput["SourceID"];
	        this.Name = formInput["IncomeSourceName"];
	    }
	    
	};

	IncomeSource.prototype.validate = function () {
		var result = true;
		if (!this.Name) {
			result = false;
		}
		return result;
	};

	return IncomeSource;
});