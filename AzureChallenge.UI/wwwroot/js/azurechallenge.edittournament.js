$(document).ready(function () {
    $.validator.setDefaults({ ignore: '' });
    $('[data-toggle="tooltip"]').tooltip()

    $("#btnAddToList").on('click', function () {

        // Clear the modal
        $("#uriTab").empty();
        $("#uriTabContent").empty();
        $("#answerTab").empty();
        $("#answerTabContent").empty();
        $("#requiredInputsText").text('');
        $("#requiredInputsUri").text('');
        $("#requiredInputsAnswer").text('');

        //// Get the question to associate
        var selectedQuestionName = $("#questionsSelector").val();
        var selectedQuestionId = $("#questionsSelector")[0].selectedOptions[0].attributes[0].nodeValue;
        var tournamentId = $("#Id").val();

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
                $("#QuestionToAdd_TournamentId").val(tournamentId);

                $.get("/Administration/Tournament/" + tournamentId + "/GlobalParameters/Get")
                    .done(function (globalParams) {

                        $("#modalWaiting").hide();
                        var numOfEmptyInputsText = 0;
                        var numOfEmptyInputsUri = 0;
                        var numOfEmptyInputsAnswers = 0;

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
                                if (globalParams.parameters[data.textParameters[i].replace('Global.', '')]) {
                                    container.append("<div class='form-group col-6'> " + globalParams.parameters[data.textParameters[i].replace('Global.', '')] + " \
                                                    </div>");
                                }
                                else {
                                    if (data.textParameters[i].startsWith('Profile.'))
                                        container.append("<div class='form-group col-6'>Will be filled from profile data</div>");
                                    else {
                                        container.append("<div class='form-group col-6'> \
                                                        <input class='form-control' name='QuestionToAdd.TextParameters[" + i + "].Value' id='QuestionToAdd_TextParameters_" + i + "__Value' required /> \
                                                    </div>");
                                        numOfEmptyInputsText += 1;
                                    }
                                }
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
                                                        <div class= 'input-group-prepend btn-group-toggle' data - toggle='buttons' > \
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
                                    if (globalParams.parameters[data.uris[i].uriParameters[j].replace('Global.', '')]) {
                                        toAppend += "<div class='form-group col-6'> " + globalParams.parameters[data.uris[i].uriParameters[j].replace('Global.', '')] + " \
                                                     </div>";
                                    }
                                    else {
                                        if (data.uris[i].uriParameters[j].startsWith('Profile.'))
                                            toAppend += "<div class='form-group col-6'>Will be filled from profile data</div>";
                                        else {
                                            toAppend += "<div class='form-group col-6'> \
                                                            <input class='form-control' name='QuestionToAdd.Uris[" + i + "].UriParameters[" + j + "].Value' id='QuestionToAdd_Uris_" + i + "__UriParameters_" + j + "__Value' required /> \
                                                         </div>";
                                            numOfEmptyInputsUri += 1;
                                        }
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

                                // Add a new tab content for the answer
                                // We need to add at least one answer parameter to ensure validation works and force the user to add an answer :)
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
                                                        <input type='radio' name='QuestionToAdd.Answers[" + i + "].ResponseType' id='QuestionToAdd_Answers_" + i + "__ResponseType' value='HEAD' id='respHEAD' autocomplete='off' checked> Headers \
                                                    </label> \
                                                    <label> \
                                                        <input type='radio' name='QuestionToAdd.Answers[" + i + "].ResponseType' id='QuestionToAdd_Answers_" + i + "__ResponseType' value='BODY' id='respBODY' autocomplete='off'> Body \
                                                    </label> \
                                                </div> \
                                            </div> \
                                            <div class='col-3 text-right pr-0'> \
                                                <button type='button' class='btn btn-info addAnswerParameter' data-index='" + i + "'>Add parameter</button> \
                                            </div> \
                                        </div> \
                                        <div id='Answers_" + i + "__params' class='form-group row'> \
                                            <div class='form-group col-6' > \
                                                <label>Path</label> \
                                            <input class='form-control answerParamInput answerParamInputKey' name='QuestionToAdd.Answers[" + i + "].AnswerParameters[0].Key' id='QuestionToAdd_Answers_" + i + "_AnswerParameters_0__Key'' data-index='0' required /></div> \
                                            <div class='form-group col-6'> \
                                                <label>Value to check</label> \
                                            <input class='form-control answerParamInput answerParamInputVal' name='QuestionToAdd.Answers[" + i + "].AnswerParameters[0].Value' id='QuestionToAdd_Answers_" + i + "_AnswerParameters_0__Value' data-index='0' required /></div> \
                                        </div > \
                                    </div>"
                                );
                                numOfEmptyInputsAnswers += 2;
                            }

                            if (numOfEmptyInputsUri > 0)
                                $("#requiredInputsUri").text(numOfEmptyInputsUri);
                            if (numOfEmptyInputsAnswers > 0)
                                $("#requiredInputsAnswer").text(numOfEmptyInputsAnswers);

                            if (numOfEmptyInputsText + numOfEmptyInputsUri + numOfEmptyInputsAnswers > 0)
                                $("#btnModalSave").addClass('d-none');


                            $("#uri-0-tab").trigger('click');
                            $("#answer-0-tab").trigger('click');
                        }
                        else {
                            $("#modalError").removeClass('d-none');
                            $(".modal-footer").removeClass('d-none');
                            $("#btnModalSave").addClass('d-none');
                        }
                    });
            });
    });

    $("#associatedQuestionsTable .tableLinkEdit").on('click', function () {
        var selectedQuestionId = $(this).attr('data-questionId');
        var tournamentId = $("#Id").val();

        populateModal(selectedQuestionId, tournamentId, false)
    })

    $("#associatedQuestionsTable .tableLinkDetails").on('click', function () {
        var selectedQuestionId = $(this).attr('data-questionId');
        var tournamentId = $("#Id").val();

        populateModal(selectedQuestionId, tournamentId, true)
    })

    $("#associatedQuestionsTable .tableLinkCheck").on('click', function () {
        var questionId = $(this).attr('data-questionId');

        // Clear the contents
        $("#checkModalContentDiv").empty();

        $.get("/Administration/Tournament/ValidateQuestion?questionId=" + questionId)
            .done(function (data) {
                $("#checkModalWaiting").hide();

                $("#checkModalContent").removeClass('d-none');

                data.forEach(function (item) {
                    $("#checkModalContentDiv").append("<div class='col-md-6'>" + item.Key + "</div><div class='col-md-6'>" + (item.Value ? "<span class='text-success'>Success</span>" : "<span class='text-danger'>Failure</span>")+"</div>");
                });
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

        container.append("<div class='form-group col-6'> \
                            <label>Path</label> \
                            <input class='form-control answerParamInput answerParamInputKey' name='QuestionToAdd.Answers[" + itemIndex + "].AnswerParameters[" + currentIndex + "].Key' id='QuestionToAdd_Answers_" + itemIndex + "_AnswerParameters_" + currentIndex + "__Key'' data-index='" + currentIndex + "' required /></div>");
        container.append("<div class='form-group col-6'> \
                            <label>Value to check</label> \
                            <input class='form-control answerParamInput answerParamInputVal' name='QuestionToAdd.Answers[" + itemIndex + "].AnswerParameters[" + currentIndex + "].Value' id='QuestionToAdd_Answers_" + itemIndex + "_AnswerParameters_" + currentIndex + "__Value' data-index='" + currentIndex + "' required /></div>");

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

        var numOfEmptyInputsInTab = $("#" + tab_pane_id + " input").filter(function () { return !$(this).val(); }).length;

        if (numOfEmptyInputsInTab > 0)
            $(span).text(numOfEmptyInputsInTab);
        else
            $(span).text('');

        var numOfEmptyInputsInAllTabs = $("#modalTabsContent input").filter(function () { return !$(this).val(); }).length;

        if (numOfEmptyInputsInAllTabs === 0)
            $("#btnModalSave").removeClass('d-none');
        else
            $("#btnModalSave").addClass('d-none');

    });
});

