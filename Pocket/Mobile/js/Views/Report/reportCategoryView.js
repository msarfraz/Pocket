define(['hbs!Views/Report/category-list-item'], function (catTemplate) {
	var $ = Framework7.$;

	function render(params) {

	    $('.reports-category ul').html(catTemplate(params.model));

	    bindEvents(params.bindings);
	}
	function bindEvents(bindings) {
		for (var i in bindings) {
			$(bindings[i].element).on(bindings[i].event, bindings[i].handler);
		}
	}

	return {
	    render: render
	};
});