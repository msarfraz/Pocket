define(['app', "Models/accountModel"], function (app, Account) {

    function AccountVM(values) {
        values = values || {};

        this.base = Account;
        this.base(values);

        this.AccountTypeText = values['AccountTypeText'] || '';
        this.AccountTypes = [];
	}

	return AccountVM;
});