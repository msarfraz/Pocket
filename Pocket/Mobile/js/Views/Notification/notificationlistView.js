define(['hbs!Views/Notification/notification-list-item'], function(template) {
	var $ = Framework7.$;

	function render(params) {
		$('.notifications-list ul').html(template(params.model));
		bindEvents(params.bindings);
	}

	function bindEvents(bindings) {
		for (var i in bindings) {
			$(bindings[i].element).on(bindings[i].event, bindings[i].handler);
		}
	}
	function addPage(model) {
	    $('.notifications-list ul').append(template(model));
	}
	return {
	    render: render,
	    addPage: addPage
	};
});