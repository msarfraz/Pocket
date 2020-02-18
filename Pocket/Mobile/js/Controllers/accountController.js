define(["app", "Views/Account/accountView", "Views/Account/accountlistView", "Models/accountModel", "ViewModels/accountViewModel", "pocketdataaccess"], function (app, AccountView, AccountListView, Account,AccountVM, Repo) {
    
	var state = {isNew: false};
	var account = new Account();
	var accountvm = null;

	var myApp = app.f7;
	var $$ = Framework7.$;
    /*
	$$(".accounts-page-content").on("refresh", function (e) {
	    initlist();
	    
	});
    */
	var bindings = [{
		element: '.account-save-link',
		event: 'click',
		handler: saveAccount
	}];

	var listbindings = [{
	    element: '.account-delete-link',
	    event: 'click',
	    handler: DeleteAccount
	}];
	var AccountTypes = [{ value: 1, text: 'Debit' }, { value: 2, text: 'Credit' }, { value: 3, text: 'Saving' }];

	function init(query) {
		if (query && query.id) {
		    var accounts = Repo.GetAccountByID(query.id, function (response) {
		        accountvm = new AccountVM(response.rows[0]);
		        state.isNew = false;
		        showAccount();
		    });
		}
		else {
		    accountvm = new AccountVM();

		    state.isNew = true;
		    showAccount();
		}
	}
	function showAccount()
	{
	    accountvm.AccountTypes = Repo.GetAccountTypes();
	    
	    AccountView.render({
	        model: accountvm,
	        state: state,
	        bindings: bindings
	    });
	    $$(".account-type").val(accountvm.AccountType);
	    var kp = myApp.keypad({
	        input: '#InitialAmount',
	        type: 'numpad',
	        toolbar: true
	    });
	}
	function initlist(query) {

	    var accounts = Repo.GetAccounts(function (response) {
	        AccountListView.render({ model: response.rows, bindings: listbindings });
	        
	    });
	}
	function DeleteAccount(e, t) {
	    var id = $$(e.currentTarget).data('id');
	    var data = { AccountID: id };
	    var classid = ".account-swipeout-" + id;

	    Repo.DeleteAccount(data, function (result) {
	        if (result.success) {
	            app.f7.swipeoutDelete(classid);
	        }
	        else {
	            app.f7.alert(result.message);
	        }
	    });
	}
	function saveAccount() {
	    var formInput = app.f7.formToJSON('#accountedit');
	    account.setValues(formInput);

	    if (!account.validate()) {
			app.f7.alert("Please fill all mandatory fields");
			return;
		}
	    app.Repository.SaveAccount(account, function (result) {
	        if (result.success) {
	            app.ReloadPreviousPage();
	            //app.mainView.router.reloadPage(app.mainView.history[app.mainView.history.length - 2]);
	           // app.mainView.router.back({ context: "reload" });

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