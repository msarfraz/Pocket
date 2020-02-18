define(['hbs!Views/Transfer/transfer-list-item'], function(template) {
	var $ = Framework7.$;

	function render(params) {
		$('.transfers-list ul').html(template(params.model));
		bindEvents(params.bindings);
	}
	function addPage(model) {
	    $('.transfers-list ul').append(template(model));
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