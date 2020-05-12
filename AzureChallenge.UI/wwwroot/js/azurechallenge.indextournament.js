$(document).ready(function () {

    $("#btnAddNew").click(function () {

        $("#addNewModal").modal('show');

    });

    $("#btnModalAdd").click(function () {

        var form = $('#form');
        $.validator.unobtrusive.parse(form);
        form.validate();
        $("form").validate().element("#Name");
        $("form").validate().element("#Description");

        var Name = $("#Name").val();
        var Description = $("#Description").val();

        if (!Name || !Description) {
            return;
        }

        var model = {
            Name: Name,
            Description: Description
        };

        $.post("/Administration/Tournament/AddNewTournament", model)
            .done(function () {
                location.reload();
            })
            .fail(function () {
                window.alert("Could not add the new Tournament, an internal error occured. Please try again later.");
                location.reload();
            })

    });

});