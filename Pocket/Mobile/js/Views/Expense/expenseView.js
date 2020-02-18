define(['hbs!Views/Expense/expense', 'hbs!Views/Expense/expense-readonly'], function (inputTemplate, viewTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    if (params.model.Editable)
	    {
	        $('.expense-page .page-content').html(inputTemplate({ model: params.model }));
	        $('.expense-save-link').show();
	    }
	    else
	    {
	        $('.expense-page .page-content').html(viewTemplate({ model: params.model }));
	        $('.expense-save-link').hide();

	    }
	    
		$('.expense-header').text(params.state.isNew ? "New Expense" : "Expense Details");
		bindEvents(params.bindings);
	}

	function bindEvents(bindings) {
		for (var i in bindings) {
			$(bindings[i].element).on(bindings[i].event, bindings[i].handler);
		}
	}

	return {
		render: render
	}
});