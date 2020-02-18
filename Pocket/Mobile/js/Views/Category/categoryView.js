define(['hbs!Views/Category/category'], function(viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    $('.category-page .page-content').html(viewTemplate({ model: params.model }));
		$('.category-header').text(params.state.isNew ? "New Category" : "Category");
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