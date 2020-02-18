define(['app'],function(app) {

	function Target(values) {
	    values = values || {};

	    this.TargetID = values['TargetID'] || 0;
	    this.Name = values['Name'] || '';
	    this.TargetAmount = values['TargetAmount'] || 0;
	    this.InitialAmount = values['InitialAmount'] || 0;
	    this.Status = values['Status'] || 0;
	    this.TargetDate = values['TargetDate'] || '';
	    
	    this.SavingAmount = values['SavingAmount'] || 0;
	}

	Target.prototype.setValues = function (formInput) {
	    if (formInput) {
	        this.TargetID = formInput['TargetID'] || 0;
	        this.Name = formInput['TargetName'] || '';
	        this.TargetAmount = formInput['TargetAmount'] || 0;
	        this.InitialAmount = formInput['TargetInitialAmount'] || 0;
	        this.Status = formInput['TargetStatus'] || 0;
	        this.TargetDate = formInput['TargetDate'] || '';
	        
	    }
		    
	};

	Target.prototype.validate = function () {
		var result = true;
		if (!this.Name || !this.TargetAmount || !this.TargetDate) {
			result = false;
		}
		return result;
	};

	return Target;
});