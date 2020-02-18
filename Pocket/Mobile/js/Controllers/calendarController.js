define(["app", "Views/Calendar/calendarlistView", "ViewModels/calendarViewModel", "pocketdataaccess"], function (app, CalendarView, CalendarVM, Repo) {
    
	var state = {isNew: false};
	var expenses = {};
	var incomes = {};
	var events = {};

	var myApp = app.f7;
	var $$ = Framework7.$;
   
	var myCalendar;
	var bindings = [{
	    element: '.link-new-expense',
	    event: 'click',
	    handler: newExpense
	}, {
	    element: '.link-new-income',
	    event: 'click',
	    handler: newIncome
	}, {
	    element: '.link-new-event',
	    event: 'click',
	    handler: newEvent
	}];

	//$$('.').on('click', newExpense);
	//$$('.link-new-income').on('click', newIncome);
	//$$('.link-new-event').on('click', newEvent);



	function init(query) {
	    showCalendar();
	    loadSummary(getYearMonth(myCalendar.value[0]));
	}
	function loadSummary(yearMonth, callback)
	{
	    if (!yearMonth)
	        yearMonth = getYearMonth(new Date());
	    Repo.GetCalendarData({ YearMonth: yearMonth }, function (response) {
	        var d = response.rows;
	        expenses = {};
	        incomes = {};
	        events = {};

	        for (var i = 0; i < response.rows.length; i++) {
	            var r = response.rows[i];
	            if (r["TransactionType"] == "Credit")
	                expenses[r["Day"]] = r["TotalAmount"];
	            else if (r["TransactionType"] == "Debit")
	                incomes[r["Day"]] = r["TotalAmount"];
	            else if (r["TransactionType"] == "Event")
	                events[r["Day"]] = r["TotalAmount"];
	        }
	        ShowSummaryData();
	        Repo.GetTransactions({ TransactionDate: toDateString(myCalendar.value[0]) }, showTransactions);
	    });
	}
	function getExpense(day)
	{
	    return formatMoney( expenses[pad(day)] );

	    
	}
	function getEvent(day) {

	    return events[pad(day)];

	}
	function getIncome(day) {

	    return formatMoney( incomes[pad(day)] );
	    
	}
	function formatMoney(amount)
	{
	    if (amount) {
	        var money = parseInt(amount);
	        if (money > 0) {
	            if (money <= 999)
	                return money;
	            else if (money <= 9999)
	                return round(money / 1000, 1) + "k";
	            else if (money <= 99999)
	                return round(money / 1000, 0) + "K";
	            else
	                return money.toString().substring(0, 1) + "e";
	        }
	    }
	    return "";
	}
	

	function showTransactions(response)
	{
	    CalendarView.render({
	        model: response.rows,
	        state: state,
	        bindings: bindings
	    });
	}
	function ShowSummaryData()
	{
	    var expense, ev, income;

	    $$('.picker-calendar-month-current').find('.picker-calendar-day').each(function () {
	        var day = $$(this);
	        if (!day.hasClass('picker-calendar-day-next') && !day.hasClass('picker-calendar-day-prev')) {
	            var d = day.data('day');
	            expense = getExpense(d);
	            income = getIncome(d);
	            ev = getEvent(d);

	            var t = d;
	            var info = "<ul class='ul-no-bullets'><li class='li-no-bullets'><span>" + t + "</span></li><li class='li-no-bullets'>";
                if(ev)
	                info += "<i class='icon ion-ios-alarm cal-small-font color-orange'></i> "
	            if (expense)
	                info += "<p class='cal-small-font color-red'>" + expense + "</p>";
	            if (income && expense)
	                info += "<p class='cal-small-font'> / </p>";
	            if (income)
	                info += "<p class='cal-small-font color-blue'>" + income + "</p>";

	            info += "</li></ul>";

	            day.html(info);
	        }


	    });
	}
	function showCalendar()
	{
	    var selDate = new Date();
	    if (myCalendar)
	    {
	        selDate = myCalendar.value[0];
	        myCalendar.destroy();
	        $$('#calendar-inline-container').html('');
	    }

	        myCalendar = app.f7.calendar({
	            container: '#calendar-inline-container',
	            value: [selDate],
	            weekHeader: false,
	            footer: false,
                header:false,
	            onChange: function (p, values, displayValues) {
	                Repo.GetTransactions({ TransactionDate: toDateString(values[0]) }, showTransactions);
	            },
	            onMonthYearChangeEnd: function (p, year, month) {
	                var ym = pad(year) + '-' + pad(month + 1);
	                loadSummary(ym);
	            }

	        });

	    
	    
	}
	
	function newExpense()
	{
	    app.mainView.router.loadPage("Pages/Expense.html?dt=" + toDateString(myCalendar.value[0]));
	}
	function newEvent() {
	    app.mainView.router.loadPage("Pages/Event.html?dt=" + toDateString(myCalendar.value[0]));
	}
	function newIncome() {
	    app.mainView.router.loadPage("Pages/Income.html?dt=" + toDateString(myCalendar.value[0]));
	}
	return {
	    init: init
	};
});