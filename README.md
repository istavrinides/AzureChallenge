# Azure Challenge
Azure Challenge is an ASP.NET Core MVC application that serves as a platform for interactive learning of Azure Services. 
Using the web application, you can define questions and challenges to users participating and check their validity by issuing REST API calls
using the Azure Resource Manager. The working sample is located at https://az-challenge.azurewebsites.net, you are welcome to give it a try!

We assume that everyone participating in the challenge has an Azure Subscription.

## Architecture
The architecture of the application consists of:
- An Azure Web App hosted in an Azure App Service instance.
- An Azure SQL Database hosting the authentication database.
- An Azure Cosmos Db Core API Database that hosts all the web application data.

## How to use it
The web application has the following main functional sections:
- [Profile administration](#profile-administration) under the Identity Area
- [Question templates](#question-templates) under the Administration Area (needs "Administrator" role)
- [Challenges](#challenges) under the Administration Area (needs "Administrator" role)
- Challenges under the default Area. This provides a list of Public Challenges the user can start or continue.

### Profile administration
Based on the out-of-the-box experience provided by MVC Core, some minor functionality has been extended:
- Email handling
- User profile extention: additional fields have been added to the User class to accomodate profile information useful for the system (such as the Azure Subscription Id etc)
- The backed has been kept as an Azure SQL Database

### Question templates
To keep the system as customizable as possible, questions are created in the system as templates. The following information is required when adding a new Question Template:
- **Name**
- **Description**
- **Targetted Azure Service**: allows for grouping questions
- **Difficulty**: Defines the points given to a successful completion of the question
  - **Easy**: 100 points
  - **Medium**: 200 points
  - **Hard**: 300 points
  - **Expert**: 400 points
- **Question**: The question to be presented to the user. To keep it as generic as possible, the system supports placeholders. A placeholder is defined by curly braces, for example, \{ResourceGroupName}. Later on during the definition of the Challenge, you will provide actual values to these placeholders. There are 3 types of placeholders:
  - **Global**: The scope of the placeholder concerns a Challenge. The name should be prefixed with "Global.", for example: \{Global.PrimaryRegion}. For every question in a challenge that this placeholder exists, a single value will be used across all questions. The placeholders are defined as Global Challenge Parameters.
  - **Profile**: In some cases, you might need to customize the question with information from the user's profile. The following profile values can be exposed (using the respective placeholders below):
    - \{Profile.UserNameHashed}: Returns a hash value based on the user's username. Since every username is unique in the database, this creates a unique identifier that can be applied in various instances (for example, whenever we need to ensure unique service names in Azure)
    - \{Profile.SubscriptionId}: The user's Subscription Id
    - \{Profile.TenantId}: The user's Tenant Id
  - **Local**: Any other placeholder. These are applied (as a value) only at the current question level
- **Justification**: Message to show if the user successfully answers the question. Serves as a way to offer an explanation, usefulness etc.
- **Useful links**: List of URLs that could help the user answer the question.
- **Uri endpoints to call**: A list of URI's the backed will call to validate the question. URIs can be parameterized with placeholders exactly in the same manner as the question text.
- **

### Challenges
Challenges is the way we define
- The sequence of questions
- The values to use for the placeholders (in the question text and URIs)
- What to check for a correct answer

Creating a Challenge requires the following information:
- **Name**
- **Description**

In the challenge screen, you have the following capabilities:
- Navigate to the challenge parameters screen
- Edit the challenge (more below)
- Copy the challenge: creates an exact copy of the challenge (preferably with a diffent name). The result is a Private, Unlocked challenge
- Locked status shows that at least one user has started the Challenge
- Delete: deletes the challenge (if not locked/started by someone)

Once created, you will need to **edit** the challenge. In the edit page, you will have the following options:
- Change the challenge to Public or Private. Public challenges are visible to any use in the Challenge list page, Private challenges require the creator of the challenge to provide a unique code to the participants. This is useful if we want to limit participation in a challenge.
- Associated a question to the challenge. From the combo box, you can select a question template to add to the challenge. The question (if added) is added last to the list.
- Change the ordering of the questions
- Edit, View or Delete a question
- Check a question: This validates the question definition (including substitution with the placeholder values) against the current user's profile (so the user setting up the challenge should have their Profile set up).

When adding a question, you are essentially "hydrating" the question. In this phase, we are leveraging the defined question template to add the specific paramaters/placeholder values for the specific challenge/question. When adding (or editing) a question, you need to do the following in the modal that will be shown:
- Fill in any parameter values. If a Global parameter has been defined in the Challenge parameters or defined in a previous question, that value will be used.
- Add answer parameters. If you don't add any, the question will be considered correct it the corresponding URI call returns an HTTP status code 200. In case where the URI returns a JSON document, you can check values within the document. To do this, the following are supported:
  - Path: 
    - A dot separated value that defines the JSON path to the desired property. For example, for the below JSON, to validate the value "myValue", you would give a path of **Path.To.Property** (case sensitive):
    ```    
    {
        "Path": {
            "To": {
                "Property": "myValue"
            }
        }
    }
    ```
    - A dot separated value with square brackets to denote a specific value in an array for a specific property. For example, for the below JSON, you would provide a path of **Path.To.Property[Name=Name2].MyProperty** and check the value **Value**
    ```    
    {
        "Path": {
            "To": {
                "Property": [
                    {
                        "Name": "Name1",
                        "MyProperty": "SomeValue"
                    },
                    {
                        "Name": "Name2",
                        "MyProperty": "Value"
                    }
                ]
            }
        }
    }
    ```
## License
Copyright (c) 2020 Ioannis Stavrinides

Full disclaimer, at the time of creation, I am a Cloud Solution Architect working for Microsoft Corp. However, this is a project I created on my own, free time.

Licensed under the MIT license.

Free as in Bacon.
