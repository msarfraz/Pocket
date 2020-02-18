define(["app", "pocketdataaccess", "Views/Home/eventlistView", "Views/Home/targetlistView", "Views/Home/budgetView"], function (app, Repo, EventListView, TargetListView, BudgetView) {
    
    var $$ = Framework7.$;
    var bindings = [{
        element: '.constant-budget',
        event: 'change',
        handler: budgetChanged
    }];
    
    function emptypage()
    {

    }
    function init(query) {
        
        //if (google)
        //{

        ShowHomeChart();
        //}
        ShowUpcomingEvents();
        ShowUpcomingTargets();
	}
    function initbudget()
    {
        
     //   if (Template7.global.IsLocal)
       //     $$(".budget-filter").hide();
       

        Repo.GetBudgetDetails({ConstantBudget:false}, function (response) {
            var model = response.rows;
            model.YearMonth = GetCurrentYearMonth();
            BudgetView.render({ model: model,bindings:bindings });
            
            });
    }
    function budgetChanged()
    {
        
        Repo.GetBudgetDetails({ ConstantBudget: !$$(".constant-budget").is(":checked") }, function (response) {
            var model = response.rows;
            model.YearMonth = GetCurrentYearMonth();
            BudgetView.refresh({ model: model });
            
        });
    }
	function ShowHomeChart() {

            
            Repo.GetHomeChartData({}, function (response) {

	        var d = response.rows;
	        var chart1 = new Highcharts.Chart({
	            chart: {
	                renderTo: 'ccontainer'
	            },
	            title: {
	                text: 'Monthly Budget and Expense',
	                x: -20 //center
	            },
	            subtitle: {
	                text: 'Summary view',
	                x: -20
	            },
	            xAxis: {
	                categories: d.categories,
	                title: {
	                    text: 'Days in month'
	                }
	            },
	            yAxis: {
	                title: {
	                    text: 'Amount'
	                }
	            },
	            credits: {
	                enabled: false
	            },

	            series: d.series
	        });

	        ShowBudgetChart(d.series);
            
	    });
	}

	function ShowBudgetChart(arr) {

	    var expense = 0;
	    if (arr[0].data.length > 0)
	        expense = Math.floor(arr[0].data[arr[0].data.length -1].y);
	    var budget = Math.floor(arr[2].data[0].y);
	 //   var limit = arr[4];
	    var remainingBudget = Math.floor(budget - expense);
	    if (remainingBudget < 0)
	        remainingBudget = 0;

	    

	    $$('.monthly-budget').text(remainingBudget);
	    $$('.monthly-expense').text(expense);
	    $$('#bp').val(((remainingBudget)*100)/budget);
	}
	function ShowUpcomingEvents() {
	    Repo.GetUpcomingEvents({}, function (response) {

	        EventListView.render({ model: response.rows });

	    });
	}
	function ShowUpcomingTargets() {
	    Repo.GetUpcomingTargets({}, function (response) {

	        TargetListView.render({ model: response.rows });

	    });
	}
	return {
	    init: init,
	    initbudget: initbudget,
	    emptypage: emptypage
	};
});