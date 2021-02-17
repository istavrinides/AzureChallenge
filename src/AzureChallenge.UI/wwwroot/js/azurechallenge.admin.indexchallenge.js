$(document).ready(function () {

    $('[data-toggle="tooltip"]').tooltip()

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
        $("#categorySelector").selectpicker('val', $(this).attr('data-category'));
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

    $("#challengeTable .tableLinkExport").on('click', function (e) {

        $("#modalWaitingExport").addClass('d-none');
        $("#modalInputExport").removeClass('d-none');
        $("#exportModal").modal('show');
        $("#btnExport").attr("data-challengeId", $(this).attr('data-challengeId'));
    });

    $("#btnImportShowModal").on('click', function (e) {

        $("#modalWaitingImport").addClass('d-none');
        $("#modalInputImport").removeClass('d-none');
        $("#importModal").modal('show');
    });

    $("#btnExport").on('click', function (e) {

        $("#exportModal").modal('hide');

        window.open("/Administration/Challenge/ExportChallenge?challengeId=" + $(this).attr('data-challengeId'));

    });

    $("#btnImport").on('click', function (e) {

        $("#modalWaitingImport").removeClass('d-none');
        $("#modalInputImport").addClass('d-none');

        $.ajax({
            type: "POST",
            url: '/Administration/Challenge/ImportChallenge?uri=' + $("#fileSelector").val(),
            statusCode: {
                200: function () {
                    // Delay a bit before refreshing
                    setTimeout(location.reload(true), 500);
                },
                409: function () {
                    alert("Challenge could not be imported - already exists.");
                    $("#importModal").modal('hide');
                },
                500: function () {
                    alert("Challenge could not be imported - an error occured.");
                    $("#importModal").modal('hide');
                }
            }
        });
    });

    $("#btnModalAdd").click(function () {

        var form = $('#form');
        $.validator.unobtrusive.parse(form);
        form.validate();
        $("form").validate().element("#Name");
        $("form").validate().element("#Description");

        var Name = $("#Name").val();
        var Description = $("#Description").val();
        var AzureServiceCategory = $("#categorySelector").val();

        if (!Name || !Description || !AzureServiceCategory) {
            return;
        }

        $("#modalWaiting").removeClass('d-none');
        $("#modalInput").addClass('d-none');
        $(".modal-footer").addClass('d-none');

        var model = {
            Name: Name,
            Description: Description,
            AzureServiceCategory: AzureServiceCategory,
            WelcomeMessage: $("#WelcomeMessage").val(),
            Duration: $("#Duration").val(),
            PrereqLinks: $(".prereqLinksDiv a").map(function () { return this.href }).get(),
            TrackAndDeductPoints: $("#TrackAndDeductPoints").val() === "on"
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
        var AzureServiceCategory = $("#categorySelector").val();

        if (!Name || !Description || !AzureServiceCategory) {
            return;
        }

        $("#modalWaiting").removeClass('d-none');
        $("#modalInput").addClass('d-none');
        $(".modal-footer").addClass('d-none');

        var model = {
            Name: Name,
            Description: Description,
            Id: $("#Id").val(),
            AzureServiceCategory: AzureServiceCategory
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

    $("#btnAddPrerequeLink").click(function () {
        var newLink = $("#inputNewLink").val();

        if (!newLink) {
            $("#inputNewLink").popover('show');
            return;
        }

        // Get the next index
        var newIndex = $("#linksInputGroup input").length;

        $(".prereqLinksDiv")
            .append("<input type='hidden' name='PrereqLinks[" + newIndex + "]' id='PrereqLinks_" + newIndex + "_' data-index='" + newIndex + "' value='" + newLink + "' /> \
                    <span class='border border-info rounded p-1 bg-info m-1' data-index='" + newIndex + "'> \
                        <a href='" + newLink + "' style='color:#fff !important'>" + newLink + "</a>&nbsp; \
                        <span class='badge badge-danger deleteLink' style='cursor:pointer' data-index='" + newIndex + "'>&times</span> \
                    </span>");

        // Clear the input
        $("#inputNewLink").val("");
    });

    $(".prereqLinksDiv").on('click', '.deleteLink', function () {
        // Get the index to delete
        var index = parseInt($(this).attr('data-index'));

        // Delete the input and span with that index
        $(".prereqLinksDiv input[data-index='" + index + "']").remove();
        $(".prereqLinksDiv span.border[data-index='" + index + "']").remove();

        // Re-index the remaining
        $(".prereqLinksDiv input").each(function () {
            var thisIndex = $(this).attr('data-index');
            if (thisIndex > index) {
                $(this).attr('data-index', (thisIndex - 1));
                $(this).attr('id', 'PrereqLinks_' + (thisIndex - 1) + '_');
                $(this).attr('name', 'PrereqLinks[' + (thisIndex - 1) + ']');
            }
        });
        $(".prereqLinksDiv span").each(function () {
            var thisIndex = $(this).attr('data-index');
            if (thisIndex > index) {
                $(this).attr('data-index', (thisIndex - 1));
            }
        });
    });

});