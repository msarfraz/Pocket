define(['hbs!Views/Transfer/transfer'], function(viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    $('.transfer-page .page-content').html(viewTemplate({ model: params.model }));
		$('.transfer-header').text(params.state.isNew ? "New Transfer" : "Transfer");
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