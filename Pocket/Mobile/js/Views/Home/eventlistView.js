define(['hbs!Views/Home/event-list-item'], function(template) {
	var $ = Framework7.$;

	function render(params) {
	    if (params.model.length > 0)
	        $('.upcoming-events-list ul').html(template(params.model));
	    else
	        $('.upcoming-events-list ul').html("<a href='Pages/Event.html' class='item-link item-content'>No record to display.</a>");
	}

	return {
		render: render
	};
});