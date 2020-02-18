define(['hbs!Views/Home/budget'], function (template) {
	var $ = Framework7.$;
	//Handlebars.helpers = Template7.helpers;

	function render(params) {
	    $('.budget-list ul').html(template({ model: params.model }));
	    bindEvents(params.bindings);
	}
	function refresh(params) {
	    $('.budget-list ul').html(template({ model: params.model }));
	}

	function bindEvents(bindings) {
	    for (var i in bindings) {
	        $(bindings[i].element).on(bindings[i].event, bindings[i].handler);
	    }
	}

	return {
	    render: render,
	    refresh: refresh
	};
});