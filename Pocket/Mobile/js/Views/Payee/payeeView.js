define(['hbs!Views/Payee/payee'], function(viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    $('.payee-page .page-content').html(viewTemplate({ model: params.model }));
		$('.payees-header').text(params.state.isNew ? "New payee" : "Payee");
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