define(['app'],function(app) {

	function Event(values) {
		values = values || {};
		this.EventID = values['EventID'] || 0 ;
		this.Name = values['Name'] || '';
		this.EventDate = values['EventDate'] || '';
		this.Amount = values['Amount'] || 0;
		this.BudgetAmount = values['BudgetAmount'] || 0;
		this.BudgetDuration = values['BudgetDuration'] || 0;
		this.Budgeted = values['Budgeted'] || 0;
		this.EventStatus = values['EventStatus'] || 1;
	}

	Event.prototype.setValues = function (formInput) {
	    if (formInput) {
	        this.EventID = formInput['EventID'] || 0;
	        this.Name = formInput['EventName'] || '';
	        this.EventDate = formInput['EventDate'] || '';
	        this.Amount = formInput['EventAmount'] || 0;
	        this.BudgetAmount = formInput['EventBudgetAmount'] || 0;
	        this.BudgetDuration = formInput['EventBudgetDuration'] || 0;
	        this.Budgeted = formInput['EventBudgeted'] || 1;
	        this.EventStatus = formInput['EventStatus'] || 1;
	    }
		    
	};

	Event.prototype.validate = function () {
		var result = true;
		if (!this.Name || !this.EventDate || !this.BudgetAmount) {
			result = false;
		}
		return result;
	};

	return Event;
});