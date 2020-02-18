define(['pocketserver'], function (pserver) {

    var _server = pserver.getServer();
    var AccessToken;
    var f7;

    function initialize(app) {
        f7 = app;
    }
    function getResults(serverUrl, data, callback) {
        try {
            console.log("getResults:" + serverUrl);
            var $$ = Framework7.$;

            $$.ajax({
                url: _server + serverUrl,
                type: "GET",
                data: data,
                //dataType: "jsonp",
                //jsonp: "d",
                contentType: 'application/json',
                //cache: false,
                crossDomain: true,
                xhrFields: {
                    withCredentials: false
                },

                headers: {
                    Authorization: AccessToken
                },
                error: function (response) {
                    f7.hideIndicator();
                    console.log(response.responseText);
                    f7.alert("Request failed: Try to login again." + response);
                },
                success: function (response) {
                    console.log("success:" + serverUrl);
                    callback(JSON.parse(response));

                },
                beforeSend: function (xhr) { console.log("beforeSend called."); xhr.setRequestHeader('Authorization', AccessToken); }
            });
        } catch (err) {
            alert(err);
        }
    }

    function setResults(serverUrl, data, callback) {
        console.log("setResults:" + serverUrl);
        var $$ = Framework7.$;
        try {
            $$.ajax({
                url: _server + serverUrl,
                data: JSON.stringify(data),
                type: "POST",
                //   dataType: "jsonp",
                // jsonp: "d",
                contentType: 'application/json',
                //cache: false,
                crossDomain: true,
                xhrFields: {
                    withCredentials: false
                },

                headers: {
                    Authorization: AccessToken
                },
                beforeSend: function (xhr) { xhr.setRequestHeader('Authorization', AccessToken); },
                success: function (response) {
                    console.log("success:" + serverUrl);
                    callback(JSON.parse(response));
                },
                error: function (response) {
                    f7.hideIndicator();
                    console.log(response.responseText);
                    f7.alert("Request failed: " + response);
                }
            });
        } catch (err) {
            alert(err);
        }
    }
    /* Login / Register */
    function AutoLogin(data, callback) {
        AccessToken = "Bearer " + localStorage.getItem('AT');

        setResults("/Account/MAutoLogin", data, callback);
    }
    function Login(data, callback) {
        setResults("/Account/MobileLogin", data, function (response) {
            if (response.rows[0].success) {
                AccessToken = "Bearer " + response.rows[0].AccessToken;
                if (data.RememberMe)
                    localStorage.setItem('AT', response.rows[0].AccessToken);
            }
            callback(response);
        });
    }

    function Register(data, callback) {
        setResults("/Account/MRegister", data, function (response) {
            if (response.rows[0].success) {
                AccessToken = "Bearer " + response.rows[0].AccessToken;
            }
            callback(response);
        });
    }
    function Logout() {
        AccessToken = "";
    }
        
    function LoadPayees(data,callback) {

        getResults("/Payee/MList", data, callback);
    }
    function LoadVendors(data, callback) {
        getResults("/Vendor/MList", data, callback);
    }

    function LoadAccounts(data, callback) {
        getResults("/QAccount/MAccounts", data, callback);
    }
    
    function SaveAccount(account, saveCallback) {
        setResults("/QAccount/MEdit", account, saveCallback);
    }
    function DeleteAccount(data, saveCallback) {
        setResults("/QAccount/MDelete", data, saveCallback);
    }
    function GetAccounts(data, callback) {
        getResults("/QAccount/MList", data, callback);
    }
    function GetAccountByID(data, callback) {
        getResults("/QAccount/MRecord", data, callback);
    }
    

    function SavePayee(payee, saveCallback) {
        setResults("/Payee/MEdit", payee, saveCallback);
    }
    function DeletePayee(data, saveCallback) {
        setResults("/Payee/MDelete", data, saveCallback);
    }

    function SaveVendor(vendor, saveCallback) {
        setResults("/Vendor/MEdit", vendor, saveCallback);
    }
    function DeleteVendor(data, saveCallback) {
        setResults("/Vendor/MDelete", data, saveCallback);
    }
    function LoadIncomeSources(data, callback) {
        getResults("/IncomeSource/MList", data, callback);
    }
    function SaveIncomeSource(incomesource, saveCallback) {
        setResults("/IncomeSource/MEdit", incomesource, saveCallback);
    }
    function DeleteIncomeSource(data, saveCallback) {
        setResults("/IncomeSource/MDelete", data, saveCallback);
    }
    function LoadCategories(data, callback) {
        getResults("/Category/MCategories", data, callback);
    }

    function GetCategories(data, callback) {
        getResults("/Category/MList", data, callback);
    }
    function SaveCategory(category, saveCallback) {
        setResults("/Category/MEdit", category, saveCallback);
    }
    function DeleteCategory(data, saveCallback) {
        setResults("/Category/MDelete", data, saveCallback);
    }
    function DeleteSubcategory(data, saveCallback) {
        setResults("/Subcat/MDelete", data, saveCallback);
    }
    function SaveSubcategory(subcategory, saveCallback) {
        setResults("/Subcat/MEdit", subcategory, saveCallback);
    }
    function GetCategoryByID(data, callback) {
        getResults("/Category/MCategoryByID", data, callback);
    }
    
    /* End Lists Data */

    /* Scrollable Data */
    function GetIncomes(data, callback) {
        getResults("/Income/MList", data, callback);
    }
    function GetIncomeByID(data, callback) {
        getResults("/Income/MRecord", data, callback);
    }
    function SaveIncome(income, saveCallback) {
        setResults("/Income/MEdit", income, saveCallback);
    }
    function DeleteIncome(data, callback) {
        setResults("/Income/MDelete", data, callback);
    }
    function GetTransfers(data, callback) {
        getResults("/QAccount/MTransferList", data, callback);
    }
    function GetTransferByID(data, callback) {
        getResults("/QAccount/MTransferRecord", data, callback);
    }
    function SaveTransfer(transfer, saveCallback) {
        setResults("/QAccount/MTransferEdit", transfer, saveCallback);
    }
    function DeleteTransfer(data, callback) {
        setResults("/QAccount/MTransferDelete", data, callback);
    }
    function GetExpenses(data, callback) {
        getResults("/Expense/MList", data, callback);

    }
    function GetExpenseByID(data, callback) {
        getResults("/Expense/MExpenseByID", data, callback);

    }
    function SaveExpense(expense, saveCallback) {
        setResults("/Expense/MEdit", expense, saveCallback);
    }
    function DeleteExpense(data, callback) {
        setResults("/Expense/MDelete", data, callback);
    }
    function GetExpenseComments(data, callback) {
        getResults("/Expense/MExpenseComments", data, callback);

    }
    function SaveComment(data, saveCallback) {
        setResults("/Expense/Mcommentedit", data, saveCallback);
    }

    function LoadEvents(data, callback) {
        getResults("/Event/MEvents", data, callback);

    }
    function GetEvents(data, callback) {
        return getResults("/Event/MList", data, callback);

    }
    function GetUpcomingEvents(data, callback) {
        return getResults("/Event/MUpcomingList", data, callback);

    }

    function SaveEvent(event, saveCallback) {
        setResults("/Event/MEdit", event, saveCallback);
    }
    function DeleteEvent(data, callback) {
        setResults("/Event/MDelete", data, callback);
    }
    function GetEventByID(data, callback) {
        getResults("/Event/MRecord", data, callback);
    }
    function LoadFriends(data, callback) {
        getResults("/Friend/MList", data, callback);
    }
    function GetFriends(data, callback) {
        getResults("/Friend/MList", data, callback);

    }
    function GetFriendRequests(data, callback) {
        getResults("/Friend/MPendingList", data, callback);
    }
    function SearchFriend(data, saveCallback) {
        setResults("/Friend/SearchFriend", data, saveCallback);
    }
    function ApproveFriend(data, saveCallback) {
        setResults("/Friend/MApproveFriend", data, saveCallback);
    }
    function RejectFriend(data, saveCallback) {
        setResults("/Friend/MRejectFriend", data, saveCallback);
    }
    function ShareResource(data, saveCallback) {
        setResults("/Friend/MShareResource", data, saveCallback);
    }
    function GetResourceFriends(data, callback) {
        getResults("/Friend/MResourceFriends", data, callback);
    }
    function GetAccountStatement(data, callback) {
        getResults("/Report/MAccountReport", data, callback);
    }
    function GetEventReport(data, callback) {
        getResults("/Report/MEventReport", data, callback);
    }
    function GetAdvanceReport(data, callback) {
        getResults("/Report/MAdvanceReport", data, callback);
    }
    function GetCategoryReport(data, callback) {
        getResults("/Report/MCategoryReport", data, callback);
    }
    function GetCategoryChartData(data, callback) {
        getResults("/Chart/MCategoryChartData", data, callback);
    }
    function GetSubcategoryChartData(data, callback) {
        getResults("/Chart/MSubcategoryChartData", data, callback);
    }
    function GetEventChartData(data, callback) {
        getResults("/Chart/MEventChartData", data, callback);
    }
    function GetHomeChartData(data, callback) {
        getResults("/Chart/MHomeChartData", data, callback);
    }
    function GetBudgetChartData(data, callback) {
        getResults("/Chart/MBudgetChartData", data, callback);
    }

    function GetBudgetDetails(data, callback) {
        getResults("/Home/MBudgetDetails", data, callback);
    }

    function LoadTargets(data, callback) {
        getResults("/Target/MTargets", data, callback);
    }
    function GetTargets(data, callback) {
        getResults("/Target/MList", data, callback);
    }
    function GetUpcomingTargets(data, callback) {
        getResults("/Target/MUpcomingList", data, callback);
    }
    
    function GetTargetByID(data, callback) {
        getResults("/Target/MRecord", data, callback);
    }
    function SaveTarget(target, saveCallback) {
        setResults("/Target/MEdit", target, saveCallback);
    }
    function DeleteTarget(data, callback) {
        setResults("/Target/MDelete", data, callback);
    }
    function GetSavings(data, callback) {
        getResults("/Saving/MList", data, callback);
    }
    function GetSavingByID(data, callback) {
        getResults("/Saving/MRecord", data, callback);
    }
    function SaveSaving(saving, saveCallback) {
        setResults("/Saving/MEdit", saving, saveCallback);
    }
    function DeleteSaving(data, callback) {
        setResults("/Saving/MDelete", data, callback);
    }

    function GetNotifications(data, callback) {
        getResults("/Notification/MList", data, callback);
    }

    function MarkAllRead(data, callback) {
        setResults("/Notification/MarkAllRead", data, callback);
    }
    function GetNotificationsCount(data, callback) {
        getResults("/Notification/NotificationsCount", data, callback);
    }

    return {

        GetNotifications: GetNotifications,
        GetNotificationsCount: GetNotificationsCount,
        MarkAllRead: MarkAllRead,

        Login: Login,
        AutoLogin: AutoLogin,
        Logout: Logout,
        Register: Register,
        initialize: initialize,

        GetSavings: GetSavings,
        GetSavingByID: GetSavingByID,
        SaveSaving: SaveSaving,
        DeleteSaving: DeleteSaving,

        GetTargets: GetTargets,
        LoadTargets: LoadTargets,
        GetUpcomingTargets: GetUpcomingTargets,
     //   GetTargetList: GetTargetList,
        GetTargetByID: GetTargetByID,
        SaveTarget: SaveTarget,
        DeleteTarget: DeleteTarget,

      //  LoadLists: LoadLists,
        //GetRepeatTypes: GetRepeatTypes,
        //GetTargetStatuses: GetTargetStatuses,
        //GetYesNoOptions: GetYesNoOptions,

        //GetRepository: GetRepository,

        LoadPayees: LoadPayees,
        SavePayee: SavePayee,
        DeletePayee: DeletePayee,

        LoadVendors: LoadVendors,
        SaveVendor: SaveVendor,
        DeleteVendor: DeleteVendor,

        LoadIncomeSources: LoadIncomeSources,
        SaveIncomeSource: SaveIncomeSource,
        DeleteIncomeSource: DeleteIncomeSource,

        GetAccounts: GetAccounts,
        LoadAccounts:LoadAccounts,
       // GetAccountGroups: GetAccountGroups,
        GetAccountByID: GetAccountByID,
        SaveAccount: SaveAccount,
        DeleteAccount: DeleteAccount,
      //  GetAccountTypes: GetAccountTypes,

        GetIncomes: GetIncomes,
        GetIncomeByID: GetIncomeByID,
        SaveIncome: SaveIncome,
        DeleteIncome: DeleteIncome,

        GetTransfers: GetTransfers,
        GetTransferByID: GetTransferByID,
        SaveTransfer: SaveTransfer,
        DeleteTransfer: DeleteTransfer,

        GetExpenses: GetExpenses,
        GetExpenseByID: GetExpenseByID,
        SaveExpense: SaveExpense,
        DeleteExpense: DeleteExpense,
        GetExpenseComments: GetExpenseComments,
        SaveComment: SaveComment,

        GetCategories: GetCategories,
        LoadCategories:LoadCategories,
     //   GetCategoryGroups: GetCategoryGroups,
       // GetSubcategories: GetSubcategories,
        SaveCategory: SaveCategory,
        GetCategoryByID: GetCategoryByID,
       // GetSubcategoryByID: GetSubcategoryByID,
        SaveSubcategory: SaveSubcategory,
        DeleteCategory: DeleteCategory,
        DeleteSubcategory: DeleteSubcategory,

        GetEvents: GetEvents,
        LoadEvents: LoadEvents,
        GetUpcomingEvents: GetUpcomingEvents,
      //  GetEventGroups: GetEventGroups,
        SaveEvent: SaveEvent,
        GetEventByID: GetEventByID,
        DeleteEvent: DeleteEvent,

        LoadFriends: LoadFriends,
        GetFriends: GetFriends,
        GetFriendRequests: GetFriendRequests,
        SearchFriend: SearchFriend,
    //    GetFriendByID: GetFriendByID,
        GetResourceFriends: GetResourceFriends,
        ShareResource: ShareResource,
        ApproveFriend: ApproveFriend,
        RejectFriend: RejectFriend,
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