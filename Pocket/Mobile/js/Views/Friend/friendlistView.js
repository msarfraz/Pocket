define(['hbs!Views/Friend/friend-list-item', 'hbs!Views/Friend/frequest-list-item'], function (ftemplate, frtemplate) {
	var $ = Framework7.$;

	function render(params) {
	    $('.friends-list ul').html(ftemplate(params.model));
		bindEvents(params.bindings);
	}
	function renderFRequests(params)
	{
	    $('.friends-request-list ul').html(frtemplate(params.model));
	    bindEvents(params.bindings);
	}
	function addPage(model) {
	    $('.friends-list ul').append(template(model));
	}
	
	function bindEvents(bindings) {
		for (var i in bindings) {
			$(bindings[i].element).on(bindings[i].event, bindings[i].handler);
		}
	}

	return {
	    render: render,
	    renderFRequests: renderFRequests
	};
});