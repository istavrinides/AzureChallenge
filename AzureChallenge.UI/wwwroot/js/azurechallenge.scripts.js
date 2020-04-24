$(document).ready(function () {

    var answerIndexer = 0;

    $("#Text").on('input', function () {
        var foundParameters = $(this).val().match(/\{([^}]+)\}/g);

        if (foundParameters.some(param => param.slice(1, -1).includes('{') || param.slice(1, -1).includes('}')))
            return;

        var container = $("#textParamsInputGroup");

        // If the regex found parameters
        if (foundParameters) {

            // Get the existing key-value pairs
            var originalDict = {};
            if ($("input[data-textparam]")) {
                // Get all the keys from the name input
                $("input[data-textparam]").each(function () {
                    originalDict[$(this).data('textparam')] = "";
                });
                $("input[data-textparamval]").each(function (currentInput) {
                    originalDict[$(this).data('textparamval')] = $(this).val();
                });
            }

            // Create a new dict with the still existing keys
            var dict = {};
            Object.keys(originalDict).forEach(function (key) {
                // Need to append the curly braces since we didn't drop them from the regex output
                if (foundParameters.includes('{' + key + '}'))
                    dict[key] = originalDict[key];
            });

            // Clear the container
            container.empty();
            var index = 0;

            foundParameters.forEach(function (item) {

                item = item.slice(1, -1);

                var oldValue = "";
                if (item in dict)
                    oldValue = dict[item];

                container.append("<div class='form-group col-6'> \
                                                        <label>Name</label> \
                                                        <input class='form-control' name='TextParameters[" + index + "].Key' data-textparam='" + item + "' value='" + item + "' disabled  /></div>");
                container.append("<div class='form-group col-6'> \
                                                        <label>Value</label> \
                                                        <input class='form-control' name='TextParameters[" + index + "].Value' data-textparamval='" + item + "' value='" + oldValue + "' /></div>");

                index += 1;


            });
        }
        else {
            container.empty();
        }
    })

    $("#uriTabContent").on('input', '.uriinput', function () {
        var uriParameters = $(this).val().match(/\{([^}]+)\}/g);

        if (uriParameters.some(param => param.slice(1, -1).includes('{') || param.slice(1, -1).includes('}')))
            return;

        var itemId = $(this).attr('id');
        var itemIndex = parseInt($(this).data('index'));
        var container = $("#" + itemId + "_params");

        // If the regex found parameters
        if (uriParameters) {

            // Get the existing key-value pairs
            var originalDict = {};
            if ($("#Uris_" + itemIndex + "__Uri_params input.uriParamInput")) {
                // Get all the keys from the name input
                $("#Uris_" + itemIndex + "__Uri_params input.uriParamInputKey").each(function () {
                    originalDict[$(this).data('uriparam')] = "";
                });
                $("#Uris_" + itemIndex + "__Uri_params input.uriParamInputVal").each(function (currentInput) {
                    originalDict[$(this).data('uriparamval')] = $(this).val();
                });
            }

            // Create a new dict with the still existing keys
            var dict = {};
            Object.keys(originalDict).forEach(function (key) {
                // Need to append the curly braces since we didn't drop them from the regex output
                if (uriParameters.includes('{' + key + '}'))
                    dict[key] = originalDict[key];
            });

            // Clear the container
            container.empty();
            var index = 0;

            uriParameters.forEach(function (item) {

                item = item.slice(1, -1);

                var oldValue = "";
                if (item in dict)
                    oldValue = dict[item];

                container.append("<div class='form-group col-6'> \
                                                        <label>Name</label> \
                                                        <input class='form-control uriParamInput uriParamInputKey' name='Uris["+ itemIndex + "].UriParameters[" + index + "].Key' id='Uris_" + itemIndex + "__UriParameters_" + index + "__Key' data-uriparam='" + item + "' value='" + item + "' data-index='" + index + "' disabled  /></div>");
                container.append("<div class='form-group col-6'> \
                                                        <label>Value</label> \
                                                        <input class='form-control uriParamInput uriParamInputVal' name='Uris["+ itemIndex + "].UriParameters[" + index + "].Value' id='Uris_" + itemIndex + "__UriParameters_" + index + "__Value' data-uriparamval='" + item + "' value='" + oldValue + "' data-index='" + index + "' /></div>");

                index += 1;
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
            $("#Uris_" + i + "__Id").attr("id", "Uris_" + (i - 1) + "__Id");
            $("#Uris_" + i + "__Uri").attr("name", "Uris[" + (i - 1) + "].Uri");
            $("#Uris_" + i + "__Uri").attr("id", "Uris_" + (i - 1) + "__Uri");
            $("#Uris_" + i + "__Uri_params .uriParamInputKey").each(function () {
                var index = parseInt($(this).data('index'));
                $(this).attr("name", "Uris[" + (i - 1) + "].UriParameters[" + index + "].Key");
                $(this).attr("id", "Uris_" + (i - 1) + "__UriParameters_" + index + "__Key");
            });
            $("#Uris_" + i + "__Uri_params .uriParamInputVal").each(function () {
                var index = parseInt($(this).data('index'));
                $(this).attr("name", "Uris[" + (i - 1) + "].UriParameters[" + index + "].Value");
                $(this).attr("id", "Uris_" + (i - 1) + "__UriParameters_" + index + "__Value");
            });
            $("#Uris_" + i + "__Uri_params").attr("id", "Uris_" + (i - 1) + "__Uri_params");
            $("#Uris_" + i + "_Uri_remove").attr("id", "Uris_" + (i - 1) + "_Uri_remove");
            $("#answer-" + i + "-tab").attr("href", "#answer-" + (i - 1) + "-content");
            $("#answer-" + i + "-tab").attr("aria-controls", "answer-" + (i - 1) + "-content");
            $("#answer-" + i + "-tab").text("Answer for Uri #" + i);
            $("#answer-" + i + "-tab").attr("id", "answer-" + (i - 1) + "-tab");
            $("#Answers_" + i + "__params .answerParamInputKey").each(function () {
                var index = parseInt($(this).data('index'));
                $(this).attr("name", "Answers[" + (i - 1) + "].AnswerParameters[" + index + "].Key");
                $(this).attr("id", "Answers_" + (i - 1) + "__AnswerParameters_" + index + "__Key");
            });
            $("#Answers_" + i + "__params .answerParamInputVal").each(function () {
                var index = parseInt($(this).data('index'));
                $(this).attr("name", "Answers[" + (i - 1) + "].AnswerParameters[" + index + "].Value");
                $(this).attr("id", "Answers_" + (i - 1) + "__AnswerParameters_" + index + "__Value");
            });
            $("#Answers_" + i + "__AssociatedQuestionId").attr("name", "#Answers[" + (i - 1) + "].AssociatedQuestionId")
            $("#Answers_" + i + "__AssociatedQuestionId").attr("id", "#Answers_" + (i - 1) + "__AssociatedQuestionId")
            $("#Answers_" + i + "__params").attr("id", "Answers_" + (i - 1) + "__params");
            $("#answer-" + i + "-content input:radio").each(function () {
                $(this).attr("name", "Uris[" + (i - 1) + "].CallType");
                $(this).attr("id", "Uris_" + (i - 1) + "__CallType");
            });
            $("#answer-" + i + "-content").attr("aria-labelledby", "answer" + (i - 1) + "-tab");
            $("#answer-" + i + "-content").attr("id", "answer-" + (i - 1) + "-content");
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
                <input type='hidden' name='Uris["+ lastIndex + "].Id' id='Uris_" + lastIndex + "__Id' /> \
                <div class='input-group mb-3'> \
                    <div class= 'input-group-prepend btn-group-toggle' data - toggle='buttons' > \
                        <label class='btn btn-secondary'> \
                            <input type='radio' name='Uris["+ lastIndex + "].CallType' id='Uris_" + lastIndex + "__CallType' value='GET' id='GET' autocomplete='off' checked> GET \
                        </label> \
                        <label class='btn btn-secondary'> \
                            <input type='radio' name='Uris["+ lastIndex + "].CallType' id='Uris_" + lastIndex + "__CallType' value='HEAD' id='HEAD' autocomplete='off'> HEAD \
                        </label> \
                    </div> \
                    <input name='Uris["+ lastIndex + "].Uri' id='Uris_" + lastIndex + "__Uri' data-index='" + lastIndex + "' class='form-control uriinput' /> \
                </div > \
                <small class='form-text text-muted'>Please enter the Uri for the API to call. Placeholders that will be replaced from below parameters should be wrapped inside curly braces {}.</small> \
                <br /> \
                <div id='Uris_"+ lastIndex + "__Uri_params' class='form-group row'> \
                </div> \
                <div class= 'text-right'> \
                    <button type='button' class='btn btn-warning btnRemoveUri' id='Uris_" + lastIndex + "_Uri_remove' data-index='" + lastIndex + "'>Remove this Uri</button> \
                </div > \
            </div>"
        );

        // Add a new answer tab (every question will have a answer associated with it)
        $("#answerTab").append(
            "<li class='nav-item'> \
                <a class='nav-link answerNavItem' id='answer-" + lastIndex + "-tab' data-toggle='tab' href='#answer-" + lastIndex + "-content' role='tab' aria-controls='answer-" + lastIndex + "-content' aria-selected='false'>Answer for Uri #" + (lastIndex + 1) + "</a> \
            </li>"
        );
        // Add a new tab content for the answer
        $("#answerTabContent").append(
            "<div class='tab-pane fade' id='answer-" + lastIndex + "-content' role='tabpanel' aria-labelledby='answer-" + lastIndex + "-tab'> \
                <input type='hidden' name='Answers[" + lastIndex + "].AssociatedQuestionId' id='Answers_" + lastIndex + "__AssociatedQuestionId' value='" + lastIndex + "' /> \
                <div class= 'form-group input-group mb-3'> \
                    <div class='col-3 pl-0'> \
                        <label class='pr-3'>Answer will be checked against:</label> \
                    </div> \
                    <div class='col-7'> \
                        <div class='custom-radio'> \
                            <label class='pr-4');> \
                                <input type='radio' name='Answers[" + lastIndex + "].ResponseType' value='HEAD' id='respHEAD' autocomplete='off' checked> Headers \
                            </label> \
                            <label> \
                                <input type='radio' name='Answers[" + lastIndex + "].ResponseType' value='BODY' id='respBODY' autocomplete='off'> Body \
                            </label> \
                        </div> \
                    </div> \
                    <div class='col-2 text-right pr-0'> \
                        <button type='button' class='btn btn-info addAnswerParameter' data-index='" + lastIndex + "'>Add parameter</button> \
                    </div> \
                </div> \
                <div id='Answers_" + lastIndex + "__params' class='form-group row'></div> \
            </div>"
        );

        // If it's the first, focus on it
        if (!foundOne) {
            $("#uri-0-tab").trigger('click');
            $("#answer-0-tab").trigger('click');
        }
    })

    $("#answersParamsGroup").on('click', '.addAnswerParameter', function () {

        // Get the index from the Id
        var itemIndex = parseInt($(this).data("index"));

        var currentIndex = -1;
        // Get the largest index under the current itemdId
        $("#Answers_" + itemIndex + "__params input.answerParamInputKey").each(function () {
            thisIndex = parseInt($(this).data('index'));
            if (currentIndex < thisIndex)
                currentIndex = thisIndex;
        })

        // If it's the first one
        if (currentIndex === -1)
            currentIndex = 0;
        else
            currentIndex += 1;

        var container = $("#Answers_" + itemIndex + "__params");

        container.append("<div class='form-group col-6'> \
                            <label>Name</label> \
                            <input class='form-control answerParamInput answerParamInputKey' name='Answers[" + itemIndex + "].AnswerParameters[" + currentIndex + "].Key' id='Answers_" + itemIndex + "_AnswerParameters_" + currentIndex + "__Key'' data-index='" + currentIndex + "' /></div>");
        container.append("<div class='form-group col-6'> \
                            <label>Value</label> \
                            <input class='form-control answerParamInput answerParamInputVal' name='Answers[" + itemIndex + "].AnswerParameters[" + currentIndex + "].Value' id='Answers_" + itemIndex + "_AnswerParameters_" + currentIndex + "__Value' data-index='" + currentIndex + "' /></div>");

        return false;
    });
});