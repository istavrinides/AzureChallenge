$(document).ready(function () {

    $("#btnAddToList").on('click', function () {

        // Get the question to associate
        var selectedQuestionName = $("#questionsSelector").val();
        var selectedQuestionId = $("#questionsSelector")[0].selectedOptions[0].attributes[0].nodeValue;

        var tableBody = $("#associatedQuestionsTable tbody");
        tableBody.append("<tr><td>" + selectedQuestionName + "</td><td>Edit</td><td>Details</td><td><button type='button' class='btn btn-danger deleteAssociatedQuestion' data-id='" + selectedQuestionId + "'>Delete</button></td>");

        // Disable the option in the selector
        var selectOption = document.getElementById("questionsSelector").getElementsByTagName("option");
        for (var i = 0; i < selectOption.length; i++) {
            if (selectOption[i].value === selectedQuestionName)
                selectOption[i].disabled = true;
        }
        $("#questionsSelector").selectpicker('refresh');
        $("#questionsSelector").selectpicker('val', '');

        $("#btnSave").prop("disabled", false);
        $("#saveAlert").removeClass("d-none");

        // Fill the modal with the initial data
        // Call the API to get the details of the selected question
        $.get("/Administration/Questions/" + selectedQuestionId + "/Get")
            .done(function (data) {
                $("#configureQuestionText").val(data.text);
                $("#AssociatedQuestionId").val(data.id);
                $("#Name").val(data.name);
                $("#TargettedAzureService").val(data.targettedAzureService);
                $("#Difficulty").val(data.difficulty);
                $("#Description").val(data.description);

                // Construct the text parameters
                var container = $("#textParamsInputGroup");
                container.empty();

                for (var i = 0; i < data.textParameters.length; i++) {
                    container.append("<div class='form-group col-6'> \
                                        <label>"+ data.textParameters[i] + "</label> \
                                        <input type='hidden' name='TextParameters[" + i + "].Key' id='TextParameters_" + i + "__Key' value='" + data.textParameters[i] + "' /> \
                                      </div>");
                    container.append("<div class='form-group col-6'> \
                                        <input class='form-control' name='TextParameters[" + i + "].Value' id='TextParameters_" + i + "__Value' required /></div>");
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
                                        <input type='hidden' name='Uris["+ i + "].Id' id='Uris_" + i + "__Id' value='" + data.uris[i].id + "' /> \
                                        <div class='input-group mb-3'> \
                                            <div class= 'input-group-prepend btn-group-toggle' data - toggle='buttons' > \
                                               <span class='input-group-text'>"+ data.uris[i].callType + "</span> \
                                            </div> \
                                            <input name='Uris["+ i + "].Uri' id='Uris_" + i + "__Uri' data-index='" + i + "' class='form-control' value='" + data.uris[i].uri + "' readonly /> \
                                        </div > \
                                        <br /> \
                                        <div class='form-group row'>";

                    for (var j = 0; j < data.uris[i].uriParameters.length; j++) {

                        toAppend += "<div class='form-group col-6'> \
                                                <label>"+ data.uris[i].uriParameters[j] + "</label> \
                                                <input type='hidden' name='Uris[" + i + "].UriParameters[" + j + "].Key' id='Uris_" + i + "__UriParameters_" + j + "__Key' value='" + data.uris[i].uriParameters[j] + "' /> \
                                             </div>";
                        toAppend += "<div class='form-group col-6'> \
                                            <input class='form-control' name='Uris[" + i + "].UriParameters[" + j + "].Value' id='Uris_" + i + "__UriParameters_" + j + "__Value' required /> \
                                         </div>";
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
                            <input type='hidden' name='Answers[" + i + "].AssociatedUriId' id='Answers_" + i + "__AssociatedUriId' value='" + i + "' /> \
                            <div class= 'form-group input-group mb-3'> \
                                <div class='col-5 pl-0'> \
                                    <label class='pr-3'>Answer will be checked against:</label> \
                                </div> \
                                <div class='col-4'> \
                                    <div class='custom-radio'> \
                                        <label class='pr-4'> \
                                            <input type='radio' name='Answers[" + i + "].ResponseType' value='HEAD' id='respHEAD' autocomplete='off' checked> Headers \
                                        </label> \
                                        <label> \
                                            <input type='radio' name='Answers[" + i + "].ResponseType' value='BODY' id='respBODY' autocomplete='off'> Body \
                                        </label> \
                                    </div> \
                                </div> \
                                <div class='col-3 text-right pr-0'> \
                                    <button type='button' class='btn btn-info addAnswerParameter' data-index='" + i + "'>Add parameter</button> \
                                </div> \
                            </div> \
                            <div id='Answers_" + i + "__params' class='form-group row'></div> \
                        </div>"
                    );
                }

                $("#uri-0-tab").trigger('click');
                $("#answer-0-tab").trigger('click');

            });
    });

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
                            <input class='form-control answerParamInput answerParamInputKey' name='Answers[" + itemIndex + "].AnswerParameters[" + currentIndex + "].Key' id='Answers_" + itemIndex + "_AnswerParameters_" + currentIndex + "__Key'' data-index='" + currentIndex + "' required /></div>");
        container.append("<div class='form-group col-6'> \
                            <label>Value</label> \
                            <input class='form-control answerParamInput answerParamInputVal' name='Answers[" + itemIndex + "].AnswerParameters[" + currentIndex + "].Value' id='Answers_" + itemIndex + "_AnswerParameters_" + currentIndex + "__Value' data-index='" + currentIndex + "' required /></div>");

        return false;
    });
});