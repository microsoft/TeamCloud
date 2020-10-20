.. :changelog:

Release History
===============

0.7.0
++++++
* Preview release
* Minimum Azure CLI version now 2.10.0
* Fix bug when using --no-wait arg
* Add tc project offer list command
* Add tc project offer show command
* Add tc project component create command
* Add tc project component delete command
* Add tc project component list command
* Add tc project component show command

0.6.0
++++++
* Preview release
* Python client now generated with @autorest/python v3

0.5.7
++++++
* Preview release
* Add command to update tc cli extension

0.5.6
++++++
* Preview release
* Add tc app deploy command
* Add tc app upgrade command
* Code cleanup and consolodation

0.5.5
++++++
* Preview release
* model updates

0.5.4
++++++
* Preview release
* az tc [provider] upgrade: checks existing version before upgrading
* Add tc info command

0.5.3
++++++
* Preview release
* az tc [provider] deploy: don't require a location if an existing resource group is provided

0.5.2
++++++
* Preview release
* Add az tc [project] user update commands

0.5.1
++++++
* Preview release
* az tc provider upgrade/deploy: Fix output format
* az tc project create: Add properties param

0.5.0
++++++
* Preview release
* Update models to match API model overhaul (v0.2+)

0.4.6
++++++
* Preview release
* Fix table output for projects and users
* Fix --no-wait

0.4.5
++++++
* Preview release
* az tc [provider] upgrade: update the config/provider data object
* ac tc provider list-available: Fix bug
* Replace log warnings with temporary status output

0.4.4
++++++
* Preview release
* az tc project-type create: Fix provider id validation bug

0.4.3
++++++
* Preview release
* Fix bug in tc upgrade
* Fix bug in user table output

0.4.2
++++++
* Preview release
* Tab completion for providers using index.json
* Additional arg validation
* az tc provider deploy: Open setup url in browser after deployment

0.4.1
++++++
* Preview release
* Updated models

0.4.0
++++++
* Preview release
* Deploy and upgrade TeamCloud instances and Providers using dynamic ARM templates and index.json

0.3.6
++++++
* Preview release
* Help and linter fixes

0.3.5
++++++
* Preview release
* az tc [project] user: Accept user email or id in all commands
* az tc [project] user: Replace tags with properties

0.3.4
++++++
* Preview release
* az tc [provider] deploy/upgrade: Validate version numbers against repo releases
* az tc [provider] deploy/upgrade: Add --pre flag to use the latest prerelease

0.3.3
++++++
* Preview release
* Updated teamcloud python SDK

0.3.2
++++++
* Preview release
* az tc provider deploy: Deploy providers into specific resource groups
* az tc project user delete: Fix incorrect api path
* Tab completion for project types and providers
* Support --no-wait for all create/delete commands
* Add confirmation on user, project type, and provider delete commands

0.3.1
++++++
* Preview release
* az tc deploy: Fix permissions for auto-created service principal
* az tc project-type create: Require --location

0.3.0
++++++
* Preview release
* Fix version validator
* Fix missing help for several parameters
* Update metadata from Alpha to Beta
* Change max core CLI version to 3.0.0
* Drop support for python 3.5
* tc create -> tc deploy
* Simplify tc provider deploy parameters
* Return object instead of strings deploy/upgrade
* Updated teamcloud python client
* Added some options to allow redeployment using tc deploy
* Create system Managed Identity for function apps
* Allow project type names without a period
* Removed 3+ subscriptions requirement for project types
* Fix project type create provider validation

0.2.2
++++++
* Internal preview release.
* Fix version validator for tc create
* Fix missing help for several parameter

0.2.1
++++++
* Internal preview release.

0.2.0
++++++
* Internal preview release.

0.1.0
++++++
* Initial internal development release.
