define(['hbs!Views/Income/income-list-item'], function(template) {
	var $ = Framework7.$;

	function render(params) {
		$('.incomes-list ul').html(template(params.model));
		bindEvents(params.bindings);
	}
	function addPage(model) {
	    $('.incomes-list ul').append(template(model));
	}
	function bindEvents(bindings) {
		for (var i in bindings) {
			$(bindings[i].element).on(bindings[i].event, bindings[i].handler);
		}
	}

	return {
	    render: render,
	    addPage: addPage
	};
});