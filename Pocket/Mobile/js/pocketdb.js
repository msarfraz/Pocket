define(function () {
    
    var database;
    var pagesize = 5;
    var f7;
    var ScheduleType = {
        Expense: 1,
        Income: 2,
        Event:3
    }
    function FormatChartData(rows)
    {
        var labels = [];
        var amounts = { name: "Expense", color: "red", data: [] };
        var budgets = { name: "Budget", color: "#7cb5ec", data: [] };
        for (var i = 0; i < rows.length; i++) {
            var r = rows[i];
            labels.push(r["Name"]);
            amounts.data.push(r["TotalAmount"]);
            budgets.data.push(r["TotalBudget"]);
        }

        var data = {
            categories: labels,
            series: [amounts, budgets]
        };
        return data;
    }
    function GetBudgetDates()
    {
        var dt = new Date();
        var day = dt.getDate();
        var wday = dt.getDay();  // week day 0-6
        var mon = dt.getMonth() ;
        var year = dt.getFullYear();

        var labels = {};

        labels["Daily"] = toDateString(dt);
        var alter = (day +1) % 2;
        labels["Alternate"] = toDateString( (new Date()).setDate(day - alter) );
        labels["Weekly"] = toDateString( (new Date()).setDate(day - wday));
        var aweek;
        var weeks = getWeekNumber(dt);
        if (weeks % 2 == 0)
            aweek = 0; // first week
        else
            aweek = 7;

        labels["BiWeekly"] = toDateString((new Date()).setDate(day - wday - aweek));

        labels["Monthly"] = toDateString(new Date(year, mon, 1));
        if(mon == 0 || mon == 2 || mon == 4 || mon == 6 || mon == 8 || mon == 10)
            labels["BiMonthly"] = toDateString(new Date(year, mon ,1));
        else
            labels["BiMonthly"] = toDateString(new Date(year , mon-1, 1));

        if(mon == 0 || mon == 3 || mon == 6 || mon == 9 )
            labels["Quarterly"] = toDateString(new Date(year, mon, 1));
        else if(mon == 1 || mon == 4 || mon == 7 || mon == 10 )
            labels["Quarterly"] = toDateString(new Date(year , mon-1 , 1));
        else
            labels["Quarterly"] = toDateString(new Date(year , mon-2 , 1));
        if(mon <=5)
            labels["BiQuarterly"] = toDateString(new Date(year, 1));
        else
            labels["BiQuarterly"] = toDateString(new Date(year, 6));

        labels["Yearly"] = toDateString(new Date(year, 0));

        return [
            labels["Daily"],
            labels["Alternate"],
            labels["Weekly"],
            labels["BiWeekly"],
            labels["Monthly"],
            labels["BiMonthly"],
            labels["Quarterly"],
            labels["BiQuarterly"],
            labels["Yearly"],
            labels["Monthly"]
        ];
    }
    function GetBudgetDetails(data, callback) {
        var eventbudget, subcatbudget, catparams, evparams;

        if (data.ConstantBudget) { // not monthly
            eventbudget = "select e.EventID, e.Name, e.BudgetAmount Budget, ifnull( sum(ex.Amount) , 0) Amount, ((e.BudgetAmount-ifnull( sum(ex.Amount) , 0))*100)/e.BudgetAmount as Percentage from Events e left join expenses ex on e.EventID = ex.EventID where (e.EventDate > ? or ex.ExpenseDate like ?) and e.Budgeted = 1 and e.EventStatus = 1 group by e.Name, Budget";
            subcatbudget = "select c.CategoryID, c.Name, s.SubcategoryID, s.name as SubcategoryName, s.BudgetAmount SubcatBudget, (select round(ifnull(sum(ex.Amount),0),2) from expenses ex where ex.SubcategoryID = s.SubcategoryID and ex.ExpenseDate >= CASE s.BudgetDuration WHEN 1 THEN ? WHEN 2 THEN ? WHEN 7 THEN ? WHEN 14 THEN ? WHEN 30 THEN ? WHEN 60 THEN ? WHEN 90 THEN ? WHEN 180 THEN ? WHEN 365 THEN ? ELSE ? END) as SubcatAmount from categories c inner join subcategories s on c.CategoryID = s.CategoryID  ORDER BY c.CategoryID, s.SubcategoryID";
            var month = GetCurrentYearMonth();

            if (data.YearMonth)// "2015-05"
                month = data.YearMonth;

            var filter = month + "%";
            var labels = GetBudgetDates();
            catparams = labels;
            evparams = [month, filter];
        }
        else
        {
            eventbudget = "select e.EventID, e.Name, round( BudgetAmount/ (cast( (julianday(eventDate) - julianday('now') ) / CAST(30 as REAL) as int) + 1)  , 2) Budget, ifnull( sum(ex.Amount) , 0) Amount, ((round( BudgetAmount/ (cast( (julianday(eventDate) - julianday('now') ) / CAST(30 as REAL) as int) + 1)  , 2)-ifnull( sum(ex.Amount) , 0))*100)/round( BudgetAmount/ (cast( (julianday(eventDate) - julianday('now') ) / CAST(30 as REAL) as int) + 1)  , 2) as Percentage from Events e left join expenses ex on e.EventID = ex.EventID where (e.EventDate > ? or ex.ExpenseDate like ?) and e.Budgeted = 1 and e.EventStatus = 1 group by e.Name, Budget";
            subcatbudget = "select c.CategoryID, c.Name, s.SubcategoryID, s.name as SubcategoryName, round(ifnull((s.BudgetAmount * (CAST(30 AS REAL) / s.budgetDuration)),0),2) SubcatBudget, (select round(ifnull(sum(ex.Amount),0),2) from expenses ex where ex.SubcategoryID = s.SubcategoryID and ex.ExpenseDate like ?) as SubcatAmount from categories c inner join subcategories s on c.CategoryID = s.CategoryID  ORDER BY c.CategoryID, s.SubcategoryID";
            var month = GetCurrentYearMonth();

            if (data.YearMonth)// "2015-05"
                month = data.YearMonth;

            var filter = month + "%";
            catparams = [filter];
            evparams = [month, filter];
        }
       

        
        getData(subcatbudget, catparams, function (response) {

            var rdata = response.rows;
            var budget = {
                Categories: [],
                AllEvents: {
                    Name: "Events",
                    Budget: 0,
                    Amount: 0,
                    Percentage: 0,
                    Events: []
                }
            };
            var catid = 0;
            var cat;

            for (var i = 0; i < rdata.length; i++) {

                if (rdata[i].CategoryID != catid) {
                    catid = rdata[i].CategoryID;
                    cat = {
                        CategoryID: catid,
                        Name: rdata[i].Name,
                        Budget: 0,
                        Amount: 0,
                        Percentage: 0,
                        Subcategories: []
                    };
                    budget.Categories.push(cat);
                }
                var scat = { SubcategoryID: rdata[i].SubcategoryID, Name: rdata[i].SubcategoryName, Budget: rdata[i].SubcatBudget, Amount: rdata[i].SubcatAmount };
                scat.Percentage = ((scat.Budget - scat.Amount) * 100) / scat.Budget;
                cat.Subcategories.push(scat);
                cat.Amount = Math.round(cat.Amount + scat.Amount);
                cat.Budget = Math.round(cat.Budget + scat.Budget);
                cat.Percentage = ((cat.Budget - cat.Amount) * 100) / cat.Budget;
            }
            getData(eventbudget, evparams, function (res) {
                var r = res.rows;
                var al = budget.AllEvents;

                for (var i = 0; i < r.length; i++) {
                    var ev = { EventID: r[i].EventID, Name: r[i].Name, Budget: r[i].Budget, Amount: r[i].Amount, Percentage: r[i].Percentage };
                    al.Events.push(ev);
                    al.Amount = Math.round(al.Amount + ev.Amount);
                    al.Budget = Math.round(al.Budget + ev.Budget);
                    al.Percentage = ((budget.AllEvents.Budget - budget.AllEvents.Amount) * 100) / budget.AllEvents.Budget;
                }

                response.rows = budget;
                callback(response);
            });
            
        });

    }

    function GetCategoryChartData(data, callback) {
        var q = "select c.CategoryID, c.Name, (select round( sum(BudgetAmount * (CAST(30 AS REAL) / budgetDuration)),2) from subcategories s where s.CategoryID = c.CategoryID) as TotalBudget, (select round(sum(ifnull(Amount,0)),2) from expenses e inner join subcategories s where s.SubcategoryID = e.SubcategoryID and s.CategoryID = c.CategoryID and (e.EventID is null or e.EventID like '') and ExpenseDate like '{filter}') as TotalAmount from categories c ";

        var month = GetCurrentYearMonth();

        if (data.YearMonth)// "2015-05"
            month = data.YearMonth;

        var filter = month + "%";
            
        getData(q.replace("{filter}", filter) , [], function(response){
        
            var data = FormatChartData(response.rows);
            response.rows = data;
            callback(response);

        });

    }
    function GetSubcategoryChartData(data, callback) {
        var q = "select s.SubcategoryID, s.Name, round((BudgetAmount * (CAST(30 AS REAL) / budgetDuration)), 2) as TotalBudget, sum(Amount) TotalAmount from subcategories s left join expenses e on e.SubcategoryID = s.SubcategoryID where (e.EventID is null or e.EventID like '') and ExpenseDate like '{filter}' and CategoryID = ? group by s.SubcategoryID, s.Name ";

        var month = data.YearMonth;

        if (!data.YearMonth)// "2015-05"
            month = GetCurrentYearMonth();

        var filter = month + "%";

        getData(q.replace("{filter}", filter), [data.CategoryID], function (response) {

            var data = FormatChartData(response.rows);
            response.rows = data;
            callback(response);

        });

    }
    function GetEventChartData(data, callback) {
        var q = "select e.EventID, e.Name, e.BudgetAmount as TotalBudget, sum(ex.Amount) TotalAmount from events e inner join expenses ex on ex.EventID = e.EventID where ex.ExpenseDate like '{filter}' or e.EventDate like '{filter}' group by e.EventID, e.Name ";

        var month = GetCurrentYearMonth();

        if (data.YearMonth)// "2015-05"
            month = data.YearMonth;

        var filter = month + "%";

        getData(q.replace("{filter}", filter).replace("{filter}", filter), [], function (response) {

            var data = FormatChartData(response.rows);
            response.rows = data;
            callback(response);

        });
    }
    function GetHomeChartData(data, callback) {
        var eq = "select e.ExpenseDate, strftime('%d', e.ExpenseDate) as Day, sum (Amount) TotalAmount from expenses e where ExpenseDate like '{filter}' group by e.ExpenseDate ";
        var iq = "select i.IncomeDate, strftime('%d', i.IncomeDate) as Day, sum (Amount) TotalAmount from income i where i.IncomeDate like '{filter}' group by i.IncomeDate";
        var bq = "select round(sum(TotalBudget),2) as TotalBudget from (select sum(BudgetAmount * (CAST(30 AS REAL) / budgetDuration)) as TotalBudget from subcategories s union select round( sum(BudgetAmount/ (cast( (julianday(eventDate) - julianday('now') ) / CAST(30 as REAL) as int) + 1) ) , 2) as TotalBudget from events e where e.EventStatus = 1 and Budgeted = 1 ) ";
      //  var eb = "select round( sum(BudgetAmount/ (cast( (julianday(eventDate) - julianday('now') ) / CAST(30 as REAL) as int) + 1) ) , 2) as TotalBudget  from events e where e.EventStatus = 1 and Budgeted = 1";

        var month = GetCurrentYearMonth();
        var dt = new Date();
        if (data.YearMonth)// "2015-05"
            month = data.YearMonth;

        var filter = month + "%";
        
        var d = {
            categories: [],
            series: []
        };

        for (var i = 1; i <= dt.DaysInMonth(); i++) {
            d.categories.push(i);
        }

        getData(eq.replace("{filter}", filter), [], function (er) {

            var expenses = { name: "Expense", color:"red", data: [] };
            var t = 0;
            for (var i = 0; i < er.rows.length; i++) {
                t += er.rows[i]["TotalAmount"];
                expenses.data.push({x:er.rows[i]["Day"], y: t});
            }
            d.series.push(expenses);
            getData(iq.replace("{filter}", filter), [], function (ir) {

                var incomes = { name: "Income", color: "#7cb5ec", data: [] };
                t = 0;
                for (var i = 0; i < ir.rows.length; i++) {
                    t += ir.rows[i]["TotalAmount"];
                    incomes.data.push({ x: ir.rows[i]["Day"], y: t });
                }
                d.series.push(incomes);

                getData(bq, [], function (br) {

                    var budget = { name: "Budget", color:"#90ed7d", data: [] };
                    if (br.rows.length > 0)
                    {
                        budget.data.push({ x: 1, y: br.rows[0]["TotalBudget"] });
                        budget.data.push({ x: dt.DaysInMonth(), y: br.rows[0]["TotalBudget"] });
                    }

                    d.series.push(budget);
                    br.rows = d;
                    callback(br);

                });

            });

        });
        RefreshSchedules();
    }
    function GetBudgetChartData(data, callback) {
        getResults("/Chart/MBudgetChartData", data, callback);
    }
    function GetCalendarData(data, callback) {
        var eq = "select Day, TransactionType, sum(Amount) as TotalAmount from ( select strftime('%d', e.ExpenseDate) as Day, Amount, 'Credit' as TransactionType from expenses e where ExpenseDate like ? union all select strftime('%d', s.SavingDate) as Day, Amount, 'Credit' as TransactionType from savings s where SavingDate like ? union all select strftime('%d', i.IncomeDate) as Day, Amount, 'Debit' as TransactionType from income i where IncomeDate like ? union all select strftime('%d', e.EventDate) as Day, BudgetAmount Amount, 'Event' as TransactionType from events e where EventDate like ?) group by Day, TransactionType ";

        var month = GetCurrentYearMonth();

        if (data.YearMonth)// "2015-05"
            month = data.YearMonth;

        var ym = month + "%";

        getData(eq, [ym, ym, ym, ym], callback);

        
    }
    function GetTransactions(data, callback) {

        var q = "select ex.ExpenseID TransactionID, ex.ExpenseDate TransactionDate,a.Name Name, 'Credit' as TransactionType, Amount, 'Expense' as ObjectType, c.Name || ' - ' || s.Name as Description from expenses ex inner join subcategories s on s.SubcategoryID = ex.SubcategoryID inner join categories c on c.CategoryID = s.CategoryID inner join accounts a on a.AccountID = ex.AccountID where ex.ExpenseDate like ? " +
                " union all select i.IncomeID, i.IncomeDate,a.Name Name, 'Debit' as TransactionType, Amount, 'Income' as ObjectType, s.Name as Description from income i inner join sources s on s.sourceid = i.sourceid inner join accounts a on a.AccountID = i.AccountID where i.IncomeDate like ?  " +
                "union all select t.TransferID, t.TransferDate,acs.Name Name, 'DebitCredit' as TransactionType, Amount, 'Transfer' as ObjectType, 'From ' || acs.Name || ' To ' || act.Name as Description from accounttransfers t inner join accounts acs on acs.AccountID = t.SourceAccount inner join accounts act on act.AccountID = t.TargetAccount where t.TransferDate like ? " +
                "union all select s.SavingID, s.SavingDate,a.Name Name, 'Credit' as TransactionType, Amount, 'Saving' as ObjectType, 'Saving for ' || t.Name as Description from Savings s inner join targets t on s.TargetID = t.TargetID inner join accounts a on a.AccountID = s.AccountID where s.SavingDate like ? " + 
                "union all select e.EventID, e.EventDate, rp.text Name, 'DebitCredit' as TransactionType, BudgetAmount Amount, 'Event' as ObjectType, e.Name as Description from Events e inner join repeatPattern rp on rp.id = e.BudgetDuration where e.EventDate like ? order by TransactionDate ";

        var accountid = data["AccountID"];
        var from = data["TransactionDate"];
        from = from + "%";
        var args =     [from,  //expenses
                        from,  //income
                        from,  // transfer
                        from, // savings
                        from, // event
        ];
        getData(q, args, callback);

    }
    function GetAccountStatement(data, callback) {

        var accountid = data["AccountID"];
        var from = data["From"];
        var to = data["To"];

        var total = 0;
        var offset = 0;
        var limit = pagesize;

        if (data.rows)
            limit = data.rows;

        if (data.page)
            offset = (data.page - 1) * limit;

        // balance is not correct for second page
        var bq = "SELECT a.AccountID, a.name, a.InitialAmount ,at.text as AccountTypeText, 'true' as Editable, a.InitialAmount + (select ifnull(sum(amount),0) from income i where i.accountID = a.AccountID and i.Incomedate < 'BALANCE_DATE') +  (select ifnull(sum(amount),0) from accountTransfers t where t.targetAccount = a.AccountID and t.TransferDate < 'BALANCE_DATE') - (select ifnull(sum(amount),0) from expenses e where e.AccountID = a.AccountID and e.ExpenseDate < 'BALANCE_DATE')  - (select ifnull(sum(amount),0) from accountTransfers t where t.sourceAccount = a.accountid  and t.TransferDate < 'BALANCE_DATE')  - (select ifnull(sum(amount),0) from savings sv where sv.AccountID = a.accountid and sv.SavingDate < 'BALANCE_DATE')  + (" +
            "select ifnull(sum(amount),0) from "+
            " (select amount,Incomedate TransactionDate from income i where i.accountID = a.AccountID and i.Incomedate >= 'BALANCE_DATE' union all select amount, TransferDate from accountTransfers t where t.targetAccount = a.AccountID and t.TransferDate >= 'BALANCE_DATE' union all select -amount, ExpenseDate from expenses e where e.AccountID = a.AccountID and e.ExpenseDate >= 'BALANCE_DATE' union all select -amount, TransferDate from accountTransfers t where t.sourceAccount = a.accountid  and t.TransferDate >= 'BALANCE_DATE' union all select -amount,SavingDate from savings sv where sv.AccountID = a.accountid and sv.SavingDate >= 'BALANCE_DATE'" +
            "order by TransactionDate limit " + offset + " )) " +
            " CurrentAmount FROM accounts a inner join accounttype at on at.id = a.accounttype where a.AccountID = ?";
        bq = replaceAll("BALANCE_DATE", from, bq);

        getData(bq, [accountid], function (response) {
            var balance = 0;
            if (response.rows.length > 0)
                balance = response.rows[0]['CurrentAmount'];
            var q = "select i.IncomeID TransactionID, i.IncomeDate TransactionDate, 'Debit' as TransactionType, Amount, 'Income' as ObjectType, s.Name as Description from income i inner join sources s on s.sourceid = i.sourceid where i.AccountID = ? and i.IncomeDate >= ? and i.IncomeDate <= ? union all select t.TransferID TransactionID, t.TransferDate, 'Debit' as TransactionType, Amount, 'Transfer' as ObjectType, 'Transfer to ' || a.Name as Description from accounttransfers t inner join accounts a on a.AccountID = t.TargetAccount where t.TargetAccount = ? and t.TransferDate >= ? and t.TransferDate <= ? union all select ex.ExpenseID TransactionID, ex.ExpenseDate TransactionDate, 'Credit' as TransactionType, Amount, 'Expense' as ObjectType, c.Name || ' - ' || s.Name as Description from expenses ex inner join subcategories s on s.SubcategoryID = ex.SubcategoryID inner join categories c on c.CategoryID = s.CategoryID where ex.AccountID = ? and ex.ExpenseDate >= ? and ex.ExpenseDate <= ? union all select t.TransferID  TransactionID, t.TransferDate, 'Credit' as TransactionType, Amount, 'Transfer' as ObjectType, 'Transfer from ' || a.Name as Description from accounttransfers t inner join accounts a on a.AccountID = t.SourceAccount where t.SourceAccount = ? and t.TransferDate >= ? and t.TransferDate <= ? union all select s.SavingID  TransactionID, s.SavingDate, 'Credit' as TransactionType, Amount, 'Saving' as ObjectType, 'Saving for ' || t.Name as Description from Savings s inner join targets t on s.TargetID = t.TargetID where s.AccountID = ? and s.SavingDate >= ? and s.SavingDate <= ?  order by TransactionDate ";
            var countQuery = "select count(1) from ( " + q + " )";

            data.args = [accountid, from, to, //expenses
                        accountid, from, to, // trout
                        accountid, from, to, //income
                        accountid, from, to, // trin
                        accountid, from, to // savings
            ];
            data.cargs = data.args; // count query args

            getPagedData(countQuery, q + "  limit {limit} offset {offset} ", data, function (transactions) {
                for(var i=0; i< transactions.rows.length; i++)
                {
                    var t = transactions.rows[i];
                    var ttype = t["TransactionType"];
                    if (ttype == 'Credit') {
                        balance = balance - t["Amount"];
                    }
                    else if (ttype == 'Debit') {
                        balance = balance + t["Amount"];
                    }

                    t["Balance"] = round( balance, 2);
                    t["UserName"] = '';
                }
                callback(transactions);
            });

        });

    }
    function GetEventReport(data, callback) {
        GetAdvanceReport(data, callback);
    }
    function GetAdvanceReport(data, callback) {
        // no difference with GetExpenses except a join with cats & scats

        var mainfilter = getExpenseFilter(data, "t");
        var sumfilter = getExpenseFilter(data, "e");
        var datefilter = getExpenseFilter(data, "d");

        var countQuery = "select count(1) as Total from (select distinct expenseDate from expenses t inner join subcategories on t.SubcategoryID = subcategories.SubcategoryID inner join categories on subcategories.CategoryID = categories.CategoryID {filter} )";
        countQuery = countQuery.replace("{filter}", mainfilter);

        var query = "select t.*,a.Name as AccountText,categories.name as CategoryText, subcategories.name as SubcategoryText, '' as UserName, '1' as Editable, (SELECT sum(e.amount) FROM expenses e inner join subcategories on e.SubcategoryID = subcategories.SubcategoryID inner join categories on subcategories.CategoryID = categories.CategoryID where e.expenseDate = t.expenseDate {sumfilter} group by expenseDate  ) as TotalAmount from expenses t inner join accounts a on a.AccountID = t.AccountID inner join subcategories on subcategories.SubcategoryID = t.SubcategoryID inner join categories on categories.CategoryID = subcategories.CategoryID where expenseDate in ( SELECT expenseDate FROM expenses d inner join subcategories on d.SubcategoryID = subcategories.SubcategoryID inner join categories on subcategories.CategoryID = categories.CategoryID {datefilter} group by d.expenseDate ORDER BY d.ExpenseDate desc limit {limit} offset {offset} ) {mainfilter} order by t.ExpenseDate Desc";

        query = query.replace("{datefilter}", datefilter);
        query = query.replace("{mainfilter}", mainfilter.replace("where", "and")); // replaces staring where with and
        query = query.replace("{sumfilter}", sumfilter.replace("where", "and")); // replaces second instance staring where with and
        data.args = [];
        getPagedData(countQuery, query, data, callback, groupExpenses);
    }

    function LoadTargets(data, callback) {
        getData("Select t.*, t.TargetDate as TargetDateText, s.text as StatusText, '0' as Outlook, rp.text as BudgetDurationText from targets t inner join TargetStatus s on s.id = t.Status inner join RepeatPattern rp on rp.id = t.BudgetDuration order by TargetDate desc ", [], callback);

    }
    function GetTargets(data, callback) {
        var countQuery = "select count(1) as Total from targets";
        var query = "Select t.*, t.TargetDate as TargetDateText, ifnull((select sum(amount) from savings ss where ss.TargetID = t.TargetID),0)+InitialAmount as SavingAmount from targets t order by TargetDate desc limit {limit} offset {offset}";
        data.args = [];
        getPagedData(countQuery, query, data, callback);
    }
    function GetUpcomingTargets(data, callback) {
        getData("Select t.*, t.TargetDate as TargetDateText, ifnull((select sum(amount) from savings ss where ss.TargetID = t.TargetID),0)+InitialAmount as SavingAmount from targets t order by TargetDate desc limit 5", [], callback);

    }

    function GetTargetByID(data, callback) {
        getData("Select t.*, t.TargetDate as TargetDateText, t.CreatedDate as CreatedDateText, s.text as StatusText, (select sum(amount) from savings ss where ss.TargetID = t.TargetID) as SavingAmount from targets t inner join TargetStatus s on s.id = t.Status where t.TargetID = ? order by TargetDate desc", [data.TargetID], callback);

    }
    function SaveTarget(target, saveCallback) {
        if (target.TargetID == 0)
            createData("INSERT INTO targets (Name, TargetAmount, InitialAmount,Status,TargetDate, CreatedDate) VALUES ( ?, ? ,?, ?, ?, date())", [target.Name, target.TargetAmount, target.InitialAmount, 1, target.TargetDate], saveCallback);
        else
            updateData("UPDATE targets set Name = ?, TargetAmount=?,InitialAmount=?,Status=?, TargetDate=? where TargetID = ?", [target.Name, target.TargetAmount, target.InitialAmount, target.Status, target.TargetDate, target.TargetID], saveCallback);

    }
    function DeleteTarget(data, callback) {
        deleteData("delete from targets where TargetID = ?", [data.TargetID], callback);
    }
    function GetSavings(data, callback) {
        var countQuery = "select count(1) as Total from savings where TargetID = " + data.TargetID;
        var query = "Select s.*, a.name as AccountText, t.name as TargetText from savings s inner join accounts a on a.AccountID = s.AccountID inner join targets t on t.TargetID = s.TargetID where s.TargetID = ? order by SavingDate desc limit {limit} offset {offset}";
        data.args = [data.TargetID];
        getPagedData(countQuery, query, data, callback);
    }
    function GetSavingByID(data, callback) {
        getData("Select s.*, a.name as AccountText, t.name as TargetText from savings s inner join accounts a on a.AccountID = s.AccountID inner join targets t on t.TargetID = s.TargetID where s.SavingID = ?", [data.SavingID], callback);

    }
    function SaveSaving(saving, saveCallback) {
        if (saving.SavingID == 0)
            createData("INSERT INTO savings (Amount, TargetID, AccountID,SavingDate,Description) VALUES ( ?, ?, ? ,?,?);", [saving.Amount, saving.TargetID, saving.AccountID, saving.SavingDate, saving.Description], saveCallback);
        else
            updateData("UPDATE savings set Amount = ?, AccountID=?,SavingDate=?, Description=? where SavingID = ?", [saving.Amount, saving.AccountID, saving.SavingDate,saving.Description, saving.SavingID], saveCallback);

    }
    function DeleteSaving(data, callback) {
        deleteData("delete from savings where SavingID = ?", [data.SavingID], callback);
    }
    function getExpenseFilter(data, tablePrefix)
    {
        var alias = "";
        if (tablePrefix)
            alias = tablePrefix + ".";

        var filter = "";
        var fitems = [];

        if (data.ExpenseFrom)
            fitems.push( " " + alias + "ExpenseDate >= '" + data.ExpenseFrom + "' ");
        if (data.ExpenseTo)
            fitems.push(" " + alias + "ExpenseDate <= '" + data.ExpenseTo + "' ");
        if (data.FromDate)
            fitems.push(" " + alias + "ExpenseDate >= '" + data.FromDate + "' "); // should be replaced by ExpenseFrom
        if (data.ToDate)
            fitems.push(" " + alias + "ExpenseDate <= '" + data.ToDate + "' "); // should be replaced by ExpenseTo
        if (data.EventID)
            fitems.push(" " + alias + "EventID = " + data.EventID + " ");
        if (data.VendorID)
            fitems.push(" " + alias + "VendorID = " + data.VendorID + " ");
        if (data.PayeeID)
            fitems.push(" " + alias + "PayeeID = " + data.PayeeID + " ");
        if (data.AccountID)
            fitems.push(" " + alias + "AccountID = " + data.AccountID + " ");
        if (data.SubcatID)
            fitems.push(" subcategories.SubcategoryID = " + data.SubcatID + " ");
        if (data.CatID)
            fitems.push(" categories.CategoryID = " + data.CatID + " "); // we'll suppose there will be already be a join with Category c
        
        if(fitems.length > 0)
        {
            filter = fitems.join(" and ");
            filter = " where " + filter;
        }

        return filter;
    }
    function groupExpenses(drows)
    {
        var g = [];
        for (var i = 0; i < drows.length; i++) {
            if (!GetObjectByID(g, "ExpenseDateText", drows[i].ExpenseDate)) {
                g.push({
                    ExpenseDateText: drows[i].ExpenseDate, TotalAmount: drows[i].TotalAmount, Expenses: []
                });
            }

        }
        for (var i = 0; i < drows.length; i++) {
            var d = GetObjectByID(g, "ExpenseDateText", drows[i].ExpenseDate);
            d.Expenses.push(drows[i]);
        }
        return g;
    }
    function GetExpenses(data, callback) {
        var filter = getExpenseFilter(data);
        
        var countQuery = "select count(1) as Total from (select distinct expenseDate from expenses {filter} )";
        countQuery = countQuery.replace("{filter}", filter);

        var query = "select t.*,a.Name as AccountText,c.name as CategoryText, s.name as SubcategoryText, '' as UserName, '1' as Editable, (SELECT sum(e.amount) FROM expenses e where e.expenseDate = t.expenseDate group by expenseDate ) as TotalAmount from expenses t inner join accounts a on a.AccountID = t.AccountID inner join subcategories s on s.SubcategoryID = t.SubcategoryID inner join categories c on c.CategoryID = s.CategoryID where expenseDate in ( SELECT expenseDate FROM expenses {filter} group by expenseDate ORDER BY ExpenseDate desc limit {limit} offset {offset} ) order by ExpenseDate Desc";
        query = query.replace("{filter}", filter);

        data.args = [];
        getPagedData(countQuery, query, data, callback, groupExpenses);
        
    }
    function GetExpenseByID(data, callback) {
        getData("select t.*,a.Name as AccountText,c.CategoryID, c.name as CategoryText, s.name as SubcategoryText, '' as UserName, '1' as Editable, p.Name as PayeeText, e.Name as EventText, rp.text as RepeatText, v.Name as VendorText, '0' as Comments from expenses t inner join accounts a on a.AccountID = t.AccountID inner join subcategories s on s.SubcategoryID = t.SubcategoryID inner join categories c on c.CategoryID = s.CategoryID inner join repeatPattern rp on rp.id = t.Repeat left join vendors v on v.VendorID = t.VendorID left join payees p on p.PayeeID = t.PayeeID left join events e on e.EventID = t.EventID where t.ExpenseID = ?", [data.ExpenseID], callback);

    }
    function DeleteExpense(data, callback) {
        deleteData("delete from expenses where ExpenseID = ?", [data.ExpenseID], callback);
    }
    function SaveExpense(expense, saveCallback) {
        if (expense.ExpenseID == 0)
            createData("INSERT INTO expenses (Description, AccountID, Amount,ExpenseDate, SubcategoryID, Repeat) VALUES ( ?, ?, ?, ?, ?, ?);", [expense.Description, expense.AccountID, expense.Amount, expense.ExpenseDate, expense.SubcategoryID, expense.Repeat], function (response) {
                if (response.success)
                {
                    if (expense.EventID != "" || expense.PayeeID != "" || expense.VendorID != "") // set these fields only if provided. foreign key issue
                    {
                        var exid = response.new_id;
                        var setitems = [];
                        if (expense.PayeeID != "")
                            setitems.push (" PayeeID = " + expense.PayeeID);
                        if (expense.EventID != "")
                            setitems.push (" EventID = " + expense.EventID);
                        if (expense.VendorID != "")
                            setitems.push (" VendorID = " + expense.VendorID);
                        updateData("update expenses set " + setitems.join() + " where ExpenseID = ?", [exid], function (r) { });
                    }
                    
                    AddSchedule(ScheduleType.Expense, response.new_id, expense.ExpenseDate, expense.Repeat, function (r) { saveCallback(response); });
                }
                else
                    saveCallback(response);
            });
        else
        {
            var setitems = []; // set these fields only if provided. foreign key issue
            if (expense.PayeeID != "")
                setitems.push(" PayeeID = " + expense.PayeeID);
            if (expense.EventID != "")
                setitems.push(" EventID = " + expense.EventID);
            if (expense.VendorID != "")
                setitems.push(" VendorID = " + expense.VendorID);
            var f = "";
            if (setitems.length > 0)
                f = setitems.join() + ",";
            updateData("UPDATE expenses set " + f + " Description = ?, AccountID=?,Amount=?,ExpenseDate=?, SubcategoryID=? , Repeat=? where ExpenseID = ?", [expense.Description, expense.AccountID, expense.Amount, expense.ExpenseDate, expense.SubcategoryID, expense.Repeat, expense.ExpenseID], function (response) {
                if (response.success) {
                    AddUpdateSchedule(ScheduleType.Expense, expense.ExpenseID, expense.ExpenseDate, expense.Repeat, function (r) { saveCallback(response); });
                }
                else
                    saveCallback(response);
            });

        }

    }
    function GetNextRunDate(from, rep)
    {
        var repeat = parseInt(rep);
        var dt = new Date(from.getFullYear(), from.getMonth(), from.getDate());
        

        if (repeat < 30) {
            dt.setDate(from.getDate() + repeat);
        }
        else if(repeat == 30)
        {
            dt.setMonth(from.getMonth() + 1);
        }
        else if(repeat == 365)
        {
            dt.setYear(from.getFullYear() + 1);
        }
        else
        {
            dt.setMonth(from.getMonth() + (repeat / 30));
        }
        

        return toDateString(dt);
    }
    function GetActiveNextRunDate(lastRunDate, repeat)
    {
        var dates = [];
        var dtnow = toDateString(new Date());
        var dt = lastRunDate;
        for (var i = 0; i < 1024; i++) { //1024 is just upper limit to avoid recursion when giving very old expense/income/event date.
            dt = GetNextRunDate(new Date(dt), repeat);
            if (dt >= dtnow) {
                return dt;

            }
        }
        return dtnow;

    }
    function AddSchedule(scheduleType, objectID,lastRunDate, repeat, callback)
    {
        if (repeat != 0) {
            var ins = "insert into schedules(LastRunDate, NextRunDate, CreatedDate, ScheduleType, ScheduledObjectID) values ( ?, ?, date(), ?, ?)";
            var nextRun = GetActiveNextRunDate(lastRunDate, repeat);
            if (scheduleType == ScheduleType.Event && lastRunDate > toDateString(new Date())) // event should always be in future so next run date will be event date
                nextRun = lastRunDate;
            createData(ins, [lastRunDate, nextRun, scheduleType, objectID], callback);
        }
        else
            callback(success(0)); // must call the callback
    }
    
    function AddUpdateSchedule(scheduleType, objectID, lastRunDate, repeat, callback) {
        var q = "select * from schedules where ScheduleType = ? and ScheduledObjectID = ?";
        getData(q, [scheduleType, objectID], function (response) {
            if (response.rows.length > 0) {
                var rdata = response.rows[0];
                var ScheduleID = rdata.ScheduleID;

                if (repeat == 0)
                {
                    deleteData("delete from schedules where ScheduleID = ?", [ScheduleID], callback);
                }
                else {
                    var dt = GetActiveNextRunDate(new Date(lastRunDate), repeat);
                    updateData( "update schedules set LastRunDate = ?, NextRunDate = ? where ScheduleID = ?", [lastRunDate, dt, ScheduleID], callback);
                }
                
            }
            else
            {
                AddSchedule(scheduleType, objectID, lastRunDate, repeat, callback);
            }
        });
        
    }
    function getAllRunDates(nextRunDate, repeat)
    {
        var dates = [];
        dates.push(nextRunDate);
        var dtnow = toDateString(new Date());
        var dt = nextRunDate;
        for (var i = 0; i < 100; i++) { // fill all dates from last run date upto today. 100 is just upper limit to avoid too many record insertion when opening app after long time.
            dt = GetNextRunDate(new Date( dt), repeat);
            if (dt <= dtnow)
            {
                dates.push(dt);

            }
            else
                break;
        }
        return dates;
    }
    function RunSchedule(scheduleID, new_rec, update_rec, id, repeat, nextRunDate)
    {
        var dates = getAllRunDates(nextRunDate, repeat);
        for(var i=0; i < dates.length; i++)
        {
            if ((i + 1) == dates.length) // last record to insert
            {
                var lastdate = dates[i];
                createData(new_rec, [lastdate, repeat, id], function (response) { // insert the repeat value in last inserted record so that it got repeated in future
                    updateData("update schedules set LastRunDate = ?, NextRunDate = ?, ScheduledObjectID = ? where ScheduleID = ?", [lastdate, GetActiveNextRunDate(new Date(lastdate), repeat), response.new_id, scheduleID], function (response) { }); //Map the schedule with new record

                });

            }
            else
                createData(new_rec, [dates[i], 0, id], function (response) { });
        }
        updateData(update_rec, [0, id], function (response) { }); // Remove the repeatition from original
    }
    function RefreshSchedules()
    {
        var dt = toDateString( new Date());
        var lastrun = localStorage.getItem('Schedule_Run_Date');
        if(lastrun)
        {
            if (lastrun < dt) // recently run
            {
                RunSchedules();
                localStorage.setItem('Schedule_Run_Date', dt);
            }    
            else
                return;

        }
        else
        {
            RunSchedules();
            localStorage.setItem('Schedule_Run_Date', dt);
        }
    }
    function RunSchedules()
    {
        var new_rec_query, update_rec_query;

        var q = "select s.*, ex.Repeat as Repeat from schedules s inner join expenses ex on ex.ExpenseID = s.ScheduledObjectID  where ScheduleType = 1 and NextRunDate <= date() " +
            "union all select s.*, inc.Repeat as Repeat from schedules s inner join income inc on inc.IncomeID = s.ScheduledObjectID  where ScheduleType = 2 and NextRunDate <= date() " +
            "union all select s.*, ev.BudgetDuration as Repeat from schedules s inner join events ev on ev.EventID = s.ScheduledObjectID  where ScheduleType = 3 and NextRunDate <= date() ";

        getData(q, [], function (response) {
            var rdata = response.rows;
            for (var i = 0; i < rdata.length; i++)
            {
                var row = rdata[i];
                if (row.ScheduleType == ScheduleType.Expense) {
                    new_rec_query = "INSERT INTO expenses (Description, Amount,ExpenseDate,SubcategoryID,AccountID, EventID, PayeeID, VendorID,Repeat) select Description, Amount, ?, SubcategoryID,AccountID, EventID, PayeeID, VendorID, ? from expenses where ExpenseID = ?";
                    update_rec_query = "Update expenses set Repeat = ? where ExpenseID = ?";
                    
                }
                else if (row.ScheduleType == ScheduleType.Income) {
                    new_rec_query = "INSERT INTO income (Description, AccountID, Amount,SourceID,IncomeDate, Repeat) select Description, AccountID, Amount,SourceID, ?, ? from income where IncomeID = ?";
                    update_rec_query = "Update income set Repeat = ? where IncomeID = ?";
                }
                else if (row.ScheduleType == ScheduleType.Event) {
                    new_rec_query = "INSERT INTO events (name, EventDate, BudgetAmount,BudgetDuration, Budgeted, EventStatus) select name, ?, BudgetAmount,?, Budgeted, EventStatus from events where EventID = ?";
                    update_rec_query = "Update events set BudgetDuration = ?, EventStatus = 2 where EventID = ?";
                }

                RunSchedule(row.ScheduleID, new_rec_query, update_rec_query, row.ScheduledObjectID, row.Repeat, row.NextRunDate);
            }
            // expire all old events
            updateData("update events set EventStatus = 2 where EventDate < date()", [], function (response) { });
        });
    }
    function GetIncomes(data, callback) {

        var filter = "";
        if (data.IncomeFrom || data.IncomeTo) {
            if (data.IncomeFrom && data.IncomeTo) {
                filter = " where incomeDate >= '" + data.IncomeFrom + "' and incomeDate <= '" + data.IncomeTo + "'";
            }
            else {
                if (data.IncomeFrom)
                    filter = " where incomeDate >= '" + data.IncomeFrom + "'";
                else
                    filter = " where incomeDate <= '" + data.IncomeTo + "'";
            }
        }
        var countQuery = "select count(incomeDate) as Total from income {filter}";
        countQuery = countQuery.replace("{filter}", filter);

        var query = "Select i.*, a.name as AccountText, s.name as SourceText, rp.text as RepeatText from income i inner join accounts a on i.AccountID = a.AccountID inner join sources s on i.SourceID = s.SourceID inner join RepeatPattern rp on rp.id = i.Repeat {filter} order by incomedate desc limit {limit} offset {offset}";
        query = query.replace("{filter}", filter);

        data.args = [];
        getPagedData(countQuery, query, data, callback);
    }
    function GetIncomeByID(data, callback) {
        getData("Select i.*, a.name as AccountText, s.name as SourceText, rp.text as RepeatText from income i inner join accounts a on i.AccountID = a.AccountID inner join sources s on i.SourceID = s.SourceID inner join RepeatPattern rp on rp.id = i.Repeat where IncomeID = ?", [data.IncomeID], callback);
    }
    function SaveIncome(income, saveCallback) {
        if (income.IncomeID == 0)
            createData("INSERT INTO income (Description, AccountID, Amount,SourceID,IncomeDate, Repeat) VALUES ( ?, ?, ? ,?, ?, ?);", [income.Description, income.AccountID, income.Amount, income.SourceID, income.IncomeDate, income.Repeat], function (response) {
                if (response.success) {
                    AddSchedule(ScheduleType.Income, response.new_id, income.IncomeDate, income.Repeat, function (r) { saveCallback(response); });
                }
                else
                    saveCallback(response);
            });
        else
            updateData("UPDATE income set Description = ?, AccountID=?,Amount=?,SourceID=?,IncomeDate=?, Repeat=? where IncomeID = ?", [income.Description, income.AccountID, income.Amount, income.SourceID, income.IncomeDate, income.Repeat, income.IncomeID], function (response) {
                if (response.success) {
                    AddUpdateSchedule(ScheduleType.Income, income.IncomeID, income.IncomeDate, income.Repeat, function (r) { saveCallback(response); });
                }
                else
                    saveCallback(response);
            });

    }
    function DeleteIncome(data, callback) {
        deleteData("delete from income where IncomeID = ?", [data.IncomeID], callback);
    }

    function GetTransfers(data, callback) {
        var countQuery = "select count(*) as Total from accountTransfers";
        var query = "select at.*,s.Name as SourceAccountText, t.Name as TargetAccountText from accountTransfers at inner join accounts s on at.sourceAccount = s.AccountID inner join accounts t on at.targetAccount = t.AccountID ORDER BY TransferDate ASC limit {limit} offset {offset}";
        data.args = [];
        getPagedData(countQuery, query, data, callback);

      //  getData("select at.*,s.Name as SourceAccountText, t.Name as TargetAccountText from accountTransfers at inner join accounts s on at.sourceAccount = s.AccountID inner join accounts t on at.targetAccount = t.AccountID ORDER BY TransferDate ASC", [], callback);
    }
    function GetTransferByID(data, callback) {
        getData("select at.*,s.Name as SourceAccountText, t.Name as TargetAccountText from accountTransfers at inner join accounts s on at.sourceAccount = s.AccountID inner join accounts t on at.targetAccount = t.AccountID where at.TransferID = ?", [data.TransferID], callback);
    }
    function DeleteTransfer(data, callback) {
        deleteData("delete from accountTransfers where TransferID = ?", [data.TransferID], callback);
    }
    function SaveTransfer(transfer, saveCallback) {
        if (transfer.TransferID == 0)
            createData("INSERT INTO accountTransfers (TransferDate, SourceAccount, TargetAccount,Amount,Description) VALUES ( ?, ?, ? ,?, ?);", [transfer.TransferDate, transfer.SourceAccount, transfer.TargetAccount, transfer.Amount, transfer.Description], saveCallback);
        else
            updateData("UPDATE accountTransfers set TransferDate = ?, SourceAccount=?,TargetAccount=?,Amount=?,Description=? where TransferID = ?", [transfer.TransferDate, transfer.SourceAccount, transfer.TargetAccount, transfer.Amount, transfer.Description, transfer.TransferID], saveCallback);

    }

    function LoadEvents(data, callback) {
        getData("SELECT EventID, Name FROM events where EventStatus = 1 ORDER BY name ASC", [], function (response) {
            response.rows = [{ UserName: "", Events: response.rows }];
            callback(response);
        });

    }
    function GetEvents(data, callback) {

        var countQuery = "select count(EventDate) as Total from events";
        var query = "SELECT e.*, e.BudgetAmount as Amount, 'true' as Editable, e.EventDate as EventDateText, s.text as EventStatusText, rp.text as BudgetDurationText, yn.text as BudgetedText FROM events e inner join eventstatus s on e.EventStatus = s.id inner join repeatPattern rp on rp.id = e.BudgetDuration inner join YesNoOptions yn on yn.id = e.Budgeted ORDER BY EventDate desc limit {limit} offset {offset}";
        data.args = [];
        getPagedData(countQuery, query, data, callback);


    }
    function GetUpcomingEvents(data, callback) {
        getData("SELECT e.*, e.EventDate as EventDateText FROM events e where e.EventDate > date() and EventStatus = 1 ORDER BY EventDate desc limit 5", [], callback);

    }

    function SaveEvent(event, saveCallback) {
        if (event.EventID == 0)
            createData("INSERT INTO events (name, EventDate, BudgetAmount,BudgetDuration, Budgeted, EventStatus) VALUES ( ?, ?, ? ,?, ?,1);", [event.Name, event.EventDate, event.BudgetAmount, event.BudgetDuration, event.Budgeted], function (response) {
                if (response.success) {
                    AddSchedule(ScheduleType.Event, response.new_id, event.EventDate, event.BudgetDuration, function (r) { saveCallback(response); });
                }
                else
                    saveCallback(response);
            });
        else
            updateData("UPDATE events set Name = ?, EventDate=?,BudgetAmount=?,BudgetDuration=?, Budgeted=?, EventStatus=? where EventID = ?", [event.Name, event.EventDate, event.BudgetAmount, event.BudgetDuration, event.Budgeted,event.EventStatus, event.EventID], function (response) {
                if (response.success) {
                    AddUpdateSchedule(ScheduleType.Event, event.EventID, event.EventDate, event.BudgetDuration, function (r) { saveCallback(response); });
                }
                else
                    saveCallback(response);
            });

    }
    function DeleteEvent(data, callback) {
        deleteData("delete from events where EventID = ?", [data.EventID], callback);
    }
    function GetEventByID(data, callback) {
        getData("SELECT e.*, (select sum(Amount) from expenses where expenses.EventID = e.EventID) as Amount, 'true' as Editable, e.EventDate as EventDateText, s.text as EventStatusText, rp.text as BudgetDurationText, yn.text as BudgetedText FROM events e inner join eventstatus s on e.EventStatus = s.id inner join repeatPattern rp on rp.id = e.BudgetDuration inner join YesNoOptions yn on yn.id = e.Budgeted where EventID = ?", [data.EventID], callback);
    }

    
    function LoadAccounts(data, callback) {
        getData("SELECT AccountID,Name FROM accounts ORDER BY name ASC", [], function (response) {
            response.rows = [{ UserName: "", Accounts: response.rows }];
            callback(response);
        });
    }

    function SaveAccount(account, saveCallback) {
        if (account.AccountID == 0)
            createData("INSERT INTO accounts (name, InitialAmount, AccountType) VALUES ( ?, ?, ? );", [account.Name, account.InitialAmount, account.AccountType], saveCallback);
        else
            updateData("UPDATE accounts set Name = ?, InitialAmount=?,AccountType=? where AccountID = ?", [account.Name, account.InitialAmount, account.AccountType,account.AccountID], saveCallback);
    }
    function DeleteAccount(data, callback) {
        deleteData("delete from accounts where AccountID = ?", [data.AccountID], callback);
    }
    function GetAccounts(data, callback) {
        getData("SELECT a.AccountID, a.name, a.InitialAmount ,at.text as AccountTypeText, 'true' as Editable, a.InitialAmount + (select ifnull(sum(amount),0) from income i where i.accountID = a.AccountID) + (select ifnull(sum(amount),0) from accountTransfers t where t.targetAccount = a.AccountID) - (select ifnull(sum(amount),0) from expenses e where e.AccountID = a.AccountID) - (select ifnull(sum(amount),0) from accountTransfers t where t.sourceAccount = a.accountid) - (select ifnull(sum(amount),0) from savings sv where sv.AccountID = a.accountid) CurrentAmount FROM accounts a inner join accounttype at on at.id = a.accounttype ORDER BY a.name ASC", [], function (response) {
            response.rows = [{ UserName: "", Accounts: response.rows }];
            callback(response);
        });
    }
    function GetAccountByID(data, callback) {
        getData("SELECT a.AccountID, a.name, a.InitialAmount ,a.AccountType AccountType, at.text as AccountTypeText, 'true' as Editable, a.InitialAmount + (select ifnull(sum(amount),0) from income i where i.accountID = a.AccountID) + (select ifnull(sum(amount),0) from accountTransfers t where t.targetAccount = a.AccountID) - (select ifnull(sum(amount),0) from expenses e where e.AccountID = a.AccountID) - (select ifnull(sum(amount),0) from accountTransfers t where t.sourceAccount = a.accountid)  - (select ifnull(sum(amount),0) from savings sv where sv.AccountID = a.accountid) CurrentAmount FROM accounts a inner join accounttype at on at.id = a.accounttype where a.AccountID = ? ORDER BY a.name ASC", [data.AccountID], callback);
    }
    function LoadPayees(data, callback) {
        getData("SELECT * FROM payees ORDER BY name ASC", [], callback);

    }
    function SavePayee(payee, saveCallback) {
        if (payee.PayeeID == 0)
            createData("INSERT INTO payees (name) VALUES ( ? );", [payee.Name], saveCallback);
        else
            updateData( "UPDATE payees set Name = ? where PayeeID = ?", [payee.Name, payee.PayeeID], saveCallback);
        
    }
    function DeletePayee(data, callback) {
        deleteData("delete from payees where PayeeID = ?", [data.PayeeID], callback);
    }

    function LoadVendors(data, callback) {
        getData("SELECT * FROM vendors ORDER BY name ASC", [], callback);

    }
    function SaveVendor(vendor, saveCallback) {
        if (vendor.VendorID == 0)
            createData("INSERT INTO vendors (name) VALUES ( ? );", [vendor.Name], saveCallback);
        else
            updateData("UPDATE vendors set Name = ? where VendorID = ?", [vendor.Name, vendor.VendorID], saveCallback);

    }
    function DeleteVendor(data, callback) {
        deleteData("delete from vendors where VendorID = ?", [data.VendorID], callback);
    }
    
    function LoadIncomeSources(data, callback) {
        getData("SELECT * FROM sources ORDER BY name ASC", [], callback);
    }
    function SaveIncomeSource(incomesource, saveCallback) {
        if (incomesource.SourceID == 0)
            createData("INSERT INTO sources (name) VALUES ( ? );", [incomesource.Name], saveCallback);
        else
            updateData("UPDATE sources set Name = ? where SourceID = ?", [incomesource.Name, incomesource.SourceID], saveCallback);
    }
    function DeleteIncomeSource(data, callback) {
        deleteData("delete from sources where SourceID = ?", [data.SourceID], callback);
    }

    function LoadCategories(data, callback) {
        getData("select c.CategoryID, c.Name, s.SubcategoryID, s.name as SubcategoryName from categories c left join subcategories s on c.CategoryID = s.CategoryID", [], function (response) {
        
            var rdata = response.rows;
            var cats =[];
            for (var i = 0; i < rdata.length; i++) {
                if (!GetObjectByID(cats, "CategoryID", rdata[i].CategoryID))
                    cats.push({ CategoryID: rdata[i].CategoryID, Name: rdata[i].Name, Subcategories: [] });
            }
            for (var i = 0; i < rdata.length; i++) {
                if (rdata[i].SubcategoryID)
                {
                    var cat = GetObjectByID(cats, "CategoryID", rdata[i].CategoryID);
                    cat.Subcategories.push({ SubcategoryID: rdata[i].SubcategoryID, Name: rdata[i].SubcategoryName });
                }
                
            }
            response.rows = [{
                                UserName: "", Categories: cats
                            }];
            callback(response);
        });
    }
    
    function GetCategories(data, callback) {
        getData("SELECT c.CategoryID, c.Name, round(sum(BudgetAmount * (CAST(30 AS REAL) / budgetDuration)),2) as Budget, '1' as Editable, '' as UserName, '0' as IsShared FROM categories c left join subcategories s on c.CategoryID = s.CategoryID group by c.CategoryID, c.Name ORDER BY c.name ASC", [], function (response) {
            response.rows = [{ UserName: "", Categories: response.rows
        }];
            callback(response);
        });
    }
    function SaveCategory(category, saveCallback) {
        if (category.CategoryID == 0)
            createData("INSERT INTO categories (name) VALUES ( ? );", [category.Name], saveCallback);
        else
            updateData("UPDATE categories set Name = ? where CategoryID = ?", [category.Name, category.CategoryID], saveCallback);
    }
    function DeleteCategory(data, callback) {
        deleteData("delete from categories where CategoryID = ?", [data.CategoryID], callback);
    }
    function DeleteSubcategory(data, callback) {
        deleteData("delete from subcategories where SubcategoryID = ?", [data.SubcategoryID], callback);
    }
    function SaveSubcategory(subcategory, saveCallback) {
        if (subcategory.SubcategoryID == 0)
            createData("INSERT INTO subcategories (name, BudgetAmount, CategoryID, BudgetDuration) VALUES ( ?, ?, ? , ? );", [subcategory.Name, subcategory.BudgetAmount, subcategory.CategoryID, subcategory.BudgetDuration], saveCallback);
        else
            updateData("UPDATE subcategories set Name = ?, BudgetAmount = ?, BudgetDuration = ? where SubcategoryID = ?", [subcategory.Name, subcategory.BudgetAmount, subcategory.BudgetDuration, subcategory.SubcategoryID], saveCallback);

    }
    function GetCategoryByID(data, callback) {
        getData("select c.CategoryID, c.Name, s.SubcategoryID, s.name as SubcategoryName, s.BudgetAmount, s.BudgetDuration, rp.text as BudgetDurationText, (select round(sum(BudgetAmount * (CAST(30 AS REAL) / budgetDuration)),2) from subcategories sc where sc.CategoryID = c.CategoryID) as Budget from categories c left join subcategories s on c.CategoryID = s.CategoryID left join repeatPattern rp on rp.id = BudgetDuration where c.CategoryID = ? ORDER BY s.name ASC", [data.CategoryID], function (response) {

            var rdata = response.rows;
            var cats = [];
            for (var i = 0; i < rdata.length; i++) {
                if (!GetObjectByID(cats, "CategoryID", rdata[i].CategoryID))
                    cats.push({ CategoryID: rdata[i].CategoryID, Name: rdata[i].Name, Editable: 1, IsShared: 0, Budget: rdata[i].Budget, Subcategories: [] });
            }
            for (var i = 0; i < rdata.length; i++) {
                if (rdata[i].SubcategoryID)
                {
                    var cat = GetObjectByID(cats, "CategoryID", rdata[i].CategoryID);
                    cat.Subcategories.push({
                        SubcategoryID: rdata[i].SubcategoryID, Name: rdata[i].SubcategoryName, Budget: rdata[i].BudgetAmount,
                        BudgetDuration: rdata[i].BudgetDuration, BudgetDurationText: rdata[i].BudgetDurationText
                    });
                }
                
            }
            response.rows = cats;
            callback(response);
        });
    }
    
    function createSchema(transaction, query) {
        try {
            transaction.executeSql(
            query,
            [],
            function (transaction, results) {
                console.log("createSchema.success:");
            },
            function (transaction, err) {
                console.error("createSchema.error:" + err.message);
            }
           );
        }
        catch (err) {
            console.error("createSchema.error:" + err);
        }
    }

    function createData(query, data, callback) {
        console.log(query);
        console.log(data);

        database.transaction(
             function (transaction) {
                 try {
                     transaction.executeSql(
                     query,
                     data,
                     function (transaction, results) {
                         console.log("createData.success:");
                         callback(success(results.insertId));
                     },
                     function (transaction, err) {
                        // f7.alert("createData.error:" + err.message);
                         callback(failure());
                     }
                    );
                 }
                 catch (err) {
                  //   f7.alert("createData.error:" + err);
                     callback(failure());
                 }
             }
         );
    }
    function updateData(query, data, callback) {
        console.log(query);
        console.log(data);
        database.transaction(
             function (transaction) {
                 try {
                     transaction.executeSql(
                     (
                         query
                     ),
                     data,
                     function (transaction, results) {
                         console.log("updateData.success:");
                         callback(success(data[0]));
                     },
                     function (transaction, err) {
                         console.error("updateData.error:" + err.message);
                         callback(failure(data[0]));
                     }
                    );
                 }
                 catch (err) {
                     console.error("updateData.error:" + err);
                     callback(failure(data[0]));
                 }
             }
         );
    }
    function getPagedData(countQuery, query, data,  callback, formatCallback) {

        var total = 0;
        var offset = 0;
        var limit = pagesize;

        if (data.rows)
            limit = data.rows;

        if (data.page)
            offset = (data.page - 1) * limit;
        if (!data.cargs) // count query args
            data.cargs = [];

        getData(countQuery, data.cargs, function (response) {

            total = response.rows[0].Total;
            if (offset > total)
                offset = total;

            query = query.replace("{limit}", limit).replace("{offset}", offset);

            getData(query, data.args, function (r) {
                response.total = Math.floor(total / limit) + 1;
                response.page = Math.floor(offset / limit) + 1;// 1 based index;
                
                var g = r.rows;

                if (formatCallback) 
                    g = formatCallback(r.rows);

                    response.rows = g;
                callback(response);
            });
        });

    }
    function getData(query, data, callback) {
        console.log(query);
        console.log(data);
        database.transaction(
             function (transaction) {
                 try {
                     transaction.executeSql(
                     (
                         query
                     ),
                     data,
                     function (transaction, results) {
                         console.log("getData.success:");
                         callback(resultset(results));
                     },
                     function (transaction, err) {
                         console.error("getData.error:" + err.message);
                         callback(failure());
                     }
                    );
                 }
                 catch (err) {
                     console.error("getData.error:" + err);
                     callback(failure());
                 }
             }
         );
    }
    
    function deleteData(query, data, callback) {
        console.log(query);
        database.transaction(
             function (transaction) {
                 try {
                         transaction.executeSql(
                         query
                         ,
                         data,
                         function (transaction, results) {
                             console.log("deleteData.success:");
                             callback(success());
                         },
                         function (transaction, err) {
                            // f7.alert("deleteData.error:" + err.message);
                             callback(delfailure());
                         }
                         );
                 }
                 catch (err) {
                    // f7.alert("deleteData.error:" + err);
                     callback(delfailure());
                 }
             }
             );
    }

    function resultset(results) {
        var r = [];
        for (var i = 0; i < results.rows.length; i++) {
            r.push(results.rows.item(i));
        }

        return {
            success: true,
            rows: r
        };
    }
    function success(NewID) {
        return {
            success: true,
            message: "Success",
            new_id: NewID
        };
    }
    function failure(NewID) {
        return {
            success: false,
            message: "Model statis is invalid.",
            new_id: NewID
        };
    }
    function delfailure(NewID) {
        return {
            success: false,
            message: "Unable to delete the record. Other objects might have dependency on it.",
            new_id: NewID
        };
    }
    
    function initialize(app, init_complete) {
        f7 = app; 
        var databaseOptions = {
            fileName: "PocketDB",
            version: "1.0",
            displayName: "Pocket Database",
            maxSize: 10240
        };
        
        if (window.sqlitePlugin !== undefined) {
            database = window.sqlitePlugin.openDatabase({ name: databaseOptions.fileName, location: 1 });

            // demonstrate PRAGMA:
            database.executeSql("PRAGMA foreign_keys = ON;", [], function (res) {
                console.log("PRAGMA res: " + JSON.stringify(res));
            });
        } else {
            // For debugging in simulator fallback to native SQL Lite
            //f7.alert("SQL Lite plugin not found.");
            database = openDatabase(
            databaseOptions.fileName,
            databaseOptions.version,
            databaseOptions.displayName,
            databaseOptions.maxSize);
        }
        if (localStorage.getItem('PDB_Initialized'))
        {
            RefreshSchedules();
            init_complete();
            return;
        }

        database.transaction(
            function (transaction) {

               // transaction.executeSql("PRAGMA foreign_keys = ON;");
                // Create our tables if it doesn't exist.
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS payees (" +
                        "PayeeID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Name TEXT NOT NULL," +
                        "DateModified TEXT DEFAULT CURRENT_TIMESTAMP" +
                    ");"
                );
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS vendors (" +
                        "VendorID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Name TEXT NOT NULL," +
                        "DateModified TEXT DEFAULT CURRENT_TIMESTAMP" +
                    ");"
                );
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS sources (" +
                        "SourceID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Name TEXT NOT NULL," +
                        "DateModified TEXT DEFAULT CURRENT_TIMESTAMP" +
                    ");"
                );

                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS accounts (" +
                        "AccountID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Name TEXT NOT NULL," +
                        "InitialAmount REAL NOT NULL," +
                        "CurrentAmount REAL NULL," +
                        "AccountType INTEGER NOT NULL," +
                        "DateModified TEXT DEFAULT CURRENT_TIMESTAMP" +
                    ");"
                );
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS categories (" +
                        "CategoryID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Name TEXT NOT NULL," +
                        "DateModified TEXT DEFAULT CURRENT_TIMESTAMP" +
                    ");"
                );
                
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS events (" +
                        "EventID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Name TEXT NOT NULL," +
                        "EventDate TEXT NOT NULL," +
                        "EventStatus INTEGER NOT NULL," +
                        "BudgetAmount REAL NOT NULL," +
                        "BudgetDuration INTEGER NOT NULL," +
                        "Budgeted INTEGER NOT NULL," +
                        "DateModified TEXT DEFAULT CURRENT_TIMESTAMP" +
                    ");"
                );
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS subcategories (" +
                        "SubcategoryID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Name TEXT NOT NULL," +
                        "BudgetAmount REAL NOT NULL," +
                        "CategoryID INTEGER NOT NULL REFERENCES categories(CategoryID) ON UPDATE CASCADE ON DELETE CASCADE," +
                        "BudgetDuration INTEGER NOT NULL," +
                        "DateModified TEXT DEFAULT CURRENT_TIMESTAMP" +
                    ");"
                );
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS expenses (" +
                        "ExpenseID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Description TEXT NULL," +
                        "AccountID INTEGER NOT NULL REFERENCES accounts(AccountID) ON UPDATE CASCADE," +
                        "Amount REAL NOT NULL," +
                        "EventID INTEGER NULL REFERENCES events(EventID) ON UPDATE CASCADE ON DELETE SET NULL," +
                        "ExpenseDate TEXT NOT NULL," +
                        "PayeeID INTEGER NULL REFERENCES payees(PayeeID) ON UPDATE CASCADE ON DELETE SET NULL," +
                        "SubcategoryID INTEGER NOT NULL REFERENCES subcategories(SubcategoryID) ON UPDATE CASCADE," +
                        "VendorID INTEGER NULL REFERENCES vendors(VendorID) ON UPDATE CASCADE ON DELETE SET NULL," +
                        "Repeat INTEGER NOT NULL DEFAULT 0," +
                        "DateModified TEXT DEFAULT CURRENT_TIMESTAMP" +
                    ");"
                );
                
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS income (" +
                        "IncomeID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Description TEXT NOT NULL," +
                        "AccountID INTEGER NOT NULL REFERENCES accounts(AccountID) ON UPDATE CASCADE," +
                        "Amount REAL NOT NULL," +
                        "SourceID INTEGER NOT NULL REFERENCES sources(SourceID) ON UPDATE CASCADE," +
                        "IncomeDate TEXT NOT NULL," +
                        "Repeat INTEGER NOT NULL DEFAULT 0," +
                        "DateModified TEXT DEFAULT CURRENT_TIMESTAMP" +
                    ");"
                );
                
                
                
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS targets (" +
                        "TargetID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Name TEXT NOT NULL," +
                        "TargetAmount REAL NOT NULL," +
                        "InitialAmount REAL NOT NULL DEFAULT 0," +
                        "Status INTEGER NOT NULL," +
                        "TargetDate TEXT NOT NULL," +
                        "CreatedDate TEXT NOT NULL," +
                        "DateModified TEXT DEFAULT CURRENT_TIMESTAMP" +
                    ");"
                );
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS savings (" +
                        "SavingID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "Amount REAL NOT NULL," +
                        "TargetID INTEGER NOT NULL REFERENCES targets(TargetID) ON UPDATE CASCADE ON DELETE CASCADE," +
                        "AccountID INTEGER NOT NULL REFERENCES accounts(AccountID) ON UPDATE CASCADE," +
                        "SavingDate TEXT NOT NULL," +
                        "Description TEXT ," +
                        "DateModified TEXT DEFAULT CURRENT_TIMESTAMP" +
                    ");"
                );
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS accountTransfers (" +
                        "TransferID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "TransferDate TEXT NOT NULL," +
                        "SourceAccount INTEGER NOT NULL REFERENCES accounts(AccountID) ON UPDATE CASCADE," +
                        "TargetAccount INTEGER NOT NULL REFERENCES accounts(AccountID) ON UPDATE CASCADE," +
                        "Amount REAL NOT NULL," +
                        "Description TEXT ," +
                        "DateModified TEXT DEFAULT CURRENT_TIMESTAMP" +
                    ");"
                );
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS schedules (" +
                        "ScheduleID INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                        "LastRunDate TEXT NOT NULL," +
                        "NextRunDate TEXT NOT NULL," +
                        "CreatedDate TEXT NOT NULL," +
                        "ScheduleType INTEGER NOT NULL ," +
                        "ScheduledObjectID INTEGER NOT NULL" +
                    ");"
                );
                
                // Lookup Tables

                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS accountType (" +
                        "ID INTEGER NOT NULL PRIMARY KEY," +
                        "text TEXT NOT NULL" +
                    ");"
                );

                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS RepeatPattern (" +
                        "ID INTEGER NOT NULL PRIMARY KEY," +
                        "text TEXT NOT NULL" +
                    ");"
                );
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS TargetStatus (" +
                        "ID INTEGER NOT NULL PRIMARY KEY," +
                        "text TEXT NOT NULL" +
                    ");"
                );
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS YesNoOptions (" +
                        "ID INTEGER NOT NULL PRIMARY KEY," +
                        "text TEXT NOT NULL" +
                    ");"
                );
                createSchema(transaction,
                    "CREATE TABLE IF NOT EXISTS EventStatus (" +
                        "ID INTEGER NOT NULL PRIMARY KEY," +
                        "text TEXT NOT NULL" +
                    ");"
                );

                // Populate Tables

                /* Account Type
                    Debit =1,
                    Credit =2,
                    Saving=3 */
                createSchema(transaction,
                                    "INSERT INTO accountType(id,text)" +
                                        "SELECT 1, 'Debit'" +
                                    "WHERE NOT EXISTS(SELECT * FROM accountType WHERE id = 1);"

                                );
                createSchema(transaction,
                                    "INSERT INTO accountType(id,text)" +
                                        "SELECT 2, 'Credit'" +
                                    "WHERE NOT EXISTS(SELECT * FROM accountType WHERE id = 2);"

                                );
                createSchema(transaction,
                                    "INSERT INTO accountType(id,text)" +
                                        "SELECT 3, 'Saving'" +
                                    "WHERE NOT EXISTS(SELECT * FROM accountType WHERE id = 3);"

                                );
                // Repeat Pattern
                /*
                Daily = 1,
                Alternate_Days = 2,
                Weekly = 7,
                Bi_Weekly = 14,
                Monthly = 30,
                Bi_Monthly = 60,
                Quarterly = 90,
                Bi_Quarterly = 180,
                Yearly = 365
                */
                createSchema(transaction,
                                        "INSERT INTO RepeatPattern(id,text)" +
                                            "SELECT 0, 'None'" +
                                        "WHERE NOT EXISTS(SELECT * FROM RepeatPattern WHERE id = 0);"

                                    );
                createSchema(transaction,
                                                        "INSERT INTO RepeatPattern(id,text)" +
                                                            "SELECT 1, 'Daily'" +
                                                        "WHERE NOT EXISTS(SELECT * FROM RepeatPattern WHERE id = 1);"

                                                    );
                createSchema(transaction,
                                                        "INSERT INTO RepeatPattern(id,text)" +
                                                            "SELECT 2, 'Alternate Days'" +
                                                        "WHERE NOT EXISTS(SELECT * FROM RepeatPattern WHERE id = 2);"

                                                    );
                createSchema(transaction,
                                                        "INSERT INTO RepeatPattern(id,text)" +
                                                            "SELECT 7, 'Weekly'" +
                                                        "WHERE NOT EXISTS(SELECT * FROM RepeatPattern WHERE id = 7);"

                                                    );
                createSchema(transaction,
                                                        "INSERT INTO RepeatPattern(id,text)" +
                                                            "SELECT 14, 'Bi Weekly'" +
                                                        "WHERE NOT EXISTS(SELECT * FROM RepeatPattern WHERE id = 14);"

                                                    );
                createSchema(transaction,
                                                        "INSERT INTO RepeatPattern(id,text)" +
                                                            "SELECT 30, 'Monthly'" +
                                                        "WHERE NOT EXISTS(SELECT * FROM RepeatPattern WHERE id = 30);"

                                                    );
                createSchema(transaction,
                                                        "INSERT INTO RepeatPattern(id,text)" +
                                                            "SELECT 60, 'Bi Monthly'" +
                                                        "WHERE NOT EXISTS(SELECT * FROM RepeatPattern WHERE id = 60);"

                                                    );
                createSchema(transaction,
                                                        "INSERT INTO RepeatPattern(id,text)" +
                                                            "SELECT 90, 'Quarterly'" +
                                                        "WHERE NOT EXISTS(SELECT * FROM RepeatPattern WHERE id = 90);"

                                                    );
                createSchema(transaction,
                                                        "INSERT INTO RepeatPattern(id,text)" +
                                                            "SELECT 180, 'Bi Quarterly'" +
                                                        "WHERE NOT EXISTS(SELECT * FROM RepeatPattern WHERE id = 180);"

                                                    );
                createSchema(transaction,
                                                        "INSERT INTO RepeatPattern(id,text)" +
                                                            "SELECT 365, 'Yearly'" +
                                                        "WHERE NOT EXISTS(SELECT * FROM RepeatPattern WHERE id = 365);"

                                                    );
                /*
 TargetStatus
    {
        Active = 1,
        Achieved = 2,
        Cancelled = 3,
        InActive = 4
    }
    */
                createSchema(transaction,
                                                    "INSERT INTO TargetStatus(id,text)" +
                                                        "SELECT 1, 'Active'" +
                                                    "WHERE NOT EXISTS(SELECT * FROM TargetStatus WHERE id = 1);"

                                                );
                createSchema(transaction,
                                                                    "INSERT INTO TargetStatus(id,text)" +
                                                                        "SELECT 2, 'Achieved'" +
                                                                    "WHERE NOT EXISTS(SELECT * FROM TargetStatus WHERE id = 2);"

                                                                );
                createSchema(transaction,
                                                                    "INSERT INTO TargetStatus(id,text)" +
                                                                        "SELECT 3, 'Cancelled'" +
                                                                    "WHERE NOT EXISTS(SELECT * FROM TargetStatus WHERE id = 3);"

                                                                );
                createSchema(transaction,
                                                                    "INSERT INTO TargetStatus(id,text)" +
                                                                        "SELECT 4, 'InActive'" +
                                                                    "WHERE NOT EXISTS(SELECT * FROM TargetStatus WHERE id = 4);"

                                                                );
                /*
EventStatus
    {
        Active = 1,
        Expired = 2
    }
    
                */
                createSchema(transaction,
                                                                    "INSERT INTO EventStatus(id,text)" +
                                                                        "SELECT 1, 'Active'" +
                                                                    "WHERE NOT EXISTS(SELECT * FROM EventStatus WHERE id = 1);"

                                                                );
                createSchema(transaction,
                                                                    "INSERT INTO EventStatus(id,text)" +
                                                                        "SELECT 2, 'Expired'" +
                                                                    "WHERE NOT EXISTS(SELECT * FROM EventStatus WHERE id = 2);"

                                                                );
                /* YesNoOptions
    {
        No = 0,
        Yes = 1
    }
                */
                createSchema(transaction,
                                                                                    "INSERT INTO YesNoOptions(id,text)" +
                                                                                        "SELECT 0, 'No'" +
                                                                                    "WHERE NOT EXISTS(SELECT * FROM YesNoOptions WHERE id = 0);"

                                                                                );
                createSchema(transaction,
                                                                    "INSERT INTO YesNoOptions(id,text)" +
                                                                        "SELECT 1, 'Yes'" +
                                                                    "WHERE NOT EXISTS(SELECT * FROM YesNoOptions WHERE id = 1);"

                                                                );
                // Default Categories
                createSchema(transaction,"INSERT INTO categories (name) VALUES ('Family')");
                createSchema(transaction,"INSERT INTO subcategories (name, BudgetAmount, CategoryID, BudgetDuration) select 'Entertainment', 100, CategoryID, 30 from categories where name = 'Family'");
                createSchema(transaction,"INSERT INTO subcategories (name, BudgetAmount, CategoryID, BudgetDuration) select 'Kids', 100, CategoryID, 30 from categories where name = 'Family'");
                createSchema(transaction,"INSERT INTO subcategories (name, BudgetAmount, CategoryID, BudgetDuration) select 'Shopping', 100, CategoryID, 30 from categories where name = 'Family'");

                createSchema(transaction,"INSERT INTO categories (name) VALUES ('Auto')");
                createSchema(transaction,"INSERT INTO subcategories (name, BudgetAmount, CategoryID, BudgetDuration) select 'Car Wash', 100, CategoryID, 30 from categories where name = 'Auto'");
                createSchema(transaction,"INSERT INTO subcategories (name, BudgetAmount, CategoryID, BudgetDuration) select 'Public Transport', 100, CategoryID, 30 from categories where name = 'Auto'");
                createSchema(transaction,"INSERT INTO subcategories (name, BudgetAmount, CategoryID, BudgetDuration) select 'Fuel', 100, CategoryID, 30 from categories where name = 'Auto'");
                createSchema(transaction,"INSERT INTO subcategories (name, BudgetAmount, CategoryID, BudgetDuration) select 'Maintenance', 0, CategoryID, 30 from categories where name = 'Auto'");

                createSchema(transaction,"INSERT INTO categories (name) VALUES ('Entertainment')");
                createSchema(transaction,"INSERT INTO subcategories (name, BudgetAmount, CategoryID, BudgetDuration) select 'Gifts', 100, CategoryID, 30 from categories where name = 'Entertainment'");
                createSchema(transaction,"INSERT INTO subcategories (name, BudgetAmount, CategoryID, BudgetDuration) select 'Picnic', 100, CategoryID, 30 from categories where name = 'Entertainment'");
                createSchema(transaction,"INSERT INTO subcategories (name, BudgetAmount, CategoryID, BudgetDuration) select 'Party', 100, CategoryID, 30 from categories where name = 'Entertainment'");
                createSchema(transaction,"INSERT INTO subcategories (name, BudgetAmount, CategoryID, BudgetDuration) select 'Movie', 0, CategoryID, 30 from categories where name = 'Entertainment'");

                createSchema(transaction,"INSERT INTO accounts (name, InitialAmount, AccountType) VALUES ( 'Pocket', 0, 1 )");
                createSchema(transaction,"INSERT INTO accounts (name, InitialAmount, AccountType) VALUES ( 'Credit Card', 0, 2 )");


                createSchema(transaction,"INSERT INTO payees (name) VALUES ( 'Self' )");
                createSchema(transaction,"INSERT INTO payees (name) VALUES ( 'Wife' )");
                createSchema(transaction,"INSERT INTO payees (name) VALUES ( 'Kid' )");

                createSchema(transaction,"INSERT INTO vendors (name) VALUES ( 'Amazon' )");
                createSchema(transaction,"INSERT INTO vendors (name) VALUES ( 'Walmart' )");
                createSchema(transaction,"INSERT INTO vendors (name) VALUES ( 'General' )");
                

                createSchema(transaction,"INSERT INTO sources (name) VALUES ( 'Salary' )");
                createSchema(transaction,"INSERT INTO sources (name) VALUES ( 'Bonus' )");
                createSchema(transaction, "INSERT INTO sources (name) VALUES ( 'Profit' )");

                var dt = new Date();
                dt.setDate(1);
                var d1 = toDateString(dt);
                dt.setDate(15);
                var d2 = toDateString(dt);

                createSchema(transaction, replaceAll("{date}", d1, "INSERT INTO expenses (Description, Amount,ExpenseDate,SubcategoryID,AccountID) select 'dummy expense', 10, '{date}', min(SubcategoryID), min(AccountID) from subcategories, accounts"));
                createSchema(transaction, replaceAll("{date}", d2, "INSERT INTO expenses (Description, Amount,ExpenseDate,SubcategoryID,AccountID) select 'dummy expense', 50, '{date}', min(SubcategoryID), min(AccountID) from subcategories, accounts"));
                createSchema(transaction, replaceAll("{date}", d1, "INSERT INTO income (Description, AccountID, Amount,SourceID,IncomeDate, Repeat) select 'dummy income', min(AccountID), 100 ,min(SourceID), '{date}', 0 from sources,accounts"));
                createSchema(transaction, replaceAll("{date}", d2, "INSERT INTO income (Description, AccountID, Amount,SourceID,IncomeDate, Repeat) select 'dummy income', min(AccountID), 100 ,min(SourceID), '{date}', 0 from sources,accounts"));
                createSchema(transaction,"INSERT INTO events (name, EventDate, BudgetAmount,BudgetDuration, Budgeted, EventStatus) VALUES ( 'Christmas', '2016-12-25', 100 ,0, 1,1)");

                createSchema(transaction, "INSERT INTO targets (Name, TargetAmount, InitialAmount,Status, TargetDate, CreatedDate) VALUES ( 'My Home', 10000 , 0 , 1, '2016-12-31', date() )");
                createSchema(transaction, "INSERT INTO savings (Amount, TargetID, AccountID,SavingDate,Description) select 100, min(TargetID), min(AccountID) ,date(), 'dummy saving' from targets,accounts");
                if (init_complete)
                    init_complete();
            }
        );


        localStorage.setItem('PDB_Initialized', true);
    }

    return {
        initialize: initialize,

        GetBudgetDetails: GetBudgetDetails,

        GetSavings: GetSavings,
        GetSavingByID: GetSavingByID,
        SaveSaving: SaveSaving,
        DeleteSaving: DeleteSaving,

        GetTargets: GetTargets,
        LoadTargets: LoadTargets,
        GetUpcomingTargets: GetUpcomingTargets,
        //   GetTargetList: GetTargetList,
        GetTargetByID: GetTargetByID,
        SaveTarget: SaveTarget,
        DeleteTarget: DeleteTarget,

        LoadPayees: LoadPayees,
        SavePayee: SavePayee,
        DeletePayee: DeletePayee,

        LoadVendors: LoadVendors,
        SaveVendor: SaveVendor,
        DeleteVendor: DeleteVendor,

        LoadIncomeSources: LoadIncomeSources,
        SaveIncomeSource: SaveIncomeSource,
        DeleteIncomeSource: DeleteIncomeSource,

        GetAccounts: GetAccounts,
        LoadAccounts:LoadAccounts,
        // GetAccountGroups: GetAccountGroups,
        GetAccountByID: GetAccountByID,
        SaveAccount: SaveAccount,
        DeleteAccount: DeleteAccount,
        //  GetAccountTypes: GetAccountTypes,

        GetIncomes: GetIncomes,
        GetIncomeByID: GetIncomeByID,
        SaveIncome: SaveIncome,
        DeleteIncome: DeleteIncome,

        GetTransfers: GetTransfers,
        GetTransferByID: GetTransferByID,
        SaveTransfer: SaveTransfer,
        DeleteTransfer: DeleteTransfer,

        GetExpenses: GetExpenses,
        GetExpenseByID: GetExpenseByID,
        SaveExpense: SaveExpense,
        DeleteExpense: DeleteExpense,

        GetCategories: GetCategories,
        LoadCategories: LoadCategories,
        //   GetCategoryGroups: GetCategoryGroups,
        // GetSubcategories: GetSubcategories,
        SaveCategory: SaveCategory,
        GetCategoryByID: GetCategoryByID,
        // GetSubcategoryByID: GetSubcategoryByID,
        SaveSubcategory: SaveSubcategory,
        DeleteCategory: DeleteCategory,
        DeleteSubcategory: DeleteSubcategory,

        GetEvents: GetEvents,
        LoadEvents: LoadEvents,
        GetUpcomingEvents: GetUpcomingEvents,
        //  GetEventGroups: GetEventGroups,
        SaveEvent: SaveEvent,
        GetEventByID: GetEventByID,
        DeleteEvent: DeleteEvent,

        GetCalendarData:GetCalendarData,
        GetTransactions:GetTransactions,
        GetAccountStatement: GetAccountStatement,
        GetEventReport: GetEventReport,
        GetAdvanceReport: GetAdvanceReport,

        GetCategoryChartData: GetCategoryChartData,
        GetSubcategoryChartData: GetSubcategoryChartData,
        GetEventChartData: GetEventChartData,
        GetHomeChartData: GetHomeChartData,
        GetBudgetChartData: GetBudgetChartData
}

});