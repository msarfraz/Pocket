define(['hbs!Views/Saving/saving-list-item'], function(template) {
	var $ = Framework7.$;

	function render(params) {
		$('.savings-list ul').html(template(params.model));
		bindEvents(params.bindings);
	}

	function bindEvents(bindings) {
		for (var i in bindings) {
			$(bindings[i].element).on(bindings[i].event, bindings[i].handler);
		}
	}
	function addPage(model) {
	    $('.savings-list ul').append(template(model));
	}
	return {
	    render: render,
	    addPage: addPage
	};
});