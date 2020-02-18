define(['hbs!Views/Target/target'], function(viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    $('.target-page .page-content').html(viewTemplate({ model: params.model }));
		$('.targets-header').text(params.state.isNew ? "New Target" : "Target");
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