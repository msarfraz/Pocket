define(['clouddb', 'pocketdb'], function (cDB, pDB) {
    
    var Pocket = {};
    Pocket.DataAccess = {};

    /* Lists */
    Pocket.DataAccess._vendors = null;
    Pocket.DataAccess._payees = null;
    Pocket.DataAccess._incomesources = null;

    Pocket.DataAccess._accounts = null;
    Pocket.DataAccess._events = null;
    Pocket.DataAccess._categories = null;
    Pocket.DataAccess._targets = null;

    Pocket.DataAccess.RepeatTypes = [{ value: 0, text: 'None', selected: false }, { value: 1, text: 'Daily', selected: false }, { value: 2, text: 'Alternate Days', selected: false },
		                    { value: 7, text: 'Weekly', selected: false }, { value: 14, text: 'Bi Weekly', selected: false }, { value: 30, text: 'Monthly', selected: false },
		                    { value: 60, text: 'Bi Monthly', selected: false }, { value: 90, text: 'Quarterly', selected: false }, { value: 180, text: 'Bi Quarterly', selected: false },
		                    { value: 365, text: 'Yearly', selected: false }];

    Pocket.DataAccess.AccountTypes = [{ value: 1, text: 'Debit', selected: false }, { value: 2, text: 'Credit', selected: false }, { value: 3, text: 'Saving', selected: false }];
    Pocket.DataAccess.TargetStatuses = [{ value: 1, text: 'Active' }, { value: 2, text: 'Achieved' }, { value: 3, text: 'Cancelled' }];
    Pocket.DataAccess.YesNoOptions = [{ value: 0, text: 'No' }, { value: 1, text: 'Yes' }];

    var f7;
    
    function initialize(appref, isLocal, init_complete) {
        f7 = appref;
        if (isLocal) {
            db = pDB;
            db.initialize(appref, function(){
            LoadLists();
            
                if (init_complete)
                    init_complete();
            });
        }
        else
        {
            db = cDB;
            db.initialize(appref);
        }
            

        
    }

    function GetRepeatTypes()
    {
        return Pocket.DataAccess.RepeatTypes;
    }
    function GetTargetStatuses() {
        return Pocket.DataAccess.TargetStatuses;
    }
    function GetYesNoOptions() {
        return Pocket.DataAccess.YesNoOptions;
    }
    function GetAccountTypes() {
        return Pocket.DataAccess.AccountTypes;
    }
    function GetRepository() {
        return Pocket.DataAccess;
    }
    
    function setApp(app)
    {
        f7 = app;
    }

    /* Login / Register */
    function AutoLogin(data, callback) {
        db.AutoLogin(data, function (response) {
            if (response.rows[0].success) {
                LoadLists();
            }
            callback(response);
        });
    }
    function Login(data, callback)
    {
        db.Login(data, function (response) {
            if (response.rows[0].success) {
                LoadLists();
            }
            callback(response);
        });
    }

    function Register(data, callback) {
        db.Register(data, function (response) {
            if (response.rows[0].success) {
                LoadLists();
            }
            callback(response);
        });
    }
    function Logout() {
        db.Logout();
    }
    /* Lists Data */

    function LoadLists (callback) {
        LoadPayees();
        LoadVendors();
        LoadIncomeSources();

        LoadAccounts();
        LoadEvents();
        LoadCategories();
      //  LoadTargets();
    }
    function LoadPayees(callback)
    {
        db.LoadPayees(null, function (response) {
            Pocket.DataAccess._payees = response.rows;
            if (callback)
                callback(response);
        });
    }
    function LoadVendors(callback)
    {
        db.LoadVendors(null, function (response) {
            Pocket.DataAccess._vendors = response.rows;
            if (callback)
                callback(response);
        });
    }
    
    function LoadAccounts()
    {
        db.LoadAccounts(null, function (response) {
            Pocket.DataAccess._accounts = response.rows;
        });
    }
    function LoadTargets(callback) {
        db.LoadTargets(null, function (response) {
            Pocket.DataAccess._targets = response.rows;
            if (callback)
                callback(response);
        });
    }

    function GetAccountGroups() {
        if (!Pocket.DataAccess._accounts)
            LoadAccounts();
        return Pocket.DataAccess._accounts;
    }
    function SaveAccount(account, saveCallback) {
        db.SaveAccount(account, function (response) {
            if (response.success) {
                LoadAccounts();
            }
            saveCallback(response);
        });
    }
    function DeleteAccount(data, saveCallback) {
        db.DeleteAccount( data, function (response) {
            if (response.success) {
                LoadAccounts();
            }
            saveCallback(response);
        });
    }
    function GetAccounts(callback) {
        db.GetAccounts(null, callback);
    }
    function GetAccountByID(id, callback) {
        db.GetAccountByID( {AccountID:id}, callback);
    }
    function LoadIncomeSources(callback) {
        db.LoadIncomeSources(null, function (response) {
            Pocket.DataAccess._incomesources = response.rows;
            if (callback)
                callback(response);
        });
    }
    
    function GetPayees()
    {
        if (!Pocket.DataAccess._payees)
            LoadPayees();
        return Pocket.DataAccess._payees;
    }
    function SavePayee(payee, saveCallback)
    {
        
        db.SavePayee(payee, function(response){
            if (response.success)
            {
                LoadPayees(function (r) {
                    saveCallback(response);
                });
            }
            else
            saveCallback(response);
        } );
    }
    function DeletePayee(data, saveCallback) {
        db.DeletePayee(data, function (response) {
            if (response.success) {
                LoadPayees(function (r) {
                    saveCallback(response);
                });
            }
            else
                saveCallback(response);
        });
    }
    function GetVendors() {
        if (!Pocket.DataAccess._vendors)
            LoadVendors();
        return Pocket.DataAccess._vendors;
    }
    function SaveVendor(vendor, saveCallback) {
        db.SaveVendor(vendor, function (response) {
            if (response.success) {
                LoadVendors(function (r) {
                    saveCallback(response);
                });
            }
            else
            saveCallback(response);
        });
    }
    function DeleteVendor(data, saveCallback) {
        db.DeleteVendor(data, function (response) {
            if (response.success) {
                LoadVendors(function (r) {
                    saveCallback(response);
                });
            }
            else
                saveCallback(response);
        });
    }
    function GetIncomeSources() {
        if (!Pocket.DataAccess._incomesources)
            LoadIncomeSources();
        return Pocket.DataAccess._incomesources;
    }
    function SaveIncomeSource(incomesource, saveCallback) {
        db.SaveIncomeSource(incomesource, function (response) {
            if (response.success) {
                LoadIncomeSources(function (r) {
                    saveCallback(response);
                });
            }
            else
            saveCallback(response);
        });
    }
    function DeleteIncomeSource(data, saveCallback) {
        db.DeleteIncomeSource(data, function (response) {
            if (response.success) {
                LoadIncomeSources(function (r) {
                    saveCallback(response);
                });
            }
            else
                saveCallback(response);
        });
    }
    function LoadCategories() {
        db.LoadCategories(null, function (response) {
            Pocket.DataAccess._categories = response.rows;
        });
    }

    function GetCategoryGroups() {
        if (!Pocket.DataAccess._categories)
            LoadCategories();
        return Pocket.DataAccess._categories;
    }
    function GetCategories(callback) {
        db.GetCategories(null, callback);
    }
    function SaveCategory(category, saveCallback) {
        db.SaveCategory(category, function (response) {
            if (response.success) {
                LoadCategories();
            }
            saveCallback(response);
        });
    }
    function DeleteCategory(data, saveCallback) {
        db.DeleteCategory(data, function (response) {
            if (response.success) {
                LoadCategories();
            }
            saveCallback(response);
        });
    }
    function DeleteSubcategory(data, saveCallback) {
        db.DeleteSubcategory(data, function (response) {
            if (response.success) {
                LoadCategories();
            }
            saveCallback(response);
        });
    }
    function SaveSubcategory(subcategory, saveCallback) {
        db.SaveSubcategory(subcategory, function (response) {
            if (response.success) {
                LoadCategories();
            }
            saveCallback(response);
        });
    }
    function GetCategoryByID(id,  callback) {
        db.GetCategoryByID({ CategoryID: id }, callback);
    }
    function GetSubcategories(catid) {
        for (var i = 0; i < Pocket.DataAccess._categories.length; i++) {
            var cgroup = Pocket.DataAccess._categories[i];
            var cat = GetObjectByID(cgroup.Categories, "CategoryID", catid);
            if (cat)
                return cat.Subcategories;
        }
    }
    function GetSubcategoryByID(category, subcatid) {
        return GetObjectByID(category.Subcategories, "SubcategoryID", subcatid);
        //getResults("/Category/MSubcategoryByID", { SubcategoryID: subcatid }, callback);
    }
    function GetSubcategory(catid, subcatid) {
        var scats = GetSubcategories(catid);
        return GetObjectByID(scats, "SubcategoryID", subcatid);
        //getResults("/Category/MSubcategoryByID", { SubcategoryID: subcatid }, callback);
    }
    /* End Lists Data */

    /* Scrollable Data */
    function GetIncomes(data, callback) {
        db.GetIncomes( data, callback);
    }
    function GetIncomeByID(id, callback)
    {
        db.GetIncomeByID( { IncomeID: id }, callback);
    }
    function SaveIncome(income, saveCallback) {
        db.SaveIncome(income, saveCallback);
    }
    function DeleteIncome(data, callback) {
        db.DeleteIncome( data, callback);
    }
    function GetTransfers(data, callback) {
        db.GetTransfers( data, callback);
    }
    function GetTransferByID(id, callback) {
        db.GetTransferByID( { TransferID: id }, callback);
    }
    function SaveTransfer(transfer, saveCallback) {
        db.SaveTransfer( transfer, saveCallback);
    }
    function DeleteTransfer(data, callback) {
        db.DeleteTransfer( data, callback);
    }
    function GetExpenses(data, callback) {
        db.GetExpenses(data, callback);

    }
    function GetExpenseByID(id, callback) {
        db.GetExpenseByID( { ExpenseID: id }, callback);

    } 
    function SaveExpense(expense, saveCallback) {
        db.SaveExpense( expense, saveCallback);
    }
    function DeleteExpense(data, callback) {
        db.DeleteExpense( data, callback);
    }
    function GetExpenseComments(data, callback) {
        db.GetExpenseComments( data, callback);

    }
    function SaveComment(data, saveCallback) {
        db.SaveComment( data, saveCallback);
    }

    function LoadEvents() {
        db.LoadEvents( null, function (response) {
                Pocket.DataAccess._events = response.rows;
        });

    }
    function GetEvents(data, callback) {
        return db.GetEvents( data, callback);

    }
    function GetUpcomingEvents(data, callback) {
        return db.GetUpcomingEvents( data, callback);

    }
    function GetEventGroups(data, callback) {
        if (!Pocket.DataAccess._events)
            LoadEvents();
        return Pocket.DataAccess._events;
    }
    function SaveEvent(event, saveCallback) {
        db.SaveEvent(event, function (response) {
            LoadEvents();
            if (saveCallback)
                saveCallback(response);
        });
    }
    function DeleteEvent(data, callback) {
        db.DeleteEvent(data, function (response) {
            LoadEvents();
                callback(response);
        });
    }
    function GetEventByID(id, callback) {
        db.GetEventByID( { EventID: id }, callback);
    } 
    function LoadFriends() {
        db.LoadFriends( null, function (response) {
            Pocket.DataAccess._friends = response.rows;
        });
    }
    function GetFriends(callback) {
        db.GetFriends( null, callback);
        
    }
    function GetFriendRequests(callback) {
        db.GetFriendRequests( null, callback);
    }
    function SearchFriend(data, saveCallback) {
        db.SearchFriend( data, saveCallback);
    }
    function ApproveFriend(data, saveCallback) {
        db.ApproveFriend( data, saveCallback);
    }
    function RejectFriend(data, saveCallback) {
        db.RejectFriend( data, saveCallback);
    }
    function ShareResource(data, saveCallback) {
        db.ShareResource( data, saveCallback);
    } 
    function GetFriendByID(id) {
        return GetObjectByID(Pocket.DataAccess._friends, "UserID", id);
    }
    function GetResourceFriends(data, callback)
    {
        db.GetResourceFriends( data, callback);
    }
    function GetAccountStatement(data, callback) {
        db.GetAccountStatement( data, callback);
    }
    function GetTransactions(data, callback) {
        db.GetTransactions(data, callback);
    }
    function GetCalendarData(data, callback) {
        db.GetCalendarData(data, callback);
    }
    function GetEventReport(data, callback) {
        db.GetEventReport(data, callback);
    }
    function GetAdvanceReport(data, callback) {
        db.GetAdvanceReport(data, callback);
    }
    function GetCategoryReport(data, callback) {
        db.GetCategoryReport(data, callback);
    }
    function GetCategoryChartData(data, callback) {
        db.GetCategoryChartData( data, callback);
    }
    function GetSubcategoryChartData(data, callback) {
        db.GetSubcategoryChartData(data, callback);
    }
    function GetEventChartData(data, callback) {
        db.GetEventChartData( data, callback);
    }
    function GetHomeChartData(data, callback) {
        db.GetHomeChartData( data, callback);
    }
    function GetBudgetChartData(data, callback) {
        db.GetBudgetChartData( data, callback);
    }
    
    function GetBudgetDetails(data, callback)
    {
        db.GetBudgetDetails( data, callback);
    }

    function GetTargets(data, callback) {
        db.GetTargets(data, callback);
    }
    function GetUpcomingTargets(data, callback) {
        db.GetUpcomingTargets( data, callback);
    }
    function GetTargetList(data) {
        return Pocket.DataAccess._targets;
    }
    function GetTargetByID(id, callback) {
        db.GetTargetByID( { TargetID: id }, callback);
    }
    function SaveTarget(target, saveCallback) {
        db.SaveTarget( target, saveCallback);
    }
    function DeleteTarget(data, callback) {
        db.DeleteTarget( data, callback);
    }
    function GetSavings(data, callback) {
        db.GetSavings( data, callback);
    }
    function GetSavingByID(id, callback) {
        db.GetSavingByID( { SavingID: id }, callback);
    }
    function SaveSaving(saving, saveCallback) {
        db.SaveSaving( saving, saveCallback);
    }
    function DeleteSaving(data, callback) {
        db.DeleteSaving( data, callback);
    }

    function GetNotifications(data, callback) {
        db.GetNotifications(data, callback);
    }

    function MarkAllRead(data, callback) {
        db.MarkAllRead(data, callback);
    }
    function GetNotificationsCount(data, callback) {
        db.GetNotificationsCount( data, callback);
    }
    
    return {

        GetNotifications: GetNotifications,
        GetNotificationsCount: GetNotificationsCount,
        MarkAllRead: MarkAllRead,

        Login: Login,
        AutoLogin:AutoLogin,
        Logout:Logout,
        Register: Register,
        initialize: initialize,

        GetSavings: GetSavings,
        GetSavingByID: GetSavingByID,
        SaveSaving: SaveSaving,
        DeleteSaving: DeleteSaving,

        GetTargets: GetTargets,
        GetUpcomingTargets:GetUpcomingTargets,
        GetTargetList:GetTargetList,
        GetTargetByID: GetTargetByID,
        SaveTarget: SaveTarget,
        DeleteTarget: DeleteTarget,

        LoadLists: LoadLists,
        GetRepeatTypes: GetRepeatTypes,
        GetTargetStatuses: GetTargetStatuses,
        GetYesNoOptions: GetYesNoOptions,

        GetRepository: GetRepository,

        GetPayees: GetPayees,
        SavePayee: SavePayee,
        DeletePayee: DeletePayee,

        GetVendors: GetVendors,
        SaveVendor: SaveVendor,
        DeleteVendor:DeleteVendor,

        GetIncomeSources: GetIncomeSources,
        SaveIncomeSource: SaveIncomeSource,
        DeleteIncomeSource: DeleteIncomeSource,

        GetAccounts: GetAccounts,
        GetAccountGroups: GetAccountGroups,
        GetAccountByID:GetAccountByID,
        SaveAccount: SaveAccount,
        DeleteAccount:DeleteAccount,
        GetAccountTypes: GetAccountTypes,

        GetIncomes: GetIncomes,
        GetIncomeByID:GetIncomeByID,
        SaveIncome: SaveIncome,
        DeleteIncome: DeleteIncome,

        GetTransfers: GetTransfers,
        GetTransferByID: GetTransferByID,
        SaveTransfer: SaveTransfer,
        DeleteTransfer: DeleteTransfer,

        GetExpenses: GetExpenses,
        GetExpenseByID:GetExpenseByID,
        SaveExpense: SaveExpense,
        DeleteExpense:DeleteExpense,
        GetExpenseComments: GetExpenseComments,
        SaveComment:SaveComment,

        GetCategories: GetCategories,
        GetCategoryGroups: GetCategoryGroups,
        GetSubcategories:GetSubcategories,
        SaveCategory: SaveCategory,
        GetCategoryByID: GetCategoryByID,
        GetSubcategoryByID: GetSubcategoryByID,
        GetSubcategory: GetSubcategory,
        SaveSubcategory: SaveSubcategory,
        DeleteCategory: DeleteCategory,
        DeleteSubcategory: DeleteSubcategory,

        GetEvents: GetEvents,
        GetUpcomingEvents: GetUpcomingEvents,
        GetEventGroups: GetEventGroups,
        SaveEvent: SaveEvent,
        GetEventByID: GetEventByID,
        DeleteEvent:DeleteEvent,

        LoadFriends: LoadFriends,
        GetFriends: GetFriends,
        GetFriendRequests:GetFriendRequests,
        SearchFriend: SearchFriend,
        GetFriendByID: GetFriendByID,
        GetResourceFriends: GetResourceFriends,
        ShareResource: ShareResource,
        ApproveFriend: ApproveFriend,
        RejectFriend: RejectFriend,

        GetCalendarData:GetCalendarData,
        GetTransactions:GetTransactions,
        GetAccountStatement: GetAccountStatement,
        GetEventReport: GetEventReport,
        GetAdvanceReport: GetAdvanceReport,
        GetCategoryReport: GetCategoryReport,

        GetCategoryChartData: GetCategoryChartData,
        GetSubcategoryChartData: GetSubcategoryChartData,
        GetEventChartData: GetEventChartData,
        GetHomeChartData: GetHomeChartData,
        GetBudgetChartData: GetBudgetChartData,

        GetBudgetDetails: GetBudgetDetails
        
    };
});