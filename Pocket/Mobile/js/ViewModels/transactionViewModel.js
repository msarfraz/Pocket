define(['app'], function (app) {

    function TransferVM(values) {
        values = values || {};

        TransactionID = values['TransactionID'] || '';
        TransactionDate = values['TransactionDate'] || '';
        Description = values['Description'] || '';
        Name = values['Name'] || '';
        Withdrawl = values['Withdrawl'] || '';
        Deposit = values['Deposit'] || '';
        Balance = values['Balance'] || '';

	}


	return TransferVM;
});