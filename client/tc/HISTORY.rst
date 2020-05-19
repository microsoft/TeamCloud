.. :changelog:

Release History
===============

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
