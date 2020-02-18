define(['app', "Models/eventModel"], function (app, Event) {

    function EventVM(values) {
        values = values || {};

        this.base = Event;
        this.base(values);

		this.BudgetDurationText = values['BudgetDurationText'] || 'None';
		this.EventStatusText = values['EventStatusText'] || '';
		this.BudgetedText = values['BudgetedText'] || 'No';
		this.Editable = values['Editable'] ;
		this.RepeatTypes = [];
	}

	
	return EventVM;
});