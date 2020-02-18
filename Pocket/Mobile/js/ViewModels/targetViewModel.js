define(['app', "Models/targetModel"], function (app, Target) {

    function TargetVM(values) {
        values = values || {};

        this.base = Target;
        this.base(values);

        this.TargetDateText = values['TargetDateText'] || '';
        this.StatusText = values['StatusText'] || 'Active';
        this.CreatedDateText = values['CreatedDateText'] || '';

        this.TargetStatuses = [];
	}

	
	return TargetVM;
});