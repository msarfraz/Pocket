define(["app", "Views/Income/incomeView", "Views/Income/incomelistView", "Models/incomeModel", "ViewModels/incomeViewModel", "pocketdataaccess"], function (app, IncomeView, IncomeListView, Income,IncomeVM, Repo) {
    
	var state = {isNew: false};
	var income = new Income();
	var incomevm = null;

	var sourceMap = {}; // sourceMap[IncomeSourceID] = AccountID;
	var $$ = Framework7.$;
	var myApp = app.f7;

    // Infinite scroll vars
	var pageSize = 10;
	var page = 1;
	var totalpages = 1;
	var listquery;
	var loading = false;

	var bindings = [{
		element: '.income-save-link',
		event: 'click',
		handler: saveIncomeClick
	}, {
	    element: '.income-save-more-link',
	    event: 'click',
	    handler: saveIncomeMore
	}, {
	    element: '.income-account-list',
	    event: 'change',
	    handler: accountChanged
	}, {
	    element: '.income-source-list',
	    event: 'change',
	    handler: sourceChanged
	}];

	var scrollBindings = [{
	    element: '.income-delete-link',
	    event: 'click',
	    handler: deleteIncome
	}, {
	    element: '.infinite-scroll',
	    event: 'infinite',
	    handler: infiniteScroll
	},
	{
	    element: '.income-from-date',
	    event: 'change',
	    handler: incomeDateChanged
	}, {
	    element: '.income-to-date',
	    event: 'change',
	    handler: incomeDateChanged
	}];

	var incomeTypes = [{ value: 1, text: 'Debit' }, { value: 2, text: 'Credit' }, { value: 3, text: 'Saving' }];

	loadMaps();

	function init(query) {
	    if (query && query.id) {
            
		    Repo.GetIncomeByID(query.id, function(response){
		        incomevm = new IncomeVM(response.rows[0]);
		        state.isNew = false;
		        showIncome();
                
		    });
		    
		}
		else {
	        incomevm = new IncomeVM();

	        if (query.dt)
	            incomevm.IncomeDate = query.dt;
	        else
	            incomevm.IncomeDate = toDateString(new Date());
            state.isNew = true;
		    showIncome();
		}

		
	}
	function showIncome()
	{
	    incomevm.Sources = Repo.GetIncomeSources();
	    incomevm.AccountGroups = Repo.GetAccountGroups();
	    incomevm.RepeatTypes = Repo.GetRepeatTypes();

	    IncomeView.render({
	        model: incomevm,
	        state: state,
	        bindings: bindings
	    });
	    $$('.income-account-list').val(incomevm.AccountID);
	    $$('.income-repeat-list').val(incomevm.Repeat);
	    $$('.income-source-list').val(incomevm.SourceID);
	    var kp = myApp.keypad({
	        input: '#IncomeAmount',
	        type: 'numpad',
	        toolbar: true
	    });
	}
	function initlist(query) {
	    listquery = query || {};
	    var data = listquery;
	    data["page"] = 1;
	    data["rows"] = pageSize;
        
	    Repo.GetIncomes(data, function (response) {
	        totalpages = response.total;
	        page = response.page;

	        IncomeListView.render({ model: response.rows, bindings: scrollBindings });
	       

	    });
	}
	function sourceChanged()
	{
	    var sid = $$('.income-source-list').val();
	    var account;

	    if (sourceMap[sid]) {
	        account = GetObjectByID(incomevm.AccountGroups[0].Accounts, 'AccountID', sourceMap[sid]);
	    }
	    if (!account && incomevm.AccountGroups.length > 0 && incomevm.AccountGroups[0].Accounts.length > 0)
	        account = incomevm.AccountGroups[0].Accounts[0];
	    if (account) {
	        $$('.income-account-list').val(account.AccountID);
	        $$('.income-account-list-text').text(account.Name);
	    }
	}
	function accountChanged()
	{
	    sourceMap[$$('.income-source-list').val()] = $$('.income-account-list').val();
	}
	function saveMaps() {
	    localStorage["sourceMap"] = JSON.stringify(sourceMap);
	}
	function loadMaps() {
	    if (localStorage["sourceMap"])
	        sourceMap = JSON.parse(localStorage["sourceMap"]);
	}
	function incomeDateChanged()
	{
	    var data = {};
	    data.IncomeFrom = $$('.income-from-date').val();
	    data.IncomeTo = $$('.income-to-date').val();
	    initlist(data);
	}
	function deleteIncome(e, t) {
	    var id = $$(e.currentTarget).data('id');
	    var data = { IncomeID: id };
	    var classid = ".income-swipeout-" + id;

	    Repo.DeleteIncome(data, function (result) {
	        if (result.success) {
	            app.f7.swipeoutDelete(classid);
	        }
	        else {
	            app.f7.alert(result.message);
	        }
	    });
	}
	function saveIncomeMore()
	{
	    saveIncome(true);
	}
	function saveIncomeClick()
	{
	    saveIncome(false);
	}
	function saveIncome(saveMore) {
	    var formInput = app.f7.formToJSON('#incomeedit');

	    income.setValues(formInput);
	    if (!income.validate()) {
			app.f7.alert("Please fill all the details");
			return;
		}
	    Repo.SaveIncome(income, function (result) {
	        if (result.success) {
	            if (saveMore)
	            {
	                init();
	            }
	            else {
	                app.ReloadPreviousPage();
	            }

	        }
	        else
	            app.f7.alert(result.message);
		    
		});
	    saveMaps();
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
	    Repo.GetIncomes(
            data, function (response) {
                totalpages = response.total;
                page = response.page;

                IncomeListView.addPage(response.rows);

                // Reset loading flag
                loading = false;
                
            }
            );

	}
	return {
	    init: init,
        initlist:initlist
	};
});