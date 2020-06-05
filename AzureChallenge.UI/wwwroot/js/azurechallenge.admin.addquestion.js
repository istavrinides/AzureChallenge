$(document).ready(function () {

    var answerIndexer = 0;
    var profileParams = ["Profile.UserNameHashed", "Profile.SubscriptionId", "Profile.TenantId"];
    var allParams = [];
    var suggestionList = [];
    $('[data-toggle="tooltip"]').tooltip();


    profileParams.forEach(function (item) {
        allParams.push(item);
    });
    $("#paramDataList option").each(function () {
        allParams.push($(this).val());
    });
    allParams.forEach(function (item) {
        suggestionList.push({
            key: item,
            value: '{' + item + '}'
        });
    });

    var tribute = new Tribute({
        values: suggestionList,
        selectTemplate: function (item) {
            return item.original.value;
        },
        trigger: "{",
        requireLeadingSpace: false,
        noMatchTemplate: null
    });

    tribute.attach(document.getElementById("Text"));

    $("#Text").on('input', function () {
        var foundParameters = $(this).val().match(/\{([^}]+)\}/g);

        if (!foundParameters || foundParameters.some(param => param.slice(1, -1).includes('{') || param.slice(1, -1).includes('}')))
            return;

        var container = $("#textParamsInputGroup");
        var addedParameters = [];

        // If the regex found parameters
        if (foundParameters) {

            // Clear the container
            container.empty();
            var index = 0;

            foundParameters.forEach(function (item) {

                item = item.slice(1, -1);

                if (!allParams.includes(item)) {
                    allParams.push(item);
                    tribute.appendCurrent([
                        { key: item, value: "{" + item + "}" }
                    ]);
                }

                // If we haven't already added it
                if (!addedParameters.includes(item)) {
                    addedParameters.push(item);

                    if (item.startsWith('Profile.')) {
                        if (!profileParams.includes(item)) {
                            container.append("<span class='btn btn-danger m-1' data-toggle='tooltip' data-placement='top' title='The profile parameter " + item + " is not a valid Profile parameter and will not be included - please correct. Available values: " + profileParams.join() + "'>" + item + "</span>");
                        }
                        else {
                            container.append("<input type='hidden' name='TextParameters[" + index + "]' id='TextParameters_" + index + "_' value='" + item + "' />");
                            container.append("<span class='btn btn-info m-1'>" + item + "</span>");
                        }
                    }
                    else {
                        container.append("<input type='hidden' name='TextParameters[" + index + "]' id='TextParameters_" + index + "_' value='" + item + "' />");
                        container.append("<span class='btn btn-info m-1'>" + item + "</span>");
                    }

                    index += 1;
                }

            });
        }
        else {
            container.empty();
        }
    })

    $("#uriTabContent").on('input', '.uriinput', function () {
        var uriParameters = $(this).val().match(/\{([^}]+)\}/g);

        if (!uriParameters || uriParameters.some(param => param.slice(1, -1).includes('{') || param.slice(1, -1).includes('}')))
            return;

        var itemId = $(this).attr('id');
        var itemIndex = parseInt($(this).data('index'));
        var container = $("#" + itemId + "_params");
        var addedParameters = [];

        // If the uri is for Cosmos Db, we need to add a compulsory ResourceGroupName parameter
        if ($(this).val().indexOf("documents.azure.com") > 0) {
            uriParameters.push("{ResourceGroupName}");
            // If we are looking at the offer url, we need to check the database and collection
            if ($(this).val().toLowerCase().endsWith("/offers")) {
                uriParameters.push("{DatabaseName}");
                uriParameters.push("{CollectionName}");
            }
            // If we are looking for a document, we need to include the PartitionKey
            if ($(this).val().toLowerCase().indexOf("/docs/") > 0) {
                uriParameters.push("{PartitionKey}");
            }
        }

        // If the regex found parameters
        if (uriParameters) {

            // Clear the container
            container.empty();
            var index = 0;

            uriParameters.forEach(function (item) {

                item = item.slice(1, -1);

                if (!allParams.includes(item)) {
                    allParams.push(item);
                    tribute.appendCurrent([
                        { key: item, value: "{" + item + "}" }
                    ]);
                }

                // If we haven't already added it
                if (!addedParameters.includes(item)) {
                    addedParameters.push(item);

                    if (item.startsWith('Profile.')) {
                        if (!profileParams.includes(item)) {
                            container.append("<span class='btn btn-danger m-1' data-toggle='tooltip' data-placement='top' title='The profile parameter " + item + " is not a valid Profile parameter and will not be included - please correct. Available values: " + profileParams.join() + "'>" + item + "</span>");
                        }
                        else {
                            container.append("<input type='hidden' name='Uris[" + itemIndex + "].UriParameters[" + index + "]' id='Uris_" + itemIndex + "__UriParameters_" + index + "_' data-index='" + index + "' value='" + item + "' />");
                            container.append("<span class='btn btn-info m-1'>" + item + "</span>");
                        }
                    }
                    else {
                        container.append("<input type='hidden' name='Uris[" + itemIndex + "].UriParameters[" + index + "]' id='Uris_" + itemIndex + "__UriParameters_" + index + "_' data-index='" + index + "' value='" + item + "' />");
                        container.append("<span class='btn btn-info m-1'>" + item + "</span>");
                    }

                    index += 1;
                }
            });
        }
        else {
            container.empty();
        }
    })

    $("#uriTabContent").on('click', '.btnRemoveUri', function () {
        // Get the Id
        var itemIndex = parseInt($(this).data('index'));

        $("#uri-" + itemIndex + "-content").remove();
        $("#uri-" + itemIndex + "-tab").remove();
        $("#answer-" + itemIndex + "-tab").remove();
        $("#answer-" + itemIndex + "-content").remove();

        // From this index until the max index, we need to renumber/re-id all the controls (uri and answers)
        var lastIndex = 0;
        $("#uriTab a.uriNavItem").each(function () {
            var index = parseInt($(this).data("index"));
            if (index > lastIndex)
                lastIndex = index;
        });

        for (var i = itemIndex + 1; i < lastIndex + 1; i++) {
            $("#uri-" + i + "-tab").attr("href", "#uri-" + (i - 1) + "-content");
            $("#uri-" + i + "-tab").attr("aria-controls", "uri-" + (i - 1) + "-content");
            $("#uri-" + i + "-tab").text("Uri #" + i);
            $("#uri-" + i + "-tab").attr("id", "uri-" + (i - 1) + "-tab");
            $("#uri-" + i + "-content").attr("aria-labelledby", "uri" + (i - 1) + "-tab");
            $("#uri-" + i + "-content").attr("id", "uri-" + (i - 1) + "-content");
            $("#Uris_" + i + "__Id").attr("name", "Uris[" + (i - 1) + "].Id");
            $("#Uris_" + i + "__Id").val(i - 1);
            $("#Uris_" + i + "__Id").attr("id", "Uris_" + (i - 1) + "__Id");
            $("#Uris_" + i + "__Uri").attr("name", "Uris[" + (i - 1) + "].Uri");
            $("#Uris_" + i + "__Uri").attr("id", "Uris_" + (i - 1) + "__Uri");
            $("Uris_" + i + "__CallType").attr("name", "Uris[" + (i - 1) + "].CallType");
            $("Uris_" + i + "__CallType").attr("id", "Uris_" + (i - 1) + "__CallType");
            $("#Uris_" + i + "__Uri_params input").each(function () {
                var index = parseInt($(this).data('index'));
                $(this).attr("name", "Uris[" + (i - 1) + "].UriParameters[" + index + "]");
                $(this).attr("id", "Uris_" + (i - 1) + "__UriParameters_" + index + "_");
            });
            $("#Uris_" + i + "__Uri_params").attr("id", "Uris_" + (i - 1) + "__Uri_params");
            $("#Uris_" + i + "_Uri_remove").attr("id", "Uris_" + (i - 1) + "_Uri_remove");
            $("button[data-index='" + i + "']").attr("data-index", (i - 1));
        }

        var tabToShow = 0;
        // if we removed the last tab
        if (itemIndex > lastIndex)
            tabToShow = itemIndex - 1;
        else if (itemIndex === lastIndex)
            tabToShow = lastIndex;
        else
            tabToShow = itemIndex;


        $("#uri-" + tabToShow + "-tab").trigger('click');
        //$("#answer-" + tabToShow + "-tab").trigger('click');
    });

    $('#addUriLink').on('click', function (e) {
        e.preventDefault()

        // First, figure out what the last index is
        var lastIndex = 0;
        var foundOne = false;
        $("#uriTab a.uriNavItem").each(function () {
            foundOne = true;
            var index = parseInt($(this).data("index"));
            if (index > lastIndex)
                lastIndex = index;
        });
        // Special case if it's the first that we will add, don't increase the index
        if (foundOne)
            lastIndex += 1;

        // Add a new tab before this one (new tabs go to the end)
        $(this).parent().before(
            "<li class='nav-item'> \
                <a class='nav-link uriNavItem' id='uri-"+ lastIndex + "-tab' data-toggle='tab' href='#uri-" + lastIndex + "-content' role='tab' aria-controls='uri-" + lastIndex + "-content' aria-selected='false' data-index='" + lastIndex + "'> Uri #" + (lastIndex + 1) + "</a> \
            </li>");
        // Add the new tab content at then end
        $("#uriTabContent").append(
            "<div class='tab-pane fade' id='uri-" + lastIndex + "-content' role='tabpanel' aria-labelledby='uri" + lastIndex + "-tab'> \
                <input type='hidden' name='Uris["+ lastIndex + "].Id' id='Uris_" + lastIndex + "__Id' value='" + lastIndex + "' /> \
                <div class='input-group mb-3'> \
                    <select class='selectpicker' name='Uris["+ lastIndex + "].CallType' id='Uris_" + lastIndex + "__CallType'> \
                        <option selected>GET</option> \
                    </select > \
                    <input name='Uris["+ lastIndex + "].Uri' id='Uris_" + lastIndex + "__Uri' data-index='" + lastIndex + "' class='form-control uriinput autocomplete' /> \
                </div > \
                <small class='form-text text-muted'>Please enter the Uri for the API to call. Placeholders that will be replaced from below parameters should be wrapped inside curly braces {}.</small> \
                <br /> \
                <div class='input-group mb-3' style='padding-left: 20px'> \
                    <input type='checkbox' name='Uris["+ lastIndex + "].RequiresContributorAccess' id='Uris_" + lastIndex + "__RequiresContributorAccess' class='form-check-input' value='true' /> \
                    <label class='form-check-label' for='Uris_"+ lastIndex + "__RequiresContributorAccess'>Requires elevated (Contributor) access to the resource</label> \
                </div > \
                <div class='form-group'> \
                        <div class='col-md-12 pl-0' > \
                            <span>Discovered parameters:</span> \
                        </div > \
                    <div id='Uris_"+ lastIndex + "__Uri_params' class='form-group row col-md-12'></div> \
                </div> \
                <div class= 'text-right'> \
                    <button type='button' class='btn btn-warning btnRemoveUri' id='Uris_" + lastIndex + "_Uri_remove' data-index='" + lastIndex + "'>Remove this Uri</button> \
                </div > \
            </div>"
        );

        $("#Uris_" + lastIndex + "__CallType").selectpicker();

        tribute.attach(document.getElementById('Uris_' + lastIndex + '__Uri'));

        // If it's the first, focus on it
        if (!foundOne) {
            $("#uri-0-tab").trigger('click');
        }
    });

    $("#addLink").on('click', function () {

        var newLink = $("#inputNewLink").val();

        if (!newLink) {
            $("#inputNewLink").popover('show');
            return;
        }

        // Get the next index
        var newIndex = $("#linksInputGroup input").length;

        $("#linksInputGroup")
            .append("<input type='hidden' name='UsefulLinks[" + newIndex + "]' id='UsefulLinks_" + newIndex + "_' data-index='" + newIndex + "' value='" + newLink + "' /> \
                    <span class='border border-info rounded p-1 bg-info m-1' data-index='" + newIndex + "'> \
                        <a href='" + newLink + "' style='color:#fff !important'>" + newLink + "</a>&nbsp; \
                        <span class='badge badge-danger deleteLink' style='cursor:pointer' data-index='" + newIndex + "'>&times</span> \
                    </span>");

    });

    $("#linksInputGroup").on('click', '.deleteLink', function () {
        // Get the index to delete
        var index = parseInt($(this).attr('data-index'));

        // Delete the input and span with that index
        $("#linksInputGroup input[data-index='" + index + "']").remove();
        $("#linksInputGroup span.border[data-index='" + index + "']").remove();

        // Re-index the remaining
        $("#linksInputGroup input").each(function () {
            var thisIndex = $(this).attr('data-index');
            if (thisIndex > index) {
                $(this).attr('data-index', (thisIndex - 1));
                $(this).attr('id', 'UsefulLinks_' + (thisIndex - 1) + '_');
                $(this).attr('name', 'UsefulLinks[' + (thisIndex - 1) + ']');
            }
        });
        $("#linksInputGroup span").each(function () {
            var thisIndex = $(this).attr('data-index');
            if (thisIndex > index) {
                $(this).attr('data-index', (thisIndex - 1));
            }
        });
    });

    $("form").submit(function () {
        if ($(this).valid()) {
            $("#waitModal").modal('show');
        }
    });
});