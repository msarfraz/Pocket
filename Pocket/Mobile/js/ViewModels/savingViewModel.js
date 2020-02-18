define(['app', "Models/savingModel"], function (app, Saving) {

    function SavingVM(values) {
        values = values || {};

        this.base = Saving;
        this.base(values);

        this.SavingDateText = values['SavingDateText'] || '';
        this.AccountText = values['AccountText'] || '';
        this.TargetText = values['TargetText'] || '';

        this.AccountGroups = [];
        this.Targets = [];
	}

	
	return SavingVM;
});