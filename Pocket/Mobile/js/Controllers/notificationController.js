define(["app",  "Views/Notification/notificationlistView", "pocketdataaccess"], function (app,  NotificationListView, Repo) {

    var myApp = app.f7;
    var $$ = Framework7.$;

    // Infinite scroll vars
    var pageSize = 15;
    var page = 1;
    var totalpages = 1;
    var listquery;
    var loading = false;

    var listBindings = [{
        element: '.notification-infinite-scroll',
        event: 'infinite',
        handler: infiniteScroll
    }];

    function initlist(query) {
        listquery = query || {};
        var data = listquery;
        data["page"] = 1;
        data["rows"] = pageSize;
        
        Repo.GetNotifications(data, function (response) {
            totalpages = response.total;
            page = response.page;

            NotificationListView.render({ model: response.rows, bindings: listBindings });
            

            Repo.MarkAllRead({});
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
        Repo.GetNotifications(
            data, function (response) {
                totalpages = response.total;
                page = response.page;

                NotificationListView.addPage(response.rows);

                // Reset loading flag
                loading = false;
                
            }
            );

    }
    return {
        initlist: initlist
    };
});