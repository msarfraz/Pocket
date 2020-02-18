define(["app", "Views/Event/eventView", "Views/Event/eventlistView", "Models/eventModel","ViewModels/eventViewModel", "pocketdataaccess"], function (app, EventView, EventListView, Event, EventVM, Repo) {
    
	var state = {isNew: false};
	var event = new Event();
	var eventvm = null;
	var myApp = app.f7;
	var $$ = Framework7.$;

    // Infinite scroll vars
	var pageSize = 10;
	var page = 1;
	var totalpages = 1;
	var loading = false;
	var listquery;

	var bindings = [{
	    element: '.event-save-link',
	    event: 'click',
	    handler: saveEvent
	}];

	var listBindings = [{
	    element: '.event-delete-link',
	    event: 'click',
	    handler: deleteEvent
	}, {
	    element: '.event-share-link',
	    event: 'click',
	    handler: shareEvent
	}, {
	    element: '.event-expenses-link',
	    event: 'click',
	    handler: showEventExpenses
	}, {
	    element: '.event-infinite-scroll',
	    event: 'infinite',
	    handler: infiniteScroll
	}];
	function init(query){
	    if (query && query.id) {
		    Repo.GetEventByID(query.id, function (response) {
		        eventvm = new EventVM(response.rows[0]);
		        state.isNew = false;
		        showEvent();
                
		    });
		    
		}
		else {
	        eventvm = new EventVM();
	        eventvm.Editable = true;
	        state.isNew = true;
	        if (query.dt)
	            eventvm.EventDate = query.dt;
		    showEvent();
		}
		
		


	}
	function showEvent()
	{
	    eventvm.RepeatTypes = Repo.GetRepeatTypes();

	    EventView.render({
	        model: eventvm,
	        state: state,
	        bindings: bindings
	    });
	    $$(".event-budget-duration").val(eventvm.BudgetDuration);
	    $$(".event-budgeted").val(eventvm.Budgeted);
	    $$(".event-status").val(eventvm.EventStatus);
	    var kp = myApp.keypad({
	        input: '#EventBudgetAmount',
	        type: 'numpad',
	        toolbar: true
	    });
	}
	function initlist(query) {

	    listquery = query || {};
	    var data = listquery;
	    data["page"] = 1;
	    data["rows"] = pageSize;
	    Repo.GetEvents(data, function (response) {
	        totalpages = response.total;
	        page = response.page;

	        EventListView.render({ model: response.rows, bindings: listBindings });
	        

	    });
	}
	function saveEvent() {
	    var formInput = app.f7.formToJSON('#eventedit');

	    event.setValues(formInput);
	    if (!event.validate()) {
	        app.f7.alert("Please fill all mandatory fields");
			return;
		}
	    app.Repository.SaveEvent(event, function (result) {
	        if (result.success) {
	            app.ReloadPreviousPage();

	        }
	        else
	            app.f7.alert(result.message);
		});
	    
	}
	function deleteEvent(e, t) {
	    var id = $$(e.currentTarget).data('id');
	    var data = { EventID: id };
	    var classid = ".event-swipeout-" + id;

	    Repo.DeleteEvent(data, function (result) {
	        if (result.success) {
	            app.f7.swipeoutDelete(classid);
	        }
	        else {
	            app.f7.alert(result.message);
	        }
	    });
	}
	function shareEvent(e, t) {
	    var evid = $$(e.currentTarget).data('id');

	    app.mainView.loadPage("Pages/ResourceFriendList.html?ResourceType=1&ResourceID=" + evid);
	}
	function showEventExpenses(e, t) {
	    var evid = $$(e.currentTarget).data('id');

	    app.mainView.loadPage("Pages/ReportList.html?name=eventreport&eventid=" + evid);
	}
	function myExpenseAction(target, e) {
	    var evid = $$(target.currentTarget).data('event-id');

	    var buttons = [
        {
            text: 'Edit',
            bold: true,
            onClick: function (target,e) {
                app.mainView.loadPage("Pages/Event.html?id=" + evid);
            }
        },
        {
            text: 'View Expenses',
            onClick: function (target, e) {
                app.mainView.loadPage("Pages/ReportList.html?name=eventreport&eventid=" + evid);
            }
        },
        {
            text: 'Share',
            onClick: function (target, e) {
                app.mainView.loadPage("Pages/ResourceFriendList.html?ResourceType=1&ResourceID=" + evid);
            }
        },
        {
            text: 'Cancel',
            color: 'red'
        }
	    ];
	    myApp.actions(buttons);
	    
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
	    Repo.GetEvents(
            data, function (response) {
                totalpages = response.total;
                page = response.page;

                EventListView.addPage(response.rows);

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