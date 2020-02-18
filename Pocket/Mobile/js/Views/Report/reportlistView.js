define(['hbs!Views/Report/statement-list-item', 'hbs!Views/Report/expense-list-item', 'hbs!Views/Report/expense-filter', 'hbs!Views/Report/statement-filter'], function (statementTemplate, expenseTemplate, filterTemplate, statementFilterTemplate) {
	var $ = Framework7.$;

	function render(params) {
	    

	    $('.reports-list ul').html(getHTML(params.name, params.model));

	    
        
	}
	function renderFilter(params)
	{
	    if (params.name == "advancereport") {
	        $(".reports-list-header").text("Search");
	        if (params.filter)
	            $('.report-filter-content ul').html(filterTemplate({ model: params.filter }));
	    }
	    else if (params.name == "accountstatement") {
	        $(".reports-list-header").text("Statement");
	        if (params.filter)
	            $('.report-filter-content ul').html(statementFilterTemplate({ model: params.filter }));
	    }
	    else if (params.name == "eventreport") {
	        $(".reports-list-header").text("Event Expenses");
	    }
	    if (params.bindings)
	        bindEvents(params.bindings);
	}
	function addPage(name, model) {
	    $('.reports-list ul').append(getHTML(name, model));
	}
	function getHTML(templateName, model)
	{
	    if (templateName == "accountstatement") {
	        return statementTemplate(model);
	    }
	    else if (templateName == "eventreport" || templateName == "advancereport") {
	        return expenseTemplate(model);
	    }
	}
	function bindEvents(bindings) {
		for (var i in bindings) {
			$(bindings[i].element).on(bindings[i].event, bindings[i].handler);
		}
	}

	return {
	    render: render,
	    renderFilter:renderFilter,
	    addPage: addPage
	};
});