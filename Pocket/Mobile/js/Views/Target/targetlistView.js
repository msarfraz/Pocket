define(['hbs!Views/Target/target-list-item'], function(template) {
	var $ = Framework7.$;

	function render(params) {
		$('.targets-list ul').html(template(params.model));
		bindEvents(params.bindings);
	}

	function bindEvents(bindings) {
		for (var i in bindings) {
			$(bindings[i].element).on(bindings[i].event, bindings[i].handler);
		}
	}
	function addPage(model) {
	    $('.targets-list ul').append(template(model));
	}
	return {
	    render: render,
	    addPage: addPage
	};
});