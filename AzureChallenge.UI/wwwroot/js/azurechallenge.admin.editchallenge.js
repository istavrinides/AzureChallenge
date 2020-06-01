var previousSelectedValue = -1;

$(document).ready(function () {
    $.validator.setDefaults({ ignore: '' });
    $('[data-toggle="tooltip"]').tooltip();

    $("#IsPublic").on('change', function () {
        $("#btnSave").removeAttr('disabled');
        $("#saveAlert").removeClass("d-none");
    });

    $('#questionsSelector').on('changed.bs.select', function (e, clickedIndex, isSelected, previousValue) {
        $("#btnAddToList").removeAttr('disabled');
    });

    $("#btnAddToList").on('click', function () {
        //// Get the question to associate
        var selectedQuestionId = $("#questionsSelector")[0].selectedOptions[0].attributes[0].nodeValue;
        var challengeId = $("#Id").val();

        populateModalAddNew(selectedQuestionId, challengeId);
    });

    $("#btnSave").on('click', function () {
        $.post("/Administration/Challenge/UpdateChallenge", $('form').serialize())
            .done(function () {
                $("#updateModal").hide();
                $('body').removeClass('modal-open');
                $('.modal-backdrop').remove();
                $("#btnSave").prop('disabled', 'disabled');
                $("#saveAlert").addClass("d-none");
                location.reload();
            })
            .fail(function () {
                window.alert("Could not update the challenge, an internal error occured. Please try again later.");
                location.reload();
            });
    })

    $("#associatedQuestionsTable .indexSelector").on('focus', function () {
        previousSelectedValue = parseInt($(this).val());
    }).change(function () {

        if (previousSelectedValue >= 0) {
            // Enable the save button
            $("#btnSave").removeAttr('disabled');
            $("#saveAlert").removeClass("d-none");

            // Get the value
            var index = parseInt($(this).val());

            // Find the select control that has this (new) value and is not this control
            var selectToChange = $(".indexSelector option[value='" + index + "']:selected").parent().not(this);
            $(selectToChange).val(previousSelectedValue);

            // Swap the rows. First will be the one with the smallest original index
            var firstRow, secondRow;
            if (previousSelectedValue < index) {
                firstRow = $(this).closest('tr');
                secondRow = $(selectToChange).closest('tr');
            }
            else {
                firstRow = $(selectToChange).closest('tr');
                secondRow = $(this).closest('tr');
            }


            var firstRowNQId = $(firstRow).find(".nextQuestionId").val();
            var secondRowNQId = $(secondRow).find(".nextQuestionId").val();


            // Check if they are adjacent.
            if ($(firstRow).next('tr').is($(secondRow))) {
                // Swap the rows
                $(firstRow).insertAfter($(secondRow));

                // Fix the NextQuestionIds
                $(firstRow).find(".nextQuestionId").val(secondRowNQId);
                $(secondRow).find(".nextQuestionId").val($(firstRow).find(".questionId").val());

            }
            // Check if the first row is the top most row
            else if ($(firstRow).prev('tr').length === 0) {
                // Get the row below the first one (none above)
                var belowFirst = $(firstRow).prev('tr');
                // Get the row above and below the second one
                var aboveSecond = $(secondRow).prev('tr');
                var belowSecond = $(secondRow).next('tr');

                // Second row goes above the first
                $(secondRow).insertBefore($(firstRow));
                // First row goes below the aboveSecond
                $(firstRow).insertAfter($(aboveSecond));

                // Fix the NextQuestionIds
                $(secondRow).find(".nextQuestionId").val(firstRowNQId);
                $(aboveSecond).find(".nextQuestionId").val($(firstRow).find(".questionId").val());
                $(firstRow).find(".nextQuestionId").val(secondRowNQId);
            }
            else {
                // Get the row above the first and second row
                var aboveFirst = $(firstRow).prev('tr');
                var aboveSecond = $(secondRow).prev('tr');

                // Second row goes above the aboveFirst
                $(secondRow).insertAfter($(aboveFirst));
                // First row goes below the aboveSecond
                $(firstRow).insertAfter($(aboveSecond));

                // Fix the NextQuestionIds
                $(secondRow).find(".nextQuestionId").val(firstRowNQId);
                $(aboveSecond).find(".nextQuestionId").val($(firstRow).find(".questionId").val());
                $(firstRow).find(".nextQuestionId").val(secondRowNQId);
                $(aboveFirst).find(".nextQuestionId").val($(secondRow).find(".questionId").val());
            }

            previousSelectedValue = -1;
        }
    });

    $("#associatedQuestionsTable .tableLinkEdit").on('click', function () {
        var selectedQuestionId = $(this).attr('data-questionId');
        var challengeId = $("#Id").val();

        populateModal(selectedQuestionId, challengeId, false)
    })

    $("#associatedQuestionsTable .tableLinkDetails").on('click', function () {
        var selectedQuestionId = $(this).attr('data-questionId');
        var challengeId = $("#Id").val();

        populateModal(selectedQuestionId, challengeId, true)
    })

    $("#associatedQuestionsTable .tableLinkCopy").on('click', function () {
        var selectedQuestionId = $(this).attr('data-questionId');
        var challengeId = $("#Id").val();

        populateModalAddNew(selectedQuestionId, challengeId);
    })

    $("#associatedQuestionsTable .tableLinkCheck").on('click', function () {
        var questionId = $(this).attr('data-questionId');

        // Clear the contents
        $("#checkModalContentDiv").empty();

        $.get("/Administration/Challenge/ValidateQuestion?questionId=" + questionId)
            .done(function (data) {
                $("#checkModalWaiting").hide();

                $("#checkModalContent").removeClass('d-none');

                data.forEach(function (item) {
                    $("#checkModalContentDiv").append("<div class='col-md-6'>" + item.Key + "</div><div class='col-md-6'>" + (item.Value ? "<span class='text-success'>Success</span>" : "<span class='text-danger'>Failure</span>") + "</div>");
                });
            })
            .fail(function () {
                window.alert("Could not validate the question, an internal error occured. Please try again later.");
                $("#checkModal").hide();
                $('body').removeClass('modal-open');
                $('.modal-backdrop').remove();
            });
    })

    $("#associatedQuestionsTable .deleteAssociatedQuestion").on('click', function () {
        var questionId = $(this).attr('data-questionId');
        var challengeId = $("#Id").val();

        $.get("/Administration/Challenge/RemoveQuestion?questionId=" + questionId + "&challengeId=" + challengeId)
            .done(function (data) {
                location.reload();
            })
            .fail(function () {
                window.alert("Could not delete the question, an internal error occured. Please try again later.");
            });
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

        container.append("<div class='form-group col-4'> \
                            <label>Path</label> \
                            <input class='form-control answerParamInput answerParamInputKey' name='QuestionToAdd.Answers[" + itemIndex + "].AnswerParameters[" + currentIndex + "].Key' id='QuestionToAdd_Answers_" + itemIndex + "_AnswerParameters_" + currentIndex + "__Key'' data-index='" + currentIndex + "' required /></div>");
        container.append("<div class='form-group col-4'> \
                            <label>Value to check</label> \
                            <input class='form-control answerParamInput answerParamInputVal' name='QuestionToAdd.Answers[" + itemIndex + "].AnswerParameters[" + currentIndex + "].Value' id='QuestionToAdd_Answers_" + itemIndex + "_AnswerParameters_" + currentIndex + "__Value' data-index='" + currentIndex + "' required /></div>");
        container.append("<div class='form-group col-4'> \
                            <label>Error Message</label> \
                            <input class='form-control answerParamInput answerParamInputError' name='QuestionToAdd.Answers[" + itemIndex + "].AnswerParameters[" + currentIndex + "].ErrorMessage' id='QuestionToAdd_Answers_" + itemIndex + "_AnswerParameters_" + currentIndex + "__ErrorMessage' data-index='" + currentIndex + "' required /></div>");

        var numberOfRequiredInputs = 0;
        if (parseInt($("#requiredInputsAnswer").text()))
            numberOfRequiredInputs = parseInt($("#requiredInputsAnswer").text());

        $("#requiredInputsAnswer").text(numberOfRequiredInputs + 3);

        return false;
    });

    $('#questionModal').on('hidden.bs.modal', function () {
        $("#modalWaiting").show();
        $("#modalContent").addClass('d-none');
        $("#modalError").addClass('d-none');
        $(".modal-footer").addClass('d-none');
        $("#btnModalSave").removeAttr('disabled');
        $("#btnModalSave").show();
    });

    $('#checkModal').on('hidden.bs.modal', function () {
        $("#checkModalWaiting").show();
        $("#checkModalContent").addClass('d-none');
    });

    $(".tab-pane").on('input', 'input', function () {
        var tab_pane_id = $(this).closest('.tab-pane-upper').attr('id');

        var span = $("#modalTabs a.nav-link[aria-controls='" + tab_pane_id + "'] span");

        var numOfEmptyInputsInTab = $("#" + tab_pane_id + " input:not(.modal-hidden)").filter(function () { return !$(this).val(); }).length;

        if (numOfEmptyInputsInTab > 0)
            $(span).text(numOfEmptyInputsInTab);
        else
            $(span).text('');

        var numOfEmptyInputsInAllTabs = $("#modalTabsContent input:not(.modal-hidden)").filter(function () { return !$(this).val(); }).length;

        if (numOfEmptyInputsInAllTabs === 0)
            $("#btnModalSave").removeClass('d-none');
        else
            $("#btnModalSave").addClass('d-none');

    });
});

var populateModal = function (selectedQuestionId, challengeId, readOnly = false) {

    // Clear the modal
    $("#uriTab").empty();
    $("#uriTabContent").empty();
    $("#answerTab").empty();
    $("#answerTabContent").empty();
    $("#requiredInputsText").text('');
    $("#requiredInputsUri").text('');
    $("#requiredInputsAnswer").text('');

    $.get("/Administration/Challenge/" + challengeId + "/AssignedQuestion/" + selectedQuestionId)
        .done(function (data) {
            $("#QuestionToAdd_Text").val(data.text);
            if (readOnly)
                $("#QuestionToAdd_Text").addClass('d-none');
            $("#QuestionToAdd_AssociatedQuestionId").val(data.id);
            $("#QuestionToAdd_Name").val(data.name);
            $("#QuestionToAdd_TargettedAzureService").val(data.targettedAzureService);
            $("#QuestionToAdd_Difficulty").val(data.difficulty);
            $("#QuestionToAdd_Description").val(data.description);
            $("#QuestionToAdd_ChallengeId").val(challengeId);
            $("#QuestionToAdd_Id").val(data.id);
            $("#QuestionToAdd_Justification").val(data.justification);

            // Clear any old links from previous modal loads
            $(".modal-hidden-urls").remove();

            for (var i = 0; i < data.usefulLinks.length; i++) {
                $("#hiddenParamGroup").append("<input type='hidden' class='modal-hidden modal-hidden-urls' name='QuestionToAdd.UsefulLinks[" + i + "]' id='QuestionToAdd_UsefulLinks_" + i + "_' value='" + data.usefulLinks[i] + "' />");
            }

            $.get("/Administration/Challenge/" + challengeId + "/GlobalParameters/Get")
                .done(function (globalParams) {

                    $("#modalWaiting").hide();

                    $("#modalContent").removeClass('d-none');
                    $(".modal-footer").removeClass('d-none');

                    if (readOnly) {
                        $("#btnModalSave").prop('disabled', 'disabled');
                        $("#btnModalSave").hide();
                    }

                    // Construct the text parameters
                    var container = $("#textParamsInputGroup");
                    container.empty();

                    for (var i = 0; i < data.textParameters.length; i++) {
                        container.append("<div class='form-group col-6'> \
                                            <label>"+ data.textParameters[i].key + "</label> \
                                            <input type='hidden' name='QuestionToAdd.TextParameters[" + i + "].Key' id='QuestionToAdd_TextParameters_" + i + "__Key' value='" + data.textParameters[i].key + "' /> \
                                          </div>");

                        // Check if this is a global parameters
                        if (globalParams.parameters.some(p => p.key === data.textParameters[i].key)) {
                            container.append("<div class='form-group col-6'> " + globalParams.parameters.find(p => p.key === data.textParameters[i].key).value + " \
                                                    <input type='hidden' data-paramkey='"+ data.textParameters[i].key + "' class='paramValue' value='" + globalParams.parameters.find(p => p.key === data.textParameters[i].key).value + "' /> \
                                                </div>");
                        }
                        // Check if this is a profile parameter
                        else if (data.textParameters[i].key.startsWith('Profile.')) {
                            // Get the value for the current user
                            var profileValue = $("#CurrentUserProfile_" + data.textParameters[i].key.substr(8)).val();
                            container.append("<div class='form-group col-6'>" + profileValue + " \
                                                    <input type='hidden' data-paramkey='"+ data.textParameters[i].key + "' class='paramValue' value='" + profileValue + "' /> \
                                                <span class='badge badge-warning' data-toggle='tooltip' data-placement='top' title='This is your value. Each user will automatically get their own based on their profile'>(note)</span> \
                                             </div>");
                        }
                        else {
                            container.append("<div class='form-group col-6'> \
                                                <input class='form-control paramValue' data-paramkey='"+ data.textParameters[i].key + "' name='QuestionToAdd.TextParameters[" + i + "].Value' id='QuestionToAdd_TextParameters_" + i + "__Value' required value='" + (data.textParameters[i].value === "null" ? " " : data.textParameters[i].value) + "' " + (readOnly ? "readonly" : "") + " oninput='populatePreview();' /> \
                                            </div>");
                        }
                    }

                    // Construct the uri tabs
                    container = $("#uriTab");
                    for (i = 0; i < data.uris.length; i++) {
                        container.append(
                            "<li class='nav-item'> \
                                        <a class='nav-link uriNavItem' id='uri-"+ i + "-tab' data-toggle='tab' href='#uri-" + i + "-content' role='tab' aria-controls='uri-" + i + "-content' aria-selected='false' data-index='" + i + "'> Uri #" + (i + 1) + "</a> \
                                    </li>"
                        );

                        var toAppend = "<div class='tab-pane fade' id='uri-" + i + "-content' role='tabpanel' aria-labelledby='uri" + i + "-tab'> \
                                                    <input type='hidden' name='QuestionToAdd.Uris["+ i + "].Id' id='QuestionToAdd_Uris_" + i + "__Id' value='" + data.uris[i].id + "' /> \
                                                    <div class='input-group mb-3'> \
                                                        <div class= 'input-group-prepend btn-group-toggle' data-toggle='buttons' > \
                                                            <span class='input-group-text'>"+ data.uris[i].callType + "</span> \
                                                            <input type='hidden' name='QuestionToAdd.Uris["+ i + "].CallType' id='QuestionToAdd_Uris_" + i + "__CallType' value='" + data.uris[i].callType + "' /> \
                                                        </div> \
                                                        <input name='QuestionToAdd.Uris["+ i + "].Uri' id='QuestionToAdd_Uris_" + i + "__Uri' data-index='" + i + "' class='form-control' value='" + data.uris[i].uri + "' readonly /> \
                                                    </div > \
                                                    <br /> \
                                                    <div class='form-group row'>";

                        for (var j = 0; j < data.uris[i].uriParameters.length; j++) {

                            toAppend += "<div class='form-group col-6'> \
                                                    <label>"+ data.uris[i].uriParameters[j].key + "</label> \
                                                    <input type='hidden' name='QuestionToAdd.Uris[" + i + "].UriParameters[" + j + "].Key' id='QuestionToAdd_Uris_" + i + "__UriParameters_" + j + "__Key' value='" + data.uris[i].uriParameters[j].key + "' /> \
                                                 </div>";

                            // Check if this is a global parameters
                            if (globalParams.parameters.some(p => p.key === data.uris[i].uriParameters[j].key)) {
                                toAppend += "<div class='form-group col-6'> " + globalParams.parameters.find(p => p.key === data.uris[i].uriParameters[j].key).value + " \
                                            <input type='hidden' name='QuestionToAdd.Uris[" + i + "].UriParameters[" + j + "].Value' id='QuestionToAdd_Uris_" + i + "__UriParameters_" + j + "__Key' value='" + data.uris[i].uriParameters[j].value + "' /> \
                                            </div>";

                            }
                            // Check if this is a profile parameter
                            else if (data.uris[i].uriParameters[j].key.startsWith('Profile.')) {
                                // Get the value for the current user
                                var profileValue = $("#CurrentUserProfile_" + data.uris[i].uriParameters[j].key.substr(8)).val();
                                toAppend += "<div class='form-group col-6'>" + profileValue + " <span class='badge badge-warning' data-toggle='tooltip' data-placement='top' title='This is your value. Each user will automatically get their own based on their profile'>(note)</span></div>";
                            }
                            else {
                                toAppend += "<div class='form-group col-6'> \
                                                <input class='form-control' name='QuestionToAdd.Uris[" + i + "].UriParameters[" + j + "].Value' id='QuestionToAdd_Uris_" + i + "__UriParameters_" + j + "__Value' required value='" + data.uris[i].uriParameters[j].value + "' " + (readOnly ? "readonly" : "") + "/> \
                                            </div>";
                            }


                        }
                        toAppend += "       </div> \
                                                </div>";
                        $("#uriTabContent").append(toAppend);

                        // Add a new answer tab (every question will have a answer associated with it)
                        $("#answerTab").append(
                            "<li class='nav-item'> \
                                        <a class='nav-link answerNavItem' id='answer-" + i + "-tab' data-toggle='tab' href='#answer-" + i + "-content' role='tab' aria-controls='answer-" + i + "-content' aria-selected='false'>Answer for Uri #" + (i + 1) + "</a> \
                                    </li>"
                        );

                        toAppend = "";

                        for (j = 0; j < data.answers[i].answerParameters.length; j++) {

                            toAppend += "<div class='form-group col-4'> \
                                            <label>Path</label> \
                                        <input class='form-control answerParamInput answerParamInputKey' name='QuestionToAdd.Answers[" + i + "].AnswerParameters[" + j + "].Key' id='QuestionToAdd_Answers_" + i + "_AnswerParameters_" + j + "__Key' data-index='" + j + "' required " + (readOnly ? "readonly" : "") + " /></div>";
                            toAppend += "<div class='form-group col-4'> \
                                            <label>Value to check</label> \
                                        <input class='form-control answerParamInput answerParamInputVal' name='QuestionToAdd.Answers[" + i + "].AnswerParameters[" + j + "].Value' id='QuestionToAdd_Answers_" + i + "_AnswerParameters_" + j + "__Value' data-index='" + j + "' required " + (readOnly ? "readonly" : "") + " /></div>";
                            toAppend += "<div class='form-group col-4'> \
                                            <label>Error Message</label> \
                                        <input class='form-control answerParamInput answerParamInputError' name='QuestionToAdd.Answers[" + i + "].AnswerParameters[" + j + "].ErrorMessage' id='QuestionToAdd_Answers_" + i + "_AnswerParameters_" + j + "__ErrorMessage' data-index='" + j + "' required " + (readOnly ? "readonly" : "") + " /></div>";
                        }

                        // Add a new tab content for the answer
                        $("#answerTabContent").append(
                            "<div class='tab-pane fade' id='answer-" + i + "-content' role='tabpanel' aria-labelledby='answer-" + i + "-tab'> \
                                <input type='hidden' name='QuestionToAdd.Answers[" + i + "].AssociatedUriId' id='QuestionToAdd_Answers_" + i + "__AssociatedUriId' value='" + i + "' /> \
                                <div class= 'form-group input-group mb-3'> \
                                    <div class='col-5 pl-0'> \
                                        <label class='pr-3'>Answer will be checked against:</label> \
                                    </div> \
                                    <div class='col-4'> \
                                        <div class='custom-radio'> \
                                            <label class='pr-4'> \
                                                <input type='radio' name='QuestionToAdd.Answers[" + i + "].ResponseType' id='QuestionToAdd_Answers_" + i + "__ResponseType' value='BODY' id='respBODY' autocomplete='off' " + (data.answers[i].responseType === "BODY" ? "checked" : "") + " " + (readOnly ? "disabled" : "") + "> Body \
                                            </label> \
                                        </div> \
                                    </div> \
                                    <div class='col-3 text-right pr-0'> \
                                        " + (readOnly ? "" : "<button type='button' class='btn btn-info addAnswerParameter' data-index='" + i + "'>Add parameter</button>") + " \
                                    </div> \
                                </div> \
                                <div id='Answers_" + i + "__params' class='form-group row'>" + toAppend + "</div> \
                            </div>"
                        );

                        for (j = 0; j < data.answers[i].answerParameters.length; j++) {

                            $("#QuestionToAdd_Answers_" + i + "_AnswerParameters_" + j + "__Key").val(data.answers[i].answerParameters[j].key);
                            $("#QuestionToAdd_Answers_" + i + "_AnswerParameters_" + j + "__Value").val(data.answers[i].answerParameters[j].value);
                            $("#QuestionToAdd_Answers_" + i + "_AnswerParameters_" + j + "__ErrorMessage").val(data.answers[i].answerParameters[j].errorMessage);
                        }
                    }

                    $("#uri-0-tab").trigger('click');
                    $("#answer-0-tab").trigger('click');
                    populatePreview();
                });
        })
        .fail(function () {
            $("#modalError").removeClass('d-none');
            $(".modal-footer").removeClass('d-none');
            $("#btnModalSave").addClass('d-none');
        });
}

var populateModalAddNew = function (selectedQuestionId, challengeId) {

    // Clear the modal
    $("#uriTab").empty();
    $("#uriTabContent").empty();
    $("#answerTab").empty();
    $("#answerTabContent").empty();
    $("#requiredInputsText").text('');
    $("#requiredInputsUri").text('');
    $("#requiredInputsAnswer").text('');

    // Fill the modal with the initial data
    // Call the API to get the details of the selected question
    $.get("/Administration/Questions/" + selectedQuestionId + "/Get")
        .done(function (data) {
            $("#QuestionToAdd_Text").val(data.text);
            $("#QuestionToAdd_AssociatedQuestionId").val(data.id);
            $("#QuestionToAdd_Name").val(data.name);
            $("#QuestionToAdd_TargettedAzureService").val(data.targettedAzureService);
            $("#QuestionToAdd_Difficulty").val(data.difficulty);
            $("#QuestionToAdd_Description").val(data.description);
            $("#QuestionToAdd_ChallengeId").val(challengeId);
            $("#QuestionToAdd_Justification").val(data.justification);
            $("#QuestionToAdd_Id").val("");

            // Clear any old links from previous modal loads
            $(".modal-hidden-urls").remove();

            for (var i = 0; i < data.usefulLinks.length; i++) {
                $("#hiddenParamGroup").append("<input type='hidden' class='modal-hidden modal-hidden-urls' name='QuestionToAdd.UsefulLinks[" + i + "]' id='QuestionToAdd_UsefulLinks_" + i + "_' value='" + data.usefulLinks[i] + "' />");
            }

            $.get("/Administration/Challenge/" + challengeId + "/GlobalParameters/Get")
                .done(function (globalParams) {

                    $("#modalWaiting").hide();
                    var numOfEmptyInputsText = 0;
                    var numOfEmptyInputsUri = 0;
                    var numOfEmptyInputsAnswers = 0;
                    var exists = false;
                    var paramValue = '';

                    if (globalParams) {
                        $("#modalContent").removeClass('d-none');
                        $(".modal-footer").removeClass('d-none');

                        // Construct the text parameters
                        var container = $("#textParamsInputGroup");
                        container.empty();

                        for (var i = 0; i < data.textParameters.length; i++) {
                            container.append("<div class='form-group col-6'> \
                                                    <label>"+ data.textParameters[i] + "</label> \
                                                    <input type='hidden' name='QuestionToAdd.TextParameters[" + i + "].Key' id='QuestionToAdd_TextParameters_" + i + "__Key' value='" + data.textParameters[i] + "' /> \
                                                  </div>");

                            // Check if this is a global parameters
                            if (globalParams.parameters) {
                                globalParams.parameters.forEach(function (item) {
                                    if (item.key === data.textParameters[i]) {
                                        exists = true;
                                        paramValue = item.value;
                                        return;
                                    }
                                });
                            }
                            if (exists) {
                                container.append("<div class='form-group col-6'> " + paramValue + " \
                                                    <input type='hidden' data-paramkey='"+ data.textParameters[i] + "' class='paramValue' value='" + paramValue + "' /> \
                                                    </div>");
                            }
                            else {
                                if (data.textParameters[i].startsWith('Profile.')) {
                                    // Get the value for the current user
                                    var profileValue = $("#CurrentUserProfile_" + data.textParameters[i].substr(8)).val();
                                    container.append("<div class='form-group col-6'>" + profileValue + " \
                                                            <input type='hidden' data-paramkey='"+ data.textParameters[i] + "' class='paramValue' value='" + profileValue + "' /> \
                                                            <span class='badge badge-warning' data-toggle='tooltip' data-placement='top' title='This is your value. Each user will automatically get their own based on their profile'>(note)</span> \
                                                          </div>");
                                }
                                else {
                                    container.append("<div class='form-group col-6'> \
                                                        <input class='form-control paramValue' data-paramkey='"+ data.textParameters[i] + "' name='QuestionToAdd.TextParameters[" + i + "].Value' id='QuestionToAdd_TextParameters_" + i + "__Value' required oninput='populatePreview();' /> \
                                                    </div>");
                                    numOfEmptyInputsText += 1;
                                }
                            }
                            exists = false;
                            paramValue = '';
                        }


                        if (numOfEmptyInputsText > 0)
                            $("#requiredInputsText").text(numOfEmptyInputsText);


                        // Construct the uri tabs
                        container = $("#uriTab");
                        for (i = 0; i < data.uris.length; i++) {

                            container.append(
                                "<li class='nav-item'> \
                                        <a class='nav-link uriNavItem' id='uri-"+ i + "-tab' data-toggle='tab' href='#uri-" + i + "-content' role='tab' aria-controls='uri-" + i + "-content' aria-selected='false' data-index='" + i + "'> Uri #" + (i + 1) + "</a> \
                                    </li>"
                            );

                            var toAppend = "<div class='tab-pane fade' id='uri-" + i + "-content' role='tabpanel' aria-labelledby='uri" + i + "-tab'> \
                                                    <input type='hidden' name='QuestionToAdd.Uris["+ i + "].Id' id='QuestionToAdd_Uris_" + i + "__Id' value='" + data.uris[i].id + "' /> \
                                                    <div class='input-group mb-3'> \
                                                        <div class= 'input-group-prepend btn-group-toggle' data-toggle='buttons' > \
                                                            <span class='input-group-text'>"+ data.uris[i].callType + "</span> \
                                                            <input type='hidden' name='QuestionToAdd.Uris["+ i + "].CallType' id='QuestionToAdd_Uris_" + i + "__CallType' value='" + data.uris[i].callType + "' /> \
                                                        </div> \
                                                        <input name='QuestionToAdd.Uris["+ i + "].Uri' id='QuestionToAdd_Uris_" + i + "__Uri' data-index='" + i + "' class='form-control' value='" + data.uris[i].uri + "' readonly /> \
                                                    </div > \
                                                    <br /> \
                                                    <div class='form-group row'>";

                            for (var j = 0; j < data.uris[i].uriParameters.length; j++) {

                                toAppend += "<div class='form-group col-6'> \
                                                    <label>"+ data.uris[i].uriParameters[j] + "</label> \
                                                    <input type='hidden' name='QuestionToAdd.Uris[" + i + "].UriParameters[" + j + "].Key' id='QuestionToAdd_Uris_" + i + "__UriParameters_" + j + "__Key' value='" + data.uris[i].uriParameters[j] + "' /> \
                                                 </div>";

                                // Check if this is a global parameters
                                if (globalParams.parameters) {
                                    globalParams.parameters.forEach(function (item) {
                                        if (item.key === data.uris[i].uriParameters[j]) {
                                            exists = true;
                                            paramValue = item.value;
                                            return;
                                        }
                                    });
                                }
                                if (exists) {
                                    toAppend += "<div class='form-group col-6'> " + paramValue + " \
                                                     </div>";
                                }
                                else {
                                    if (data.uris[i].uriParameters[j].startsWith('Profile.')) {
                                        // Get the value for the current user
                                        var profileValue = $("#CurrentUserProfile_" + data.uris[i].uriParameters[j].substr(8)).val();
                                        toAppend += "<div class='form-group col-6'>" + profileValue + " <span class='badge badge-warning' data-toggle='tooltip' data-placement='top' title='This is your value. Each user will automatically get their own based on their profile'>(note)</span></div>";
                                    }
                                    else {
                                        toAppend += "<div class='form-group col-6'> \
                                                            <input class='form-control' name='QuestionToAdd.Uris[" + i + "].UriParameters[" + j + "].Value' id='QuestionToAdd_Uris_" + i + "__UriParameters_" + j + "__Value' required /> \
                                                         </div>";
                                        numOfEmptyInputsUri += 1;
                                    }
                                }

                                exists = false;
                                paramValue = '';
                            }
                            toAppend += "       </div> \
                                                </div>";
                            $("#uriTabContent").append(toAppend);

                            // Add a new answer tab (every question will have a answer associated with it)
                            $("#answerTab").append(
                                "<li class='nav-item'> \
                                        <a class='nav-link answerNavItem' id='answer-" + i + "-tab' data-toggle='tab' href='#answer-" + i + "-content' role='tab' aria-controls='answer-" + i + "-content' aria-selected='false'>Answer for Uri #" + (i + 1) + "</a> \
                                    </li>"
                            );

                            // Add a new tab content for the answer
                            $("#answerTabContent").append(
                                "<div class='tab-pane fade' id='answer-" + i + "-content' role='tabpanel' aria-labelledby='answer-" + i + "-tab'> \
                                        <input type='hidden' name='QuestionToAdd.Answers[" + i + "].AssociatedUriId' id='QuestionToAdd_Answers_" + i + "__AssociatedUriId' value='" + i + "' /> \
                                        <div class= 'form-group input-group mb-3'> \
                                            <div class='col-5 pl-0'> \
                                                <label class='pr-3'>Answer will be checked against:</label> \
                                            </div> \
                                            <div class='col-4'> \
                                                <div class='custom-radio'> \
                                                    <label class='pr-4'> \
                                                        <input type='radio' name='QuestionToAdd.Answers[" + i + "].ResponseType' id='QuestionToAdd_Answers_" + i + "__ResponseType' value='BODY' id='respBODY' autocomplete='off' checked> Body \
                                                    </label> \
                                                </div> \
                                            </div> \
                                            <div class='col-3 text-right pr-0'> \
                                                <button type='button' class='btn btn-info addAnswerParameter' data-index='" + i + "'>Add parameter</button> \
                                            </div> \
                                        </div> \
                                        <div id='Answers_" + i + "__params' class='form-group row'> \
                                        </div > \
                                    </div>"
                            );
                        }

                        if (numOfEmptyInputsUri > 0)
                            $("#requiredInputsUri").text(numOfEmptyInputsUri);
                        if (numOfEmptyInputsAnswers > 0)
                            $("#requiredInputsAnswer").text(numOfEmptyInputsAnswers);

                        if (numOfEmptyInputsText + numOfEmptyInputsUri + numOfEmptyInputsAnswers > 0)
                            $("#btnModalSave").addClass('d-none');


                        $("#uri-0-tab").trigger('click');
                        $("#answer-0-tab").trigger('click');
                        populatePreview();
                    }
                    else {
                        $("#modalError").removeClass('d-none');
                        $(".modal-footer").removeClass('d-none');
                        $("#btnModalSave").addClass('d-none');
                    }
                });
        });
}

var populatePreview = function () {

    var text = $("#QuestionToAdd_Text").val();

    $("#textParamsInputGroup input.paramValue").each(function () {
        var paramKey = '{' + $(this).attr('data-paramkey') + '}';
        var paramValue = $(this).val();

        if (paramValue)
            text = text.replace(paramKey, paramValue);
    })

    $("#textParamsPreview").text(text);
}