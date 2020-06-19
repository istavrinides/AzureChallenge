$(document).ready(function () {
    $('.popover').css('background-color', 'red');

    var paramList = {}
    $("input.paramBagName").each(function () {
        var val = $(this).val()
        paramList[val] = val;
    });

    $("#addToList").on('click', function () {

        var form = $('#form');
        $.validator.unobtrusive.parse(form);
        form.validate();
        $("form").validate().element("#inputNewName");
        $("form").validate().element("#inputNewValue");

        var newName = $("#inputNewName").val();
        var newVal = $("#inputNewValue").val();

        if (!newName || !newVal) {
            return;
        }

        // If the newName already exists, don't add
        if (paramList[newName]) {
            $("#existsModal").modal('show');
            return;
        }

        // Find the last index to insert to
        var lastIndex = -1;
        var found = false;
        $("input.paramBagName").each(function () {
            var thisIndex = parseInt($(this).attr("data-index"));
            if (lastIndex < thisIndex) {
                lastIndex = thisIndex
                found = true;
            }
        });

        // If it's not the first, increment by 1 since we want the next index
        // Or if not found and we only have one
        if (found || (!found && lastIndex === 0))
            lastIndex += 1

        // Get the hidden field container
        var container = $("#hiddenDivs");
        container.append("<input type='hidden' class='paramBag paramBagName' name='ParameterList[" + lastIndex + "].Name' id='ParameterList_" + lastIndex + "__Name' data-index='" + lastIndex + "' value='" + newName + "' />");
        container.append("<input type='hidden' class='paramBag paramBagVal' name='ParameterList[" + lastIndex + "].Value' id='ParameterList_" + lastIndex + "__Value' value='" + newVal + "' data-index='" + lastIndex + "' />");
        container.append("<input type='hidden' class='paramBag paramBagAssigned' name='ParameterList[" + lastIndex + "].AssignedToQuestion' id='ParameterList_" + lastIndex + "__AssignedToQuestion' value='" + 0 + "' data-index='" + lastIndex + "' />");

        $.post("/Administration/Parameter/UpdateParameters", $('form').serialize())
            .done(function () {
                $("#updateModal").hide();
                $('body').removeClass('modal-open');
                $('.modal-backdrop').remove();

                // Add the new values
                // Create a new row on the table
                // Get the body of the table
                var tableBody = $("#parameterTable tbody");
                tableBody.append("<tr data-index='" + lastIndex + "'><td>" + newName + "</td><td>" + newVal + "</td><td class='text-center'><a href='#' class='tableParamDelete' data-name='" + newName + "'><img src='/images/trash-2.svg' class='svg-filter-danger' /></a></td>");
            })
            .fail(function () {
                window.alert("Could not update the parameter list, an internal error occured. Please try again later.");
                location.reload();
            });
        

        

    });

    $("#parameterTable").on('click', '.tableParamDelete', function () {
        // Get the index from the tr
        var indexToDelete = $(this).parent().parent().attr('data-index');
        var keyToDelete = $(this).attr('data-name');

        // Delete the hidden inputs with that data index
        $("input.paramBag[data-index='" + indexToDelete + "'").remove();
        // Delete the row on the table
        $("#parameterTable tbody tr[data-index='" + indexToDelete + "']").remove();
        // Remove the key from our parameter list
        delete paramList[keyToDelete];

        //Re-index all the params
        $("input.paramBagName").each(function () {
            var thisIndex = parseInt($(this).attr("data-index"));
            if (indexToDelete < thisIndex) {
                $(this).attr('id', "ParameterList_" + (thisIndex - 1) + "__Name");
                $(this).attr('name', "ParameterList[" + (thisIndex - 1) + "].Name");
                $(this).attr('data-index', (thisIndex - 1));
            }
        });
        $("input.paramBagVal").each(function () {
            var thisIndex = parseInt($(this).attr("data-index"));
            if (indexToDelete < thisIndex) {
                $(this).attr('id', "ParameterList_" + (thisIndex - 1) + "__Value");
                $(this).attr('name', "ParameterList[" + (thisIndex - 1) + "].Value");
                $(this).attr('data-index', (thisIndex - 1));
            }
        });
        $("input.paramBagAssigned").each(function () {
            var thisIndex = parseInt($(this).attr("data-index"));
            if (indexToDelete < thisIndex) {
                $(this).attr('id', "ParameterList_" + (thisIndex - 1) + "__AssignedToQuestion");
                $(this).attr('name', "ParameterList[" + (thisIndex - 1) + "].AssignedToQuestion");
                $(this).attr('data-index', (thisIndex - 1));
            }
        });
        // Re-index the tr's
        $("#parameterTable tbody tr").each(function () {
            var thisIndex = parseInt($(this).attr("data-index"));
            if (indexToDelete < thisIndex) {
                $(this).attr('data-index', (thisIndex - 1));
            }
        });

        $.post("/Administration/Parameter/UpdateParameters", $('form').serialize())
            .done(function () {
                $("#updateModal").hide();
                $('body').removeClass('modal-open');
                $('.modal-backdrop').remove();
            })
            .fail(function () {
                window.alert("Could not update the parameter list, an internal error occured. Please try again later.");
                location.reload();
            });
    });
});