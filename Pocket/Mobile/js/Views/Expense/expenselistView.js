define(['hbs!Views/Expense/expense-list-item'], function(template) {
	var $ = Framework7.$;

	function render(params) {
		$('.expenses-list ul').html(template(params.model));
		bindEvents(params.bindings);
	}
	function addPage(model)
	{
	    $('.expenses-list ul').append(template(model));
	}
	function renderHtml(model) {
	    return template(model);
	}

	function bindEvents(bindings) {
		for (var i in bindings) {
			$(bindings[i].element).on(bindings[i].event, bindings[i].handler);
		}
	}

	return {
	    render: render,
	    renderHtml: renderHtml,
	    addPage: addPage
	};
});