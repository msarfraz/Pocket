define(['app'],function(app) {

	function Friend(values) {
		values = values || {};
		this.UserID = values['UserID'] || 0;

		this.UserName = values['UserName'] || '';
		this.Email = values['Email'] || '';
		this.Status = values['Status'] || '';
	}

	Friend.prototype.setValues = function (formInput) {
	    if (formInput) {
	        this.UserID = formInput["FriendUserID"];
	        this.Email = formInput["UserEmail"];
	    }
		    
	};

	Friend.prototype.validate = function () {
		var result = true;
		if (!this.Email) {
			result = false;
		}
		return result;
	};

	return Friend;
});