define(function() {
	var $$ = Framework7.$;
	var mypages = ["undefined", "payee", "payeelist", "account", "accountlist", "income", "incomelist", "incomesource", "incomesourcelist",
                "vendor", "vendorlist", "contact", "contactlist", "expense", "expenselist", "category", "categorylist", "subcategory",
		        "event", "eventlist", "friend", "friendlist", "resfriendlist", "transfer", "transferlist", "reportlist", "reportcategory",
	            "chartcat", "home","homepage", "excommentlist", "targetlist", "target", "savinglist", "saving", "notificationlist", "budget", "calendar"];
	/**
	 * Init router, that handle page events
	 */ 
	function init(myApp) {
	    $$(document).on('pageBeforeInit', function (e) {
	        var page = e.detail.page;
	        var pname = page.name;
	        var fromPage = page.fromPage.name;
	        console.log("router from:" + fromPage);
	        console.log("router to:" + pname);
	        if (!fromPage)
	            fromPage = "undefined"; // first navigation
	        if (mypages.indexOf(pname) >= 0 && mypages.indexOf(fromPage) >= 0) {
	            var pcontroller = $$(page.container).data('pg-controller');
	            var pinit = $$(page.container).data('pg-init');
	            if (!pcontroller)
	                pcontroller = page.name;
	            if (!pinit)
	                pinit = "init";
	            load(pcontroller, pinit, page.query);
	        }


	    });
	    $$(document).on('pageBack', function (e) {
	        var page = e.detail.page;
	        var pname = page.name;
	        //var fromPage = page.fromPage.name;
	        console.log("back router from:" + page.context);
	        console.log("back router to:" + pname);
	        

	    });
	    
    }

	/**
	 * Load (or reload) controller from js code (another controller) - call it's init function
	 * @param controllerName
	 * @param viewName
     * @param query
	 */
	function load(controllerName, viewName, query) {
	    console.log("router.load:" + controllerName + "_" + viewName);
	    require(['Controllers/' + controllerName + 'Controller'], function (controller) {
	        console.log("router.loaded:");
		    controller[viewName](query);
		    //controller.init(query, DataAccess._payees);
		});
	}

	return {
        init: init,
		load: load
    };
});