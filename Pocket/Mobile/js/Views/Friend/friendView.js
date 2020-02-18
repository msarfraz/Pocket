define(['hbs!Views/Friend/friend'], function(viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    $('.friend-page .page-content').html(viewTemplate({ model: params.model }));
		$('.friends-header').text(params.state.isNew ? "New Friend" : "Friend");
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