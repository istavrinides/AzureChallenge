$(document).ready(function () {

    $("input:checkbox.check-filter").change(function () {

        if ($("input:checkbox.check-filter:checked").length === 0) {
            $(".card-div-category").show();
            return;
        }

        var visible = []

        $("input:checkbox.check-filter").each(function (cb) {
            if ($(this).is(':checked')) {
                visible.push($(this).attr('data-category'));
            }
        });

        $(".card-div-category").hide();

        visible.forEach(function (v) {
            $(".card-div-category[data-category='" + v + "']").show();
        });

        if ($("input:checkbox.check-filter:checked").length === 0)
            $(".card-div-category").show();
    });

});