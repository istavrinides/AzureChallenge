$(document).ready(function () {
    var textIndexer = 0;
    var uriIndexer = 0;
    var answerIndexer = 0;

    $("#Text").on('input', function () {
        var foundParameters = $(this).val().match(/\{([^}]+)\}/g);

        if (foundParameters.some(param => param.slice(1, -1).includes('{') || param.slice(1, -1).includes('{')))
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
            textIndexer = 0;

            foundParameters.forEach(function (item) {

                item = item.slice(1, -1);

                var oldValue = "";
                if (item in dict)
                    oldValue = dict[item];

                container.append("<div class='form-group col-6'> \
                                                        <label>Name</label> \
                                                        <input class='form-control' name='TextParameters[" + textIndexer + "].Key' data-textparam='" + item + "' value='" + item + "' disabled  /></div>");
                container.append("<div class='form-group col-6'> \
                                                        <label>Value</label> \
                                                        <input class='form-control' name='TextParameters[" + textIndexer + "].Value' data-textparamval='" + item + "' value='" + oldValue + "' /></div>");
                container.append("<input type='hidden' name='TextParameters.Index' value='" + textIndexer + "' />");

                textIndexer += 1;


            });
        }
        else {
            container.empty();
        }
    })

    $("#btn-add-new-text-param").click(function () {
        var container = $("#textParamsInputGroup");

        container.append("<div class='form-group col-6'> \
                                                        <label>Name</label> \
                                                        <input class='form-control' name='TextParameters[" + textIndexer + "].Key' /></div>");
        container.append("<div class='form-group col-6'> \
                                                        <label>Value</label> \
                                                        <input class='form-control' name='TextParameters[" + textIndexer + "].Value' /></div>");
        container.append("<input type='hidden' name='TextParameters.Index' value='" + textIndexer + "' />");

        textIndexer += 1;

        return false;
    });
    $("#btn-add-new-uri-param").click(function () {
        var container = $("#uriParamsInputGroup");

        container.append("<div class='form-group col-6'> \
                                                        <label>Name</label> \
                                                        <input class='form-control' name='UriParameters[" + uriIndexer + "].Key' /></div>");
        container.append("<div class='form-group col-6'> \
                                                        <label>Value</label> \
                                                        <input class='form-control' name='UriParameters[" + uriIndexer + "].Value' /></div>");
        container.append("<input type='hidden' name='UriParameters.Index' value='" + uriIndexer + "' />");

        uriIndexer += 1;

        return false;
    });
    $("#btn-add-new-answer-param").click(function () {
        var container = $("#answerParamsInputGroup");

        container.append("<div class='form-group col-6'> \
                                                        <label>Name</label> \
                                                        <input class='form-control' name='Answers[" + uriIndexer + "].Key' /></div>");
        container.append("<div class='form-group col-6'> \
                                                        <label>Value</label> \
                                                        <input class='form-control' name='Asnwers[" + uriIndexer + "].Value' /></div>");
        container.append("<input type='hidden' name='Answers.Index' value='" + uriIndexer + "' />");

        answerIndexer += 1;

        return false;
    });
});