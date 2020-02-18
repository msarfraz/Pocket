define(["app", "pocketdataaccess"], function (app, Repo) {
    
    var $$ = Framework7.$;
    var activeChart = "cat";

    function bindEvents() {
        $$('.chart-date').on('change', function () {
            ShowChart();

        });

        $$('.cat-chart').on('click', catChartSelected);
        $$('.subcat-chart').on('click', function () {
            $$('.' + activeChart + '-chart').removeClass("active");
            activeChart = "subcat";
            $$('.' + activeChart + '-chart').addClass("active");
            ShowChart();

        });
        $$('.event-chart').on('click', function () {
            $$('.' + activeChart + '-chart').removeClass("active");
            activeChart = "event";
            $$('.' + activeChart + '-chart').addClass("active");
            ShowChart();

        });
        $$('.chart-category-list').on('change', function () {
            activeChart = "subcat";
            ShowChart();

        });
    }
    
    function catChartSelected() {
        $$('.' + activeChart + '-chart').removeClass("active");
        activeChart = "cat";
        $$('.' + activeChart + '-chart').addClass("active");
        ShowChart();

    }
    function ShowChart()
    {
        if (activeChart == "cat")
        {
            $$('.chart-category-list-filter').hide();
            ShowCategoryChart();
        }
        else if (activeChart == "subcat")
        {
            $$('.chart-category-list-filter').show();
            ShowSubcategoryChart();
        }
        else
        {
            $$('.chart-category-list-filter').hide();
            ShowEventChart();
        }
    }
	function init(query) {
	    
	    var dt = new Date();
	    var YearMonth = dt.getFullYear() + "-" + (dt.getMonth() + 1);
	    $$(".chart-date").val(YearMonth);
	    var CategoryGroups = Repo.GetCategoryGroups();

	    var html = app.f7.templates.CatTemplate({ CategoryGroups: CategoryGroups });
	    $$('.chart-category-list').html(html);
	    bindEvents();

	    catChartSelected();
	    ShowChart(); // by default display the category chart
	}
	function ShowCategoryChart()
	{
	    var data = {};

	    data.YearMonth = $$(".chart-date").val();

	    Repo.GetCategoryChartData(data, function (response) {
	        //google.visualization.arrayToDataTable([
	        //        ['Year', 'Sales', 'Expenses'],
	        //        ['2004', 1000, 400],
	        //        ['2005', 1170, 460],
	        //        ['2006', 660, 1120],
	        //        ['2007', 1030, 540]
	        //]);
	       /* var cdata = google.visualization.arrayToDataTable(response.rows); */
	        var colOptions = {
	            title: 'Category View',
	            legend: { position: 'bottom' },
	            backgroundColor: "transparent"
	            //chartArea: { width: "50%", height: "70%" }

	        };
	        var pieOptions = {
	            title: 'Categories',
	            legend: { position: 'bottom', alignment: 'start'},
	            backgroundColor: "transparent",
	            pieSliceText: 'Category'
	            //chartArea: { width: "50%", height: "70%" }
	        };
	        var d = response.rows;
	        DrawChart(d, colOptions, pieOptions);
	    });
	}
	function ShowSubcategoryChart() {
	    var data = {};

	    data.YearMonth = $$(".chart-date").val();
	    data.CategoryID = $$('.chart-category-list').val();

	    Repo.GetSubcategoryChartData(data, function (response) {
	        var cdata = response.rows; // google.visualization.arrayToDataTable(response.rows);
	        var colOptions = {
	            title: 'Subcategory View'
	        };
	        var pieOptions = {
	            title: 'Subcategories'
	        };
	        DrawChart(cdata, colOptions, pieOptions);
	    });
	}
	function ShowEventChart() {
	    var data = {};
        
	    data.YearMonth = $$(".chart-date").val();

	    Repo.GetEventChartData(data, function (response) {
	        var cdata = response.rows; // google.visualization.arrayToDataTable(response.rows);
	        var colOptions = {
	            title: 'Events'
	        };
	        var pieOptions = {
	            title: 'Events'
	        };
	        DrawChart(cdata, colOptions, pieOptions);
	    });
	}
	function DrawChart(d, colOptions, pieOptions)
	{
	   // var col = $$('.chart-column-area')[0];
	   // var pie = $$('.chart-pie-area')[0];
	    var chart1 = new Highcharts.Chart({
	        chart: {
	            type: 'bar',
	            renderTo: 'chartarea'
	        },
	        title: {
	            text: 'Budget and Expense'
	        },
	        subtitle: {
	            text: colOptions.title
	        },
	        xAxis: {
	            categories: d.categories
	        },
	        yAxis: {
	            min: 0,
	            title: {
	                text: 'Amount',
	                align: 'high'
	            },
	            labels: {
	                overflow: 'justify'
	            }
	        },
	        plotOptions: {
	            bar: {
	                dataLabels: {
	                    enabled: true
	                }
	            }
	        },

	        credits: {
	            enabled: false
	        },
	        series: d.series
	    });

	    var p = toPieData(d);
	    var chart2 = new Highcharts.Chart({
	        chart: {
	            type: 'bar',
	            renderTo: 'piearea'
	        },
	        title: {
	            text: pieOptions.title
	        },
	        tooltip: {
	            pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
	        },
	        plotOptions: {
	            pie: {
	                allowPointSelect: true,
	                cursor: 'pointer',
	                dataLabels: {
	                    enabled: true,
	                    format: '<b>{point.name}</b>: {point.percentage:.1f} %',
	                    style: {
	                        color: (Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black'
	                    }
	                },
	                showInLegend: true
	            }
	        },
	        credits: {
	            enabled: false
	        },
	        series: [{
	            type: 'pie',
	            name: 'Categories share',
	            data: p
	        }]
	    });
        
	}

	function toPieData(d) {
	    var p = [];
	    for (var i = 0; i < d.categories.length; i++) {
	        p.push([d.categories[i], d.series[0].data[i]]);
	    }
	    return p;
	}
	return {
	    init: init
	};
});