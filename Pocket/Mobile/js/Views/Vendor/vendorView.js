define(['hbs!Views/Vendor/vendor'], function(viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    $('.vendor-page .page-content').html(viewTemplate({ model: params.model }));
		$('.vendors-header').text(params.state.isNew ? "New Vendor" : "Vendor");
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