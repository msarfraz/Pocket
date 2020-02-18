define(["app", "Views/Report/reportlistView", "Views/Report/reportCategoryView", "pocketdataaccess"], function (app, ReportListView,ReportCategoryView, Repo) {
    
	var state = {isNew: false};
	
	var $$ = Framework7.$;

    // Infinite scroll vars
	var pageSize = 10;
	var page = 1;
	var totalpages = 1;
	var listquery;
	var firstRender = true;
    var scrollBindings = [{
        element: '.add-event-expense',
        event: 'click',
        handler: addEventExpense
    },{
	    element: '.report-infinite-scroll',
	    event: 'infinite',
	    handler: infiniteScroll
	},
	{
	    element: '.report-from-date',
	    event: 'change',
	    handler: reportDateChanged
	}, {
	    element: '.report-to-date',
	    event: 'change',
	    handler: reportDateChanged
	}, {
	    element: '.filter-event-list',
	    event: 'change',
	    handler: filterChanged
	}, {
	    element: '.filter-vendor-list',
	    event: 'change',
	    handler: filterChanged
	}, {
	    element: '.filter-payee-list',
	    event: 'change',
	    handler: filterChanged
	}, {
	    element: '.filter-account-list',
	    event: 'change',
	    handler: filterChanged
	}, {
	    element: '.filter-subcategory-list',
	    event: 'change',
	    handler: filterChanged
	}, {
	    element: '.filter-category-list',
	    event: 'change',
	    handler: categoryChanged
	}, {
	    element: '.filter-all-users',
	    event: 'click',
	    handler: filterChanged
	}, {
	    element: '.filter-to-date',
	    event: 'change',
	    handler: filterChanged
	}, {
	    element: '.filter-from-date',
	    event: 'change',
	    handler: filterChanged
	}];
	var filter = {};

	/*$$('.accordion-item').on('opened', function () {
	    myApp.alert('Accordion item opened');
	});
	$$('.accordion-item').on('closed', function (e) {
	    myApp.alert('Accordion item closed');
	});*/
	var accordianBindings = [{
	    element: '.accordion-item',
	    event: 'opened',
	    handler: accordionOpened
	},
	{
	    element: '.accordion-item',
	    event: 'closed',
	    handler: accordionClosed
	}];
	function accordionOpened()
	{
	    app.f7.accordionClose(".subcat-accordion");
	}
	function accordionClosed(e)
	{
	    app.f7.alert('Accordion item closed');
	}
	function initcategory()
	{
	    var data = {};
	    data.FromDate = getMonthFirstDate();
	    data.ToDate = getMonthLastDate();
	    data.Month = 10;
	    data.Year = 2014;
	    Repo.GetCategoryReport(data, function (response) {
	        ReportCategoryView.render({ model: response.rows, bindings: accordianBindings });
	        //$$('.accordion-item').on('opened', function () {
	        //    app.f7.accordionClose(".subcat-accordion")
	        //    //app.f7.alert('Accordion item opened');
	        //});
	        //$$('.accordion-item').on('closed', function (e) {
	        //    app.f7.alert('Accordion item closed');
	        //});
	    });
	}
	function getMonthFirstDate()
	{
	    var firstDate = (new Date());
	    firstDate.setDate(1)
	    firstDate = firstDate.toISOString();
	    firstDate = firstDate.substring(0, firstDate.indexOf("T")); // returns date portion only
	    return firstDate;
	}
	function getMonthLastDate()
	{
	    var currDate = new Date();
	    var lastDate = (new Date(currDate.getFullYear(), currDate.getMonth() + 1, 0)).toISOString();
	    lastDate = lastDate.substring(0, lastDate.indexOf("T"));
	    return lastDate;
	}
	function initlist(query) {
	    listquery = query || {};
	    var data = listquery;
	    data["page"] = 1;
	    data["rows"] = pageSize;

	    if (listquery.name == "accountstatement") {
	            filter.From = getMonthFirstDate();
	            filter.To = getMonthLastDate();

	            $$(".report-filter").show();
	            $$('.add-event-expense').hide();
	    }
	    else if (listquery.name == "eventreport") {
	        $$(".report-filter").hide();
	        $$('.add-event-expense').show();
	    }
	    else if (listquery.name == "advancereport") {
	        
	        filter.Accounts = Repo.GetAccountGroups();
	        filter.Categories = Repo.GetCategoryGroups();
	        filter.Events = Repo.GetEventGroups();
	        filter.Payees = Repo.GetPayees();
	        filter.Vendors = Repo.GetVendors();

	        $$(".report-filter").show();
	        $$('.add-event-expense').show();
	    }

	    ReportListView.renderFilter({ bindings: scrollBindings, name: listquery.name, filter: filter });

	    if (listquery.catid && listquery.subcatid)
	        loadSubcategories(listquery.catid, listquery.subcatid);
	    if (listquery.currmonth) {
	        $$(".filter-from-date").val(getMonthFirstDate());
	        $$(".filter-to-date").val(getMonthLastDate());

	        $$(".report-from-date").val(getMonthFirstDate());
	        $$(".report-to-date").val(getMonthLastDate());
	    }
	    
	    LoadReport();
	    firstRender = true;
	}
	function LoadReport()
	{
        
	    var data = listquery;
	    if (listquery.name == "accountstatement") {
	        data["From"] = $$(".report-from-date").val() || filter.From;
	        data["To"] = $$(".report-to-date").val() || filter.To;
	        data["AccountID"] = listquery.accountid;
	        
	        Repo.GetAccountStatement(data, ShowReport);
	    }
	    else if (listquery.name == "eventreport") {
	        data["EventID"] = listquery.eventid;
	        
	        Repo.GetEventReport(data, ShowReport);
	    }
	    else if (listquery.name == "advancereport") {
	        data["EventID"] = $$(".filter-event-list").val();
	        data["VendorID"] = $$(".filter-vendor-list").val();
	        data["PayeeID"] = $$(".filter-payee-list").val();
	        data["AccountID"] = $$(".filter-account-list").val();
	        data["SubcatID"] = $$(".filter-subcategory-list").val();
	        data["CatID"] = $$(".filter-category-list").val();
	        data["AllUsers"] = $$(".filter-all-users").is(':checked');
	        data["ToDate"] = $$(".filter-to-date").val();
	        data["FromDate"] = $$(".filter-from-date").val();

	        Repo.GetAdvanceReport(data, ShowReport);
	    }
	}
	function addEventExpense()
	{
	    var evurl = "Pages/Expense.html?eventid=" + listquery.eventid;
	    app.mainView.router.loadPage(evurl);
	}
	function reportDateChanged() {
	    var data = listquery;
	    data["page"] = 1;
	    data["rows"] = pageSize;
	    firstRender = false;
	    LoadReport();
	}
	function filterChanged()
	{
	    var data = listquery;
	    data["page"] = 1;
	    data["rows"] = pageSize;
	    firstRender = false;
	    LoadReport();
	}
	function categoryChanged() {
	    var catid = $$('.filter-category-list').val();
	    
	    loadSubcategories(catid);

	    
	    filterChanged();
	}
	function loadSubcategories(catid, subcatid)
	{
	    var scats = Repo.GetSubcategories(catid);

	    var html = app.f7.templates.SubcatTemplate({ subcats: scats });
	    html = "<option></option>" + html;
	    $$('.filter-subcategory-list').html(html);
	    if (subcatid)
	    {
	        var scat = Repo.GetSubcategory(catid, subcatid);
	        if (scat)
	        {
	            $$('.filter-category-list').val(catid);
	            $$('.filter-subcategory-list').val(subcatid);
	            $$('.filter-subcategory-text').text(scat.Name);
	        }
	        
	    }
	    else
	    {

	        $$('.filter-subcategory-text').text("");
	    }
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
	    firstRender = false;
	    LoadReport();

	}
	function ShowReport(response)
	{
	    //if (response.page == 1 && firstRender)
	    //    ReportListView.render({ model: response.rows, bindings: scrollBindings, name: listquery.name, filter: filter });
	    if(response.page == 1)
	        ReportListView.render({ model: response.rows, bindings: null, name: listquery.name, filter: null });
	    else
            ReportListView.addPage(listquery.name, response.rows);

        totalpages = response.total;
        page = response.page;
	    // Reset loading flag
	    loading = false;
	    
	}
	return {
	    initlist: initlist,
	    initcategory: initcategory
	};
});