define(["app", "Views/ExComment/excommentlistView", "pocketdataaccess"], function (app, ExcommentListView, Repo) {
    
    var $$ = Framework7.$;

    var listquery;
    var bindings = [{
		element: '.comment-save-link',
		event: 'click',
		handler: saveComment
	}];

    function initlist(query) {
        if (query.expenseid) {
            listquery = query;
            var vendors = Repo.GetExpenseComments({ ExpenseID: query.expenseid }, function (response)
            {
                ExcommentListView.render({ model: response.rows, bindings: bindings });
            }

            );
        }
	}
    function saveComment() {
        var comment = $$(".comment-message").val();
        Repo.SaveComment({ Comment: comment, ExpenseID: listquery.expenseid }, function (result) {
	        if (result.success) {
	            initlist(listquery);
	        }
	        else
	            app.f7.alert(result.message);
		    
		});
		
	}

	return {
        initlist:initlist
	};
});