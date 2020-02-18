define(['hbs!Views/Home/target-list-item'], function(template) {
	var $ = Framework7.$;

	function render(params) {
        if(params.model.length > 0)
            $('.upcoming-targets-list ul').html(template(params.model));
        else
            $('.upcoming-targets-list ul').html("<a href='Pages/Target.html' class='item-link item-content'>No record to display.</a>");
	}

	
	return {
		render: render
	};
});