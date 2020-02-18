define(["app", "Views/Friend/friendView", "Views/Friend/friendlistView", "Views/Friend/resfriendlistView", "Models/friendModel", "pocketdataaccess"], function (app, FriendView, FriendListView, ResFriendListView, Friend, Repo) {
    
	var state = {isNew: false};
	var friend = null;

	var $$ = Framework7.$;

    // Infinite scroll vars
	var pageSize = 10;
	var page = 1;
	var totalpages = 1;
	var listquery;
	
	var resbindings = [
		{
	    element: '.infinite-scroll',
	    event: 'infinite',
	    handler: infiniteScroll
	}, {
	    element: '.resfriend-shared',
	    event: 'change',
	    handler: shareResource
	}];
	var addFriendbindings = [{
	    element: '.friend-search',
	    event: 'click',
	    handler: searchFriend
	}];
	var pendingFriendbindings = [{
	    element: '.approve-request',
	    event: 'click',
	    handler: approveFRequest
	}, {
	    element: '.reject-request',
	    event: 'click',
	    handler: rejectFRequest
	}];

	function init(query){
		if (query && query.id) {
		    friend = new Friend(Repo.GetFriendByID(query.id)); // JSON.parse(localStorage.getItem("f7Base"));
		    state.isNew = false;
		}
		else {
		    friend = new Friend();
			state.isNew = true;
		}
		FriendView.render({
		    model: friend,
			state: state,
			bindings: addFriendbindings
		});
	}
	function initlist(query) {
	    Repo.GetFriends(function (response) {
	        FriendListView.render({ model: response.rows, bindings: null });
	    });
	    var frequests = Repo.GetFriendRequests(function (response) {
	        FriendListView.renderFRequests({ model: response.rows, bindings: pendingFriendbindings });
	    });

	}
	function initResourceFriendList(query) {
	    if (query && query.ResourceID && query.ResourceType) {
	        listquery = query;
	        var data = listquery;
	        data["page"] = 1;
	        data["rows"] = pageSize;
            
	        Repo.GetResourceFriends(data, function (response) {
	            totalpages = response.total;
	            page = response.page;

	            ResFriendListView.render({ model: response.rows, bindings: resbindings });
	            

	        });
	       
	    }

	}
	function searchFriend() {

	    var formInput = app.f7.formToJSON('#friendedit');

	    friend.setValues(formInput);
	    if (!friend.validate()) {
			app.f7.alert("Please fill all details");
			return;
		}
	    Repo.SearchFriend(friend, function (result) {
	        if (result.success)
	        {
	            app.f7.addNotification({
	                title: 'Pocket',
	                message: 'The friend request is sent.',
	                hold: 5000
	            });
	            initlist();
	        }
	        else
	        {
	            app.f7.alert(result.message);
	        }
		});
		
	}
	function shareResource(e,t)
	{
	    var friendid = $$(e.currentTarget).data('fr-id');
	    listquery["FriendID"] = friendid;
	    listquery["Shared"] = $$(e.currentTarget).is(':checked');
	    Repo.ShareResource(listquery, function (result) {
	        
	    });
	}
	function rejectFRequest(e, t) {
	    var friendid = $$(e.currentTarget).data('fr-id');
	    var data = {FriendID:friendid};
	    Repo.RejectFriend(data, function (result) {
	        if (result.success) {
	            app.f7.addNotification({
	                title: 'Pocket',
	                message: 'The friend request is rejected.',
	                hold: 5000
	            });
	            initlist();
	        }
	        else {
	            app.f7.alert(result.message);
	        }
	    });
	}
	function approveFRequest(e, t) {
	    var friendid = $$(e.currentTarget).data('fr-id');
	    var data = { FriendID: friendid };
	    Repo.ApproveFriend(data, function (result) {
	        if (result.success) {
	            app.f7.addNotification({
	                title: 'Pocket',
	                message: 'The friend request is approved.',
	                hold: 5000
	            });
	            initlist();
	        }
	        else
	        {
	            app.f7.alert(result.message);
	        }
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
	    Repo.GetResourceFriends(
            data, function (response) {
                totalpages = response.total;
                page = response.page;

                ResFriendListView.addPage(response.rows);

                // Reset loading flag
                loading = false;
                
            });

	}
	return {
	    init: init,
	    initlist: initlist,
	    initResourceFriendList: initResourceFriendList
	};
});