define(['app'],function(app) {

	function Vendor(values) {
		values = values || {};
		this.VendorID = values['VendorID'] || 0 ;

		this.Name = values['Name'] || '';
	}

	Vendor.prototype.setValues = function (formInput) {
	    if (formInput) {
	        this.VendorID = formInput["VendorID"];
	        this.Name = formInput["VendorName"];
	    }
		    
	};

	Vendor.prototype.validate = function () {
		var result = true;
		if (!this.Name) {
			result = false;
		}
		return result;
	};

	return Vendor;
});