define(['hbs!Views/Event/event', 'hbs!Views/Event/event-readonly'], function (editTemplate, viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    if (params.model.Editable) {
	        $('.event-save-link').show();
	        $('.event-page .page-content').html(editTemplate({ model: params.model }));
	    }
	    else {
	        $('.event-save-link').hide();
	        $('.event-page .page-content').html(viewTemplate({ model: params.model }));
	    }
		$('.event-header').text(params.state.isNew ? "New Event" : "Event Details");
		bindEvents(params.bindings);
	}

	function bindEvents(bindings) {
		for (var i in bindings) {
			$(bindings[i].element).on(bindings[i].event, bindings[i].handler);
		}
	}

	return {
		render: render
	}
});