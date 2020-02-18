function toDateString(d)
{
    var d = new Date(d);
    return d.getFullYear()
        + '-' + pad(d.getMonth() + 1)
        + '-' + pad(d.getDate());
    //.toISOString().split('T')[0];
}

function pad(number) {
    var r = String(number);
    if (r.length === 1) {
        r = '0' + r;
    }
    return r;
}

function getYearMonth(d)
{
    console.log("ym:" + d);
    var dd = new Date(d);
    return pad(dd.getFullYear()) + "-" + pad(dd.getMonth() + 1);
}

function replaceAll(find, replace, str)
{
    var re = new RegExp(find, 'g');
    str = str.replace(re, replace);
    return str;
}

function GetCurrentYearMonth() {
    var today = new Date();
    var mm = today.getMonth() + 1; //January is 0!
    var yyyy = today.getFullYear();
    if (mm < 10) {
        mm = '0' + mm
    }
    return yyyy + "-" + mm;
}

Date.prototype.DaysInMonth = function () {
    var d = new Date(this.getFullYear(), this.getMonth() + 1, 0);
    return d.getDate();
}

function GetObjectByID(objectCollection, field, id) {
    for (var i = 0; i < objectCollection.length; i++) {
        if (objectCollection[i][field] == id) {
            return objectCollection[i];
        }
    }
    return null;
}

function getWeekNumber(d) {
    // Copy date so don't modify original
    d = new Date(+d);
    d.setHours(0, 0, 0);
    // Set to nearest Thursday: current date + 4 - current day number
    // Make Sunday's day number 7
    d.setDate(d.getDate() + 4 - (d.getDay() || 7));
    // Get first day of year
    var yearStart = new Date(d.getFullYear(), 0, 1);
    // Calculate full weeks to nearest Thursday
    var weekNo = Math.ceil((((d - yearStart) / 86400000) + 1) / 7);
    // Return array of year and week number
    return weekNo;
}

function round(value, decimals) {
    return Number(Math.round(value + 'e' + decimals) + 'e-' + decimals);
}
