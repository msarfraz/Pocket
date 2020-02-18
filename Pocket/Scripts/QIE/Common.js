/// <reference path="Common.js" />
function PostData(url, data, successfunc, isasync) {
    data["__RequestVerificationToken"] = jQuery('input[name=__RequestVerificationToken]').val();
    if (typeof isasync === "undefined" || isasync == null)
        isasync = true;

    $.ajax({
        async: isasync,
        type: "POST",
        url: url,
        data: data,
        //success: successfunc,
        dataType: "json"
    })
    .done(function (response) {
        if (successfunc)
            successfunc(response);

    })
              .fail(function (response) {
                  alert("Error:" + response);
              })
    ;
    /*
    $.post(
        url,
        data,
        "json")
              .done(function (response) {
                  if(successfunc)
                  successfunc(response);
                  
              })
              .fail(function (response) {
                  alert(data);
              })
    ;
    */
}
function toPieData(d) {
    var p = [];
    for (var i = 0; i < d.categories.length; i++) {
        p.push([d.categories[i], d.series[0].data[i]]);
    }
    return p;
}