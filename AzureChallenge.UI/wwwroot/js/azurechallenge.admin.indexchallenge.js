$(document).ready(function () {

    $("#btnAddNew").click(function () {
        $(".modal-title").text('Add new challenge');
        $("#btnModalAdd").removeClass('d-none');
        $("#btnModalCopy").addClass('d-none');
        $("#modalWaiting").addClass('d-none');
        $("#modalInput").removeClass('d-none');
        $(".modal-footer").removeClass('d-none');
        $("#modalWaitingMessage").text('Please wait while the new challenge is added...');
        $("#addNewModal").modal('show');
    });

    $("#challengeTable .tableLinkCopy").on('click', function () {
        $("#Id").val($(this).attr('data-challengeId'));
        $(".modal-title").text('Copy challenge');
        $("#btnModalAdd").addClass('d-none');
        $("#btnModalCopy").removeClass('d-none');
        $("#modalWaiting").addClass('d-none');
        $("#modalInput").removeClass('d-none');
        $(".modal-footer").removeClass('d-none');
        $("#modalWaitingMessage").text('Please wait while challenge is copied...');
        $("#addNewModal").modal('show');
    })

    $("#challengeTable .tableLinkDelete").on('click', function (e) {
        if (confirm("Are you sure?")) {
            
            $(".modal-title").text('Delete challenge');
            $("#btnModalAdd").addClass('d-none');
            $("#btnModalCopy").addClass('d-none');
            $("#modalWaiting").removeClass('d-none');
            $("#modalInput").addClass('d-none');
            $(".modal-footer").addClass('d-none');
            $("#modalWaitingMessage").text('Please wait while challenge is deleted...');
            $("#addNewModal").modal('show');

            $.post("/Administration/Challenge/DeleteChallenge?challengeId=" + $(this).attr('data-challengeId'))
                .done(function () {
                    location.reload();
                })
                .fail(function () {
                    window.alert("Could not delete the Challenge, an internal error occured. Please try again later.");
                    location.reload();
                })
        }
        else {
            e.preventDefault();
        }
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

        $("#modalWaiting").removeClass('d-none');
        $("#modalInput").addClass('d-none');
        $(".modal-footer").addClass('d-none');

        var model = {
            Name: Name,
            Description: Description
        };

        $.post("/Administration/Challenge/AddNewChallenge", model)
            .done(function () {
                location.reload(true);
            })
            .fail(function () {
                window.alert("Could not add the new Challenge, an internal error occured. Please try again later.");
                location.reload(true);
            })

    });

    $("#btnModalCopy").click(function () {

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

        $("#modalWaiting").removeClass('d-none');
        $("#modalInput").addClass('d-none');
        $(".modal-footer").addClass('d-none');

        var model = {
            Name: Name,
            Description: Description,
            Id: $("#Id").val()
        };

        $.post("/Administration/Challenge/CopyChallenge", model)
            .done(function () {
                location.reload(true);
            })
            .fail(function () {
                window.alert("Could not copy the Challenge, an internal error occured. Please try again later.");
                location.reload(true);
            })

    });

});