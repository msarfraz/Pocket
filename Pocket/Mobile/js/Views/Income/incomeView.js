define(['hbs!Views/Income/income'], function(viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    $('.income-page .page-content').html(viewTemplate({ model: params.model }));
		$('.income-header').text(params.state.isNew ? "New Income" : "Income");
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