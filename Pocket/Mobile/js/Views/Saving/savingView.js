define(['hbs!Views/Saving/saving'], function(viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    $('.saving-page .page-content').html(viewTemplate({ model: params.model }));
		$('.savings-header').text(params.state.isNew ? "New Saving" : "Saving");
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