define(["app", "Views/Saving/savingView", "Views/Saving/savinglistView", "Models/savingModel", "ViewModels/savingViewModel", "pocketdataaccess"], function (app, SavingView, SavingListView, Saving, SavingVM, Repo) {

    var state = { isNew: false };
    var saving = new Saving();
    var savingvm = null;
    var myApp = app.f7;
    var $$ = Framework7.$;

    // Infinite scroll vars
    var pageSize = 10;
    var page = 1;
    var totalpages = 1;
    var listquery;
    var loading = false;

    var bindings = [ {
        element: '.saving-save-link',
        event: 'click',
        handler: saveSaving
    }];

    var listBindings = [{
        element: '.saving-delete-link',
        event: 'click',
        handler: deleteSaving
    }, {
        element: '.saving-infinite-scroll',
        event: 'infinite',
        handler: infiniteScroll
    }];
    function init(query) {
        if (query && query.id) {

            Repo.GetSavingByID(query.id, function (response) {
                savingvm = new SavingVM(response.rows[0]);
                state.isNew = false;
                showSaving();
            });

        }
        else {
            savingvm = new SavingVM();
            if (listquery.TargetID)
                savingvm.TargetID = listquery.TargetID;
            state.isNew = true;
            showSaving();
        }
        
    }
    

    function showSaving() {
        savingvm.AccountGroups = Repo.GetAccountGroups();
        savingvm.Targets = Repo.GetTargetList();

        SavingView.render({
            model: savingvm,
            state: state,
            bindings: bindings
        });
        $$(".saving-account-list").val(savingvm.AccountID);
        $$(".saving-target-list").val(savingvm.TargetID);
        var kp = myApp.keypad({
            input: '#SavingAmount',
            type: 'numpad',
            toolbar: true
        });
    }
    function initlist(query) {
        listquery = query || {};
        var data = listquery;
        data["page"] = 1;
        data["rows"] = pageSize;
        
        Repo.GetSavings(data, function (response) {
            totalpages = response.total;
            page = response.page;

            SavingListView.render({ model: response.rows, bindings: listBindings });
            

        });
    }
    function deleteSaving(e, t) {
        var id = $$(e.currentTarget).data('id');
        var data = { SavingID: id };
        var classid = ".saving-swipeout-" + id;

        Repo.DeleteSaving(data, function (result) {
            if (result.success) {
                app.f7.swipeoutDelete(classid);
            }
            else {
                app.f7.alert(result.message);
            }
        });
    }
    function saveSaving() {
        var formInput = app.f7.formToJSON('#savingedit');

        saving.setValues(formInput);
        if (!saving.validate()) {
            app.f7.alert("Please fill all fields.");
            return;
        }
        app.Repository.SaveSaving(saving, function (result) {
            if (result.success) {
                app.ReloadPreviousPage();
            }
            else
                app.f7.alert(result.message);
        });

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
        Repo.GetSavings(
            data, function (response) {
                totalpages = response.total;
                page = response.page;

                SavingListView.addPage(response.rows);

                // Reset loading flag
                loading = false;
                
            }
            );

    }
    return {
        init: init,
        initlist: initlist
    };
});