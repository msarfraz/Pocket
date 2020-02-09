if (!Modernizr.inputtypes.date) {
    $(function () {
        if ($("input[type='date']").length > 0) {

            $("input[type='date']")
                    .datepicker()
                    .get(0)
                    .setAttribute("type", "text");
        }
        
    })
}