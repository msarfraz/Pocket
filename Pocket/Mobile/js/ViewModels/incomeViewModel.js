define(['app', "Models/incomeModel"], function (app, Income) {

    function IncomeVM(values) {
        values = values || {};

        this.base = Income;
        this.base(values);

        this.AccountText = values['AccountText'] || '';
        this.SourceText = values['SourceText'] || '';
        this.RepeatText = values['RepeatText'] || '';

		this.AccountGroups = [];
		this.Sources = [];
		this.RepeatTypes = [];
	}


	return IncomeVM;
});