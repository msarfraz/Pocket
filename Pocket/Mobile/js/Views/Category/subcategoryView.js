define(['hbs!Views/Category/subcategory'], function(viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    $('.subcategory-page .page-content').html(viewTemplate({ vmodel: params.model }));
	    $('.subcategory-header').text(params.state.isNew ? "New Sub category" : "Sub category");
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