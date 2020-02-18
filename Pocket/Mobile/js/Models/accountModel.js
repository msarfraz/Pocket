define(['app'],function(app) {

	function Account(values) {
		values = values || {};
		this.AccountID = values['AccountID'] || 0 ;
		this.Name = values['Name'] || '';
		this.InitialAmount = values['InitialAmount'] || 0;
		this.CurrentAmount = values['CurrentAmount'] || '';
		this.AccountType = values['AccountType'] || '';
		
	}

	Account.prototype.setValues = function (formInput) {
	    if (formInput) {
	        this.AccountID = formInput["AccountID"];
	        this.Name = formInput["AccountName"];
	        this.InitialAmount = formInput['InitialAmount'] || '';
	        this.CurrentAmount = formInput['CurrentAmount'] || '';
	        this.AccountType = formInput['AccountType'] || '';
	    }
		    
	};

	Account.prototype.validate = function () {
		var result = true;
		if (!this.Name || !this.InitialAmount || !this.AccountType) {
			result = false;
		}
		return result;
	};

	return Account;
});