define(['hbs!Views/Account/account'], function(viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
		$('.account-page .page-content').html(viewTemplate({ model: params.model }));
		$('.account-header').text(params.state.isNew ? "New Account" : "Account");
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