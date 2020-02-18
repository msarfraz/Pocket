define(["app", "Views/Expense/expenseView", "Views/Expense/expenselistView", "Models/expenseModel", "ViewModels/expenseViewModel", "pocketdataaccess"], function (app, ExpenseView, ExpenseListView, Expense,ExpenseVM, Repo) {
    
	var state = {isNew: false};
	var expense = new Expense();
	var expensevm = null;
	var myApp = app.f7;

	var catMap = {}; //catMap[CategoryID] = SubcategoryID;
	var accountMap = {}; // accountMap[SubcategoryID] = AccountID;
	var $$ = Framework7.$;

    // Loading flag
	var loading = false;

    // Infinite scroll vars
	var pageSize = 5;
	var page = 1;
	var totalpages = 1;
	var listquery;

	var exBindings = [{
	    element: '.expense-save-link',
	    event: 'click',
	    handler: saveExpenseClick
	}, {
		element: '.expense-save-more-link',
		event: 'click',
		handler: saveExpenseMore
	}, {
	    element: '.expense-category-list',
	    event: 'change',
	    handler: categoryChanged
	},
	{
	    element: '.expense-subcategory-list',
	    event: 'change',
	    handler: subcategoryChanged
	}, {
	    element: '.expense-account-list',
	    event: 'change',
	    handler: accountChanged
	}];
	var exlistBindings = [{
	    element: '.expense-delete-link',
	    event: 'click',
	    handler: deleteExpense
	}, {
        element: '.expense-infinite-scroll',
        event: 'infinite',
        handler: infiniteExpenseScroll
	},
	{
	    element: '.expense-from-date',
	    event: 'change',
	    handler: expenseChanged
	}, {
	    element: '.expense-to-date',
	    event: 'change',
	    handler: expenseChanged
	}, {
	    element: '.expense-all-users',
	    event: 'change',
	    handler: expenseChanged
	}];
	var expenseTypes = [{ value: 1, text: 'Debit' }, { value: 2, text: 'Credit' }, { value: 3, text: 'Saving' }];
	
	loadMaps();
	function init(query) {
	    
	    if (query && query.id) {
            
	        Repo.GetExpenseByID(query.id, function (response) {
	            expensevm = new ExpenseVM(response.rows[0]);

	            state.isNew = false;
	            showExpense();
                
	        });
		    
		}
		else {
	        expensevm = new ExpenseVM();
	        expensevm.ExpenseID = 0;
	        expensevm.Editable = true;
		    state.isNew = true;
            if(query.dt)
    		    expensevm.ExpenseDate = query.dt;
		    else
                expensevm.ExpenseDate = toDateString(new Date());
            if (query.eventid)
                expensevm.EventID = query.eventid;
		    showExpense();
		}

		
	}
	function showExpense()
	{
	    expensevm.AccountGroups =Repo.GetAccountGroups(); 
	    expensevm.CategoryGroups = Repo.GetCategoryGroups();
	    expensevm.Subcategories = Repo.GetSubcategories(expensevm.CategoryID);
	    expensevm.RepeatTypes = Repo.GetRepeatTypes();
	    expensevm.EventGroups = Repo.GetEventGroups();
	    expensevm.Payees = Repo.GetPayees();
	    expensevm.Vendors = Repo.GetVendors();

	    ExpenseView.render({
	        model: expensevm,
	        state: state,
	        bindings: exBindings
	    });
	        $$('.expense-category-list').val(expensevm.CategoryID);
	        $$('.expense-subcategory-list').val(expensevm.SubcategoryID);
	        $$('.expense-account-list').val(expensevm.AccountID);
	        $$('.expense-repeat-list').val(expensevm.Repeat);
	        $$('.expense-event-list').val(expensevm.EventID);
	        $$('.expense-payee-list').val(expensevm.PayeeID);
	        $$('.expense-vendor-list').val(expensevm.VendorID);
	        var calculator = myApp.keypad({
	            input: '#ExpenseAmount',
	            type: 'numpad',
	            toolbar: true
	        });
	    
	}
	function initlist(query) {
	    listquery = query || {};
	    var data = listquery;
	    data["page"] = 1;
	    data["rows"] = pageSize;

	    
	    
	    var expenses = Repo.GetExpenses(data, function (response) {
	        totalpages = response.total;
	        page = response.page;

	        ExpenseListView.render({ model: response.rows, bindings: exlistBindings });
	        
	     //   app.f7.upscroller('Go up');
	    });
	    
	}
	function expenseChanged() {
	    var data = listquery;
	    data.ExpenseFrom = $$('.expense-from-date').val();
	    data.ExpenseTo = $$('.expense-to-date').val();
	    data.AllUsers = $$('.expense-all-users').is(":checked");
	    initlist(data);
	}
	function categoryChanged()
	{
	    var catid = $$('.expense-category-list').val();
	    
	    var scats = Repo.GetSubcategories(catid);

	    var html = app.f7.templates.SubcatTemplate({ subcats: scats });
	    $$('.expense-subcategory-list').html(html);

	    var scat;
	    if (catMap[catid])
	    {
	        scat = GetObjectByID(scats, 'SubcategoryID', catMap[catid]);
	    }
	    if (!scat && scats.length > 0) {
	        scat = scats[0];
	    }

	    var account;
	    if (scat) {
	        $$('.expense-subcategory-list').val(scat.SubcategoryID);
	        $$('.expense-subcategory-text').text(scat.Name);
	        if (accountMap[scat.SubcategoryID])
	            account = GetObjectByID(expensevm.AccountGroups[0].Accounts, 'AccountID', accountMap[scat.SubcategoryID]);
	    }

	    if (!account && expensevm.AccountGroups.length > 0 && expensevm.AccountGroups[0].Accounts.length > 0)
	        account = expensevm.AccountGroups[0].Accounts[0];
	    if (account) {
	        $$('.expense-account-list').val(account.AccountID);
	        $$('.expense-account-list-text').text(account.Name);
	    }

	}
	function subcategoryChanged() {

	    var catid = $$('.expense-category-list').val();
	    var scatid = $$('.expense-subcategory-list').val();
	    catMap[catid] = scatid;
	    if (accountMap[scatid]) {
	        var account = GetObjectByID(expensevm.AccountGroups[0].Accounts, 'AccountID', accountMap[scatid]);
	        $$('.expense-account-list').val(account.AccountID);
	        $$('.expense-account-list-text').text(account.Name);
	    }
	}
	function saveMaps()
	{
	    localStorage["catMap"] = JSON.stringify(catMap);
	    localStorage["accountMap"] = JSON.stringify(accountMap);
	}
	function loadMaps()
	{
	    if (localStorage["catMap"])
	        catMap = JSON.parse(localStorage["catMap"]);
	    if (localStorage["accountMap"])
    	    accountMap = JSON.parse(localStorage["accountMap"]);
	}
	function accountChanged() {

	    var accountid = $$('.expense-account-list').val();
	    var scatid = $$('.expense-subcategory-list').val();
	    accountMap[scatid] = accountid;
	}
	function saveExpenseMore() {
	    saveExpense(true);
	}
	function saveExpenseClick() {
	    saveExpense(false);
	}
	function saveExpense(saveMore) {
	    var formInput = app.f7.formToJSON('#expenseedit');

	    expense.setValues(formInput);
	    if (!expense.validate()) {
			app.f7.alert("Please fill all the details");
			return;
		}
	    Repo.SaveExpense(expense, function (result) {
	        if (result.success) {
	            if (saveMore) {
	                init({});
	            }
	            else
	            {
	                app.ReloadPreviousPage();
	            }
	        

	        }
	        else
	            app.f7.alert(result.message);
		});
	    saveMaps();
	}
	function deleteExpense(e, t) {
	    var id = $$(e.currentTarget).data('id');
	    var data = { ExpenseID: id };
	    var classid = ".expense-swipeout-" + id;

	    Repo.DeleteExpense(data, function (result) {
	        if (result.success) {
	            app.f7.swipeoutDelete(classid);
	        }
	        else {
	            app.f7.alert(result.message);
	        }
	    });
	}
	function infiniteExpenseScroll()
	{
	    // Exit, if loading in progress
	    if (loading) return;
	    if (page + 1 > totalpages) // page is 1 based index
	        return;
	    // Set loading flag
	    loading = true;
	    
	    
        // send the request
	    var data = listquery;
	    data["page"] = page+1;
	    data["rows"] = pageSize;

	    // Generate new items HTML
	    var expenses = Repo.GetExpenses(
            data, function (response) {
                totalpages = response.total;
                page = response.page;

                ExpenseListView.addPage(response.rows);

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