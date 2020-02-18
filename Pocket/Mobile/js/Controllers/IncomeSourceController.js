define(["app", "Views/IncomeSource/incomesourceView", "Views/IncomeSource/incomesourcelistView", "Models/incomesourceModel", "pocketdataaccess"], function (app, IncomeSourceView, IncomeSourceListView, IncomeSource, Repo) {
    
	var state = {isNew: false};
	var incomesource = null;
	var $$ = Framework7.$;
	var bindings = [{
		element: '.incomesource-save-link',
		event: 'click',
		handler: saveIncomeSource
	}];
	var listbindings = [{
	    element: '.source-delete-link',
	    event: 'click',
	    handler: deleteSource
	}, {
	    element: '.source-edit-link',
	    event: 'click',
	    handler: editSource
	}];
	function init(query){
		if (query && query.id) {
		    var incomesources = Repo.GetIncomeSources(); // JSON.parse(localStorage.getItem("f7Base"));
			for (var i = 0; i< incomesources.length; i++) {
				if (incomesources[i].SourceID == query.id) {
				    incomesource = new IncomeSource(incomesources[i]);
					state.isNew = false;
					break;
				}
			}
		}
		else {
		    incomesource = new IncomeSource();
			state.isNew = true;
		}
		IncomeSourceView.render({
		    model: incomesource,
			state: state,
			bindings: bindings
		});
	}
	function initlist(query) {
	    incomesources = Repo.GetIncomeSources();
	    IncomeSourceListView.render({ model: incomesources, bindings: listbindings });
	}
	function deleteSource(e, t) {
	    var id = $$(e.currentTarget).data('id');
	    var data = { SourceID: id };
	    var classid = ".source-swipeout-" + id;

	    Repo.DeleteIncomeSource(data, function (result) {
	        if (result.success) {
	            app.f7.swipeoutDelete(classid);
	        }
	        else {
	            app.f7.alert(result.message);
	        }
	    });
	}
	function editSource(e, t) {
	    var url = $$(e.currentTarget).data('url');

	    app.mainView.router.loadPage(url);
	}

	function saveIncomeSource() {
	    var formInput = app.f7.formToJSON('#incomesourceEdit');

	    incomesource.setValues(formInput);
	    if (!incomesource.validate()) {
	        app.f7.alert("Please fill all mandatory fields.");
			return;
		}
	    Repo.SaveIncomeSource(incomesource, function (result) {
	        if (result.success) {
	            app.ReloadPreviousPage();
	        }
	        else
	            app.f7.alert(result.message);
		    
		});
		
	}

	return {
	    init: init,
        initlist:initlist
	};
});