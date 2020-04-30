$(document).ready(function () {

    var paramList = {}
    $("input.paramBagName").each(function () {
        var val = $(this).val()
        paramList[val] = val;
    });

    $("#addToList").on('click', function () {

        var newName = $("#inputNewName").val();
        var newVal = $("#inputNewValue").val();

        // If the newName already exists, don't add
        if (paramList[newName]) {
            $("#inputNewName").popover('show');
            return;
        }

        // Find the last index to insert to
        var lastIndex = 0;
        var found = false;
        $("input.paramBagName").each(function () {
            var thisIndex = parseInt($(this).attr("data-index"));
            if (lastIndex < thisIndex) {
                lastIndex = thisIndex
                found = true;
            }
        });

        // If it's not the first, increment by 1 since we want the next index
        if (found)
            lastIndex += 1

        // Add the new values.
        // Create a new row on the table
        // Create two new hidden fields for the postback action

        // Get the hidden field container
        var container = $("#hiddenDivs");
        container.append("<input type='hidden' class='paramBag paramBagName' name='ParameterList[" + lastIndex + "].Name' id='ParameterList_" + lastIndex + "__Name' data-index='" + lastIndex + "' value='" + newName + "' />");
        container.append("<input type='hidden' class='paramBag paramBagVal' name='ParameterList[" + lastIndex + "].Value' id='ParameterList_" + lastIndex + "__Value' value='" + newVal + "' data-index='" + lastIndex + "' />");

        // Get the body of the table
        var tableBody = $("#parameterTable tbody");
        tableBody.append("<tr data-index='" + lastIndex + "'><td>" + newName + "</td><td>" + newVal + "</td><td><button type='button' class='btn btn-danger deleteParameter' data-name='" + newName + "'>Delete</button></td>");

        $("#btnSave").prop("disabled", false);
        $("#saveAlert").removeClass("d-none");
    });

    $(".deleteParameter").on('click', function () {
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
        // Re-index the tr's
        $("#parameterTable tbody tr").each(function () {
            var thisIndex = parseInt($(this).attr("data-index"));
            if (indexToDelete < thisIndex) {
                $(this).attr('data-index', (thisIndex - 1));
            }
        });

        $("#btnSave").prop("disabled", false);
    });
});