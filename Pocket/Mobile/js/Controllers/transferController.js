define(["app", "Views/Transfer/transferView", "Views/Transfer/transferlistView", "Models/transferModel", "ViewModels/transferViewModel", "pocketdataaccess"], function (app, TransferView, TransferListView, Transfer,TransferVM, Repo) {
    
	var state = {isNew: false};
	var transfer = null;
	var transfervm = null;

    // Infinite scroll vars
	var pageSize = 10;
	var page = 1;
	var totalpages = 1;
	var listquery;
	var loading = false;

	var myApp = app.f7;
	var $$ = Framework7.$;

	var bindings = [{
		element: '.transfer-save-link',
		event: 'click',
		handler: saveTransfer
	}];
	var scrollBindings = [{
	    element: '.transfer-delete',
	    event: 'click',
	    handler: deleteTransfer
	}, {
	    element: '.transfer-infinite-scroll',
	    event: 'infinite',
	    handler: infiniteScroll
	},
	{
	    element: '.transfer-from-date',
	    event: 'change',
	    handler: transferDateChanged
	}, {
	    element: '.transfer-to-date',
	    event: 'change',
	    handler: transferDateChanged
	}];

	function init(query) {
	    if (query && query.id) {
            
		    Repo.GetTransferByID(query.id, function (response) {
		        transfervm = new TransferVM(response.rows[0]);
		        state.isNew = false;
		        showTransfer();
		    });
		    
		}
		else {
	        transfervm = new TransferVM();
	        transfervm.TransferDate = toDateString(new Date());
		    state.isNew = true;
		    showTransfer();
		}
		
	}
	function showTransfer()
	{
	    transfervm.SourceAccounts = Repo.GetAccountGroups();
	    transfervm.TargetAccounts = Repo.GetAccountGroups();

	    TransferView.render({
	        model: transfervm,
	        state: state,
	        bindings: bindings
	    });

	    $$('.transfer-source-list').val(transfervm.SourceAccount);
	    $$('.transfer-target-list').val(transfervm.TargetAccount);
	    var kp = myApp.keypad({
	        input: '#TransferAmount',
	        type: 'numpad',
	        toolbar: true
	    });
	}
	function initlist(query) {
	    listquery = query || {};
	    var data = listquery;
	    data["page"] = 1;
	    data["rows"] = pageSize;
        
	    Repo.GetTransfers(data, function (response) {
	        totalpages = response.total;
	        page = response.page;

	        TransferListView.render({ model: response.rows, bindings: scrollBindings });
	        

	    });

	}
	function transferDateChanged() {
	    var data = {};
	    data.TransferFrom = $$('.transfer-from-date').val();
	    data.TransferTo = $$('.transfer-to-date').val();
	    initlist(data);
	}
	function infiniteScroll() {
	    // Exit, if loading in progress
	    if (loading) return;
	    if (page + 1 > totalpages) // page is 1 based index
	        return;
	    // Set loading flag
	    loading = true;
	    

	    // send the request
	    var data = listquery;
	    data["page"] = page + 1;
	    data["rows"] = pageSize;

	    // Generate new items HTML
	    Repo.GetTransfers(
            data, function (response) {
                totalpages = response.total;
                page = response.page;

                TransferListView.addPage(response.rows);

                // Reset loading flag
                loading = false;
                
            }
            );

	}
	function saveTransfer() {
	    var formInput = app.f7.formToJSON('#transferedit');

	    if (!transfer)
	        transfer = new Transfer();

	    transfer.setValues(formInput);
	    if (!transfer.validate()) {
	        app.f7.alert("Please fill all mandatory fields");
			return;
	    }
	    if (transfer.SourceAccount == transfer.TargetAccount) {
	        app.f7.alert("Source and Target accounts cannot be same.");
	        return;
	    }
	    Repo.SaveTransfer(transfer, function (result) {
	        if (result.success) {
	            app.ReloadPreviousPage();
	        }
	        else
	            app.f7.alert(result.message);
		});
		
	}
	function deleteTransfer(e, t) {
	    var id = $$(e.currentTarget).data('id');
	    var data = { TransferID: id };
	    var classid = ".transfer-swipeout-" + id;

	    Repo.DeleteTransfer(data, function (result) {
	        if (result.success) {
	            app.f7.swipeoutDelete(classid);
	        }
	        else {
	            app.f7.alert(result.message);
	        }
	    });
	}
	return {
	    init: init,
        initlist:initlist
	};
});