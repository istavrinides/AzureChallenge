## Azure Challenge - Exported Challenges

You can create Challenges through the web application and export them. At that point, you get a zip archive that contains the correct data in the correct format. If you will manually change the contents, please note:
- The challenge definition needs to be in a file named **challenge.json**
- You should have a **challengeParameters.json** which will have the challenge parameters.
- You should have a **globalParams.json** that contains any necessary global parameter definitions. These will be appended to the existing global parameters.
- All assigned questions (to the challenge) must have the following file name pattern: aq-<assigned_question_id>.json
- All question templates must have the following file name pattern: q-<question_id>.json

Any zip files that are submitted as pull requests that do not conform to this format will be rejected.