define(['app'],function(app) {

	function Saving(values) {
	    values = values || {};

	    this.SavingID = values['SavingID'] || 0;
	    this.Amount = values['Amount'] || 0;
	    this.TargetID = values['TargetID'] || 0;
	    this.AccountID = values['AccountID'] || 0;
	    this.SavingDate = values['SavingDate'] || '';
	    this.Description = values['Description'] || '';
	}

	Saving.prototype.setValues = function (formInput) {
	    if (formInput) {
	        this.SavingID = formInput['SavingID'] || 0;
	        this.Amount = formInput['SavingAmount'] || 0;
	        this.TargetID = formInput['SavingTargetID'] || 0;
	        this.AccountID = formInput['SavingAccountID'] || 0;
	        this.SavingDate = formInput['SavingDate'] || '';
	        this.Description = formInput['SavingDescription'] || '';
	    }
		    
	};

	Saving.prototype.validate = function () {
		var result = true;
		if (!this.TargetID || !this.AccountID || !this.SavingDate || !this.Amount) {
			result = false;
		}
		return result;
	};

	return Saving;
});