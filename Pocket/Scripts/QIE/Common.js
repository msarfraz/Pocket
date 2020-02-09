/// <reference path="Common.js" />
function PostData(url, data, successfunc) {
    data["__RequestVerificationToken"] = jQuery('input[name=__RequestVerificationToken]').val();
    $.post(
        url,
        data,
        "json")
              .done(function (response) {
                  successfunc(response);
                  
              })
              .fail(function (response) {
                  alert(data);
              })
    ;

}