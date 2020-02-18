define(['app'], function (app) {

    function CalendarItem(day)
    {
        if (!day)
            this.Day = '00';
        else
        {
            if (day < 10)
                this.Day = '0' + day;
            else
                this.Day = day;
        }
        

        this.FullDate = '';
        this.Expense = 0;
        this.Income = 0;
    }
    function Row()
    {
        this.cells = [];
    }
    function CalendarVM(values) {
        var d = new Date(values.year, values.month, 1);
        this.Month = values.month;

        values = values || {};
        this.rows = [];

        var row = new Row();
        // insert dummy values for last months' days in first row
        for (var i = 0; i < d.getDay(); i++) {
            row.cells.push(new CalendarItem());

        }
        for (var i = 1; i < 31; i++) {

            row.cells.push(new CalendarItem(i));
            if (row.cells.length == 7)
            {
                this.rows.push(row);
                row = new Row();
            }
            
        }

        //insert dummy values for next month's days in last row
        var dummy = 1;
        for (var i = row.cells.length; i < 7; i++) {
            row.cells.push(new CalendarItem(dummy++));
        }
        this.rows.push(row);
	}

    return CalendarVM;
});