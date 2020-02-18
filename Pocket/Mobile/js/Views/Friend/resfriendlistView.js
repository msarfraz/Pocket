define(['hbs!Views/Friend/resfriend-list-item'], function(template) {
	var $ = Framework7.$;

	function render(params) {
		$('.resfriends-list ul').html(template(params.model));
		bindEvents(params.bindings);
	}
	function addPage(model) {
	    $('.resfriends-list ul').append(template(model));
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