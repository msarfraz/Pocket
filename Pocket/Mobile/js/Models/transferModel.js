define(['app'], function (app) {

    function Transfer(values) {

        values = values || {};
        this.TransferID = values['TransferID'] || 0;
        this.TransferDate = values['TransferDate'];
        this.SourceAccount = values['SourceAccount'] || 0;
        this.TargetAccount = values['TargetAccount'] || 0;
        this.Amount = values['Amount'] || 0;
        this.Description = values['Description'] || '';
    }

    Transfer.prototype.setValues = function (formInput) {
        if (formInput) {
            this.TransferID = formInput["TransferID"];
            this.TransferDate = formInput['TransferDate'] ;
            this.SourceAccount = formInput["TransferSourceAccount"];
            this.TargetAccount = formInput['TransferTargetAccount'];
            this.Amount = formInput['TransferAmount'];
            this.Description = formInput['TransferDescription'];
        }

    };

    Transfer.prototype.validate = function () {
        var result = true;
        if (!this.SourceAccount || !this.TargetAccount || !this.Amount || !this.TransferDate) {
            result = false;
        }
        return result;
    };

    return Transfer;
});

