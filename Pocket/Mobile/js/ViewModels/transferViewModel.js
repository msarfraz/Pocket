define(['app', 'Models/transferModel'], function (app, Transfer) {

    function TransferVM(values) {
        values = values || {};
        this.base = Transfer;
        this.base(values);

        this.SourceAccountText = values['SourceAccountText'] || '';
		this.SourceAccounts = [];
		this.TargetAccountText = values['TargetAccountText'] || '';
		this.TargetAccounts = [];

	}


	return TransferVM;
});