var populateModal = function (selectedQuestionId, tournamentId, readOnly = false) {

    // Clear the modal
    $("#uriTab").empty();
    $("#uriTabContent").empty();
    $("#answerTab").empty();
    $("#answerTabContent").empty();
    $("#requiredInputsText").text('');
    $("#requiredInputsUri").text('');
    $("#requiredInputsAnswer").text('');

    $.get("/Administration/Tournament/" + tournamentId + "/AssignedQuestion/" + selectedQuestionId)
        .done(function (data) {
            $("#QuestionToAdd_Text").val(data.text);
            $("#QuestionToAdd_AssociatedQuestionId").val(data.id);
            $("#QuestionToAdd_Name").val(data.name);
            $("#QuestionToAdd_TargettedAzureService").val(data.targettedAzureService);
            $("#QuestionToAdd_Difficulty").val(data.difficulty);
            $("#QuestionToAdd_Description").val(data.description);
            $("#QuestionToAdd_TournamentId").val(tournamentId);

            $.get("/Administration/Tournament/" + tournamentId + "/GlobalParameters/Get")
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
                        if (globalParams.parameters[data.textParameters[i].key.replace('Global.', '')]) {
                            container.append("<div class='form-group col-6'> " + globalParams.parameters[data.textParameters[i].key.replace('Global.', '')] + " \
                                                    </div>");
                        }
                        else {
                            container.append("<div class='form-group col-6'> \
                                                <input class='form-control' name='QuestionToAdd.TextParameters[" + i + "].Value' id='QuestionToAdd_TextParameters_" + i + "__Value' required /> \
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
                                                        <div class= 'input-group-prepend btn-group-toggle' data - toggle='buttons' > \
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
                                                    <input type='hidden' name='QuestionToAdd.Uris[" + i + "].UriParameters[" + j + "].Key' id='QuestionToAdd_Uris_" + i + "__UriParameters_" + j + "__Key' value='" + data.uris[i].uriParameters[j] + "' /> \
                                                 </div>";

                            // Check if this is a global parameters
                            if (globalParams.parameters[data.uris[i].uriParameters[j].key.replace('Global.', '')]) {
                                toAppend += "<div class='form-group col-6'> " + globalParams.parameters[data.uris[i].uriParameters[j].key.replace('Global.', '')] + " \
                                                     </div>";
                            }
                            else {
                                if (data.uris[i].uriParameters[j].key.startsWith('Profile.'))
                                    toAppend += "<div class='form-group col-6'>Will be filled from profile data</div>";
                                else
                                    toAppend += "<div class='form-group col-6'> \
                                                            <input class='form-control' name='QuestionToAdd.Uris[" + i + "].UriParameters[" + j + "].Value' id='QuestionToAdd_Uris_" + i + "__UriParameters_" + j + "__Value' required /> \
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

                            toAppend += "<div class='form-group col-6'> \
                                            <label>Path</label> \
                                        <input class='form-control answerParamInput answerParamInputKey' name='QuestionToAdd.Answers[" + i + "].AnswerParameters[" + j + "].Key' id='QuestionToAdd_Answers_" + i + "_AnswerParameters_" + j + "__Key' data-index='" + j + "' required " + (readOnly ? "readonly" : "") + " /></div>";
                            toAppend += "<div class='form-group col-6'> \
                                            <label>Value to check</label> \
                                        <input class='form-control answerParamInput answerParamInputVal' name='QuestionToAdd.Answers[" + i + "].AnswerParameters[" + j + "].Value' id='QuestionToAdd_Answers_" + i + "_AnswerParameters_" + j + "__Value' data-index='" + j + "' required " + (readOnly ? "readonly" : "") + " /></div>";
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
                                                <input type='radio' name='QuestionToAdd.Answers[" + i + "].ResponseType' id='QuestionToAdd_Answers_" + i + "__ResponseType' value='HEAD' id='respHEAD' autocomplete='off' " + (data.answers[i].responseType === "HEAD" ? "checked" : "") + " " + (readOnly ? "disabled" : "") + "> Headers \
                                            </label> \
                                            <label> \
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
                        }
                    }

                    $("#uri-0-tab").trigger('click');
                    $("#answer-0-tab").trigger('click');
                });
        })
        .fail(function () {
            $("#modalError").removeClass('d-none');
            $(".modal-footer").removeClass('d-none');
            $("#btnModalSave").addClass('d-none');
        });
}

