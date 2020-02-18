define(["app", "Views/Payee/payeeView", "Views/Payee/payeelistView", "Models/payeeModel", "pocketdataaccess"], function (app, PayeeView, PayeeListView, Payee, Repo) {
    
	var state = {isNew: false};
	var payee = null;
	var $$ = Framework7.$;

	var bindings = [{
		element: '.payee-save-link',
		event: 'click',
		handler: savePayee
	}];
	var listbindings = [{
	    element: '.payee-delete-link',
	    event: 'click',
	    handler: deletePayee
	}, {
	    element: '.payee-edit-link',
	    event: 'click',
	    handler: editPayee
	}];
	function init(query){
		if (query && query.id) {
		    var payees = Repo.GetPayees(); // JSON.parse(localStorage.getItem("f7Base"));
			for (var i = 0; i< payees.length; i++) {
				if (payees[i].PayeeID == query.id) {
				    payee = new Payee(payees[i]);
					state.isNew = false;
					break;
				}
			}
		}
		else {
		    payee = new Payee();
			state.isNew = true;
		}
		PayeeView.render({
		    model: payee,
			state: state,
			bindings: bindings
		});
	}
	function initlist(query) {
	    payees = Repo.GetPayees();
	    PayeeListView.render({ model: payees, bindings: listbindings });
	}
	function deletePayee(e, t) {
	    var id = $$(e.currentTarget).data('id');
	    var data = { PayeeID: id };
	    var classid = ".payee-swipeout-" + id;

	    Repo.DeletePayee(data, function (result) {
	        if (result.success) {
	            app.f7.swipeoutDelete(classid);
	        }
	        else {
	            app.f7.alert(result.message);
	        }
	    });
	}
	function editPayee(e, t) {
	    var url = $$(e.currentTarget).data('url');

	    app.mainView.router.loadPage(url);
	}
	function savePayee() {
	    var formInput = app.f7.formToJSON('#payeeEdit');

	    payee.setValues(formInput);
	    if (!payee.validate()) {
			app.f7.alert("Please fill all mandatory fields.");
			return;
		}
	    Repo.SavePayee(payee, function (result) {
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