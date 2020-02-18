define(['app'], function (app) {

    function Income(values) {
        values = values || {};
        this.IncomeID = values['IncomeID'] || 0;
        this.AccountID = values['AccountID'] || '';
        
        this.SourceID = values['SourceID'] || '';
        
        this.Amount = values['Amount'] || '';
        this.IncomeDate = values['IncomeDate'];
        this.Description = values['Description'] || '';
        this.Repeat = values['Repeat'] || '0';
        
    }

    Income.prototype.setValues = function (formInput) {
        if (formInput) {
            this.IncomeID = formInput["IncomeID"];
            this.AccountID = formInput["IncomeAccountID"];
            this.SourceID = formInput['IncomeSourceID'] ;
            this.Amount = formInput['IncomeAmount'] ;
            this.IncomeDate = formInput['IncomeDate'] ;
            this.Description = formInput['IncomeDescription'];
            this.Repeat = formInput['IncomeRepeat'];
        }

    };

    Income.prototype.validate = function () {
        var result = true;
        if (!this.AccountID || !this.SourceID || !this.Amount || !this.IncomeDate) {
            result = false;
        }
        return result;
    };

    return Income;
});

