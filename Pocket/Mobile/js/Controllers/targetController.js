define(["app", "Views/Target/targetView", "Views/Target/targetlistView", "Models/targetModel", "ViewModels/targetViewModel", "pocketdataaccess"], function (app, TargetView, TargetListView, Target, TargetVM, Repo) {

    var state = { isNew: false };
    var target = new Target();
    var targetvm = null;
    var myApp = app.f7;
    var $$ = Framework7.$;

    // Infinite scroll vars
    var pageSize = 10;
    var page = 1;
    var totalpages = 1;
    var listquery;
    var loading = false;

    var bindings = [{
        element: '.target-save-link',
        event: 'click',
        handler: saveTarget
    }];

    var listBindings = [{
        element: '.target-delete-link',
        event: 'click',
        handler: deleteTarget
    }, {
        element: '.target-infinite-scroll',
        event: 'infinite',
        handler: infiniteScroll
    }];
    function init(query) {
        if (query && query.id) {
            
            Repo.GetTargetByID(query.id, function (response) {
                targetvm = new TargetVM(response.rows[0]);
                state.isNew = false;
                showTarget();
            });

        }
        else {
            targetvm = new TargetVM();
            state.isNew = true;
            showTarget();
        }

        
        
    }
    

    function showTarget() {
        targetvm.RepeatTypes = Repo.GetRepeatTypes();
        targetvm.TargetStatuses = Repo.GetTargetStatuses();
        targetvm.YesNoOptions = Repo.GetYesNoOptions();

        TargetView.render({
            model: targetvm,
            state: state,
            bindings: bindings
        });
        $$(".target-budget-duration").val(targetvm.BudgetDuration);
        $$(".target-status").val(targetvm.Status);
        $$(".target-budgeted").val(targetvm.Budgeted);
        var kp = myApp.keypad({
            input: '#TargetAmount',
            type: 'numpad',
            toolbar: true
        });
        var kp2 = myApp.keypad({
            input: '#TargetInitialAmount',
            type: 'numpad',
            toolbar: true
        });
        
    }
    function initlist(query) {
        console.log("target.initlist: called.");
        listquery = query || {};
        var data = listquery;
        data["page"] = 1;
        data["rows"] = pageSize;
        

        console.log("Repo.GetTargets:");
        Repo.GetTargets(data, function (response) {
            console.log("Callback Repo.GetTargets:");
            totalpages = response.total;
            page = response.page;

            TargetListView.render({ model: response.rows, bindings: listBindings });
            

        });
    }
    function deleteTarget(e, t) {
        var id = $$(e.currentTarget).data('id');
        var data = { TargetID: id };
        var classid = ".target-swipeout-" + id;

        Repo.DeleteTarget(data, function (result) {
            if (result.success) {
                app.f7.swipeoutDelete(classid);
            }
            else {
                app.f7.alert(result.message);
            }
        });
    }
    function saveTarget() {
        var formInput = app.f7.formToJSON('#targetedit');

        target.setValues(formInput);
        if (!target.validate()) {
            app.f7.alert("Please fill all mandatory fields");
            return;
        }
        app.Repository.SaveTarget(target, function (result) {
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
        Repo.GetTargets(
            data, function (response) {
                totalpages = response.total;
                page = response.page;

                TargetListView.addPage(response.rows);

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