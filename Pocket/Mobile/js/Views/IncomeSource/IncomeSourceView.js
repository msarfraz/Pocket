define(['hbs!Views/IncomeSource/incomesource'], function(viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    $('.incomesource-page .page-content').html(viewTemplate({ model: params.model }));
		$('.incomesource-header').text(params.state.isNew ? "New Income Source" : "Income Source");
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