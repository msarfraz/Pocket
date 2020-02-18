define(['app'],function(app) {

	function Payee(values) {
		values = values || {};
		this.PayeeID = values['PayeeID'] || 0 ;

		this.Name = values['Name'] || '';
	}

	Payee.prototype.setValues = function (formInput) {
	    if (formInput) {
	        for (var field in formInput) {
	            if (this[field] !== undefined) {
	                this[field] = formInput[field];
	            }
	        }
	    }
		
	};

	Payee.prototype.validate = function () {
		var result = true;
		if (!this.Name) {
			result = false;
		}
		return result;
	};

	return Payee;
});