define(['app'],function(app) {

	function Contact(values) {
		values = values || {};
		this.PayeeID = values['PayeeID'] || 0 ;

		this.Name = values['Name'] || '';
	}

	Contact.prototype.setValues = function(formInput) {
		for(var field in formInput){
			if (this[field] !== undefined) {
				this[field] = formInput[field];
			}
		}
	};

	Contact.prototype.validate = function() {
		var result = true;
		if (!this.Name) {
			result = false;
		}
		return result;
	};

	return Contact;
});