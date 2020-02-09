$(function () {

    
    $('#CategoryID').change(function () {
        var URL = "/Category/SubcatList";
        $.getJSON(URL, {"id":$('#CategoryID').val()}, function (data) {
            var items = '<option></option>';
            $.each(data, function (i, subcat) {
                items += "<option value='" + subcat.Value + "'>" + subcat.Text + "</option>";
                // state.Value cannot contain ' character. We are OK because state.Value = cnt++;
            });
            $('#SubcategoryID').html(items);
        });
    });

});