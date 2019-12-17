# User

***\*This file is incomplete. It is a work in progress and will change.\****

## Properties

| Property | Type   | Description |
|:---------|:-------|:------------|
| Id       | String | Unique user Id (corresponds to the AAD ObjectID) |
| Role     | [TeamCloudUserRole](#teamclouduserrole) or [ProjectUserRole](#projectuserrole) | |
| Tags     | Dictionary\<String:String\> | |

## TeamCloud Users vs. Project Users

In the context of TeamCloud, a User corresponds to a user from the associated Azure AD tenant that has been given access to the TeamCloud instance or a specific Project managed by the instance.

To handle these different access levels, there are two types of Users:

### TeamCloud Users

TeamCloud Users are associated with the TeamCloud instance.  These users have permissions to administer the TeamCloud instance itself and/or create new Projects.  These permissions are represented by the `TeamCloudUserRole` enumeration below.

#### TeamCloudUserRole

| Role    | Description |
|:--------|:------------|
| Admin   | TeamCloud Users with the Admin role can manage all TeamCloud Users, as well as all Projects and Providers associated with the TeamCloud instance.  This role implicitly has all the permissions of the Creator role below. |
| Creator | TeamCloud Users with the Creator role create new Projects in the TeamCloud instance and make changes to the Projects they created, including managing the Project Users. |

### Project Users

Project Users are associated with a specific Project either as an owner or a member, and have no implicit permissions on the TeamCloud instance.  These permissions are represented by the `ProjectUserRole` enumeration below.

#### ProjectUserRole

| Role   | Description |
|:-------|:------------|
| Owner  | Project Users with the Owners role have read and write permissions on the Project.  They can update and delete the Project and manage the Project's users. |
| Member | Project Users with the Member role have read permissions on the Project. |

Note: The creator of the Project is automatically assigned the role of Owner on the Project.  Project Owners (and TeamCloud Users with the Admin role) can assign additional Owners to the Project.

## Provider Resources

When a User is added to a TeamCloud Project, they are given permission to make authenticated calls to the [TeamCloud Project API](../API.md#teamcloud-project-api).  However, the User is **not** automatically assigned any permissions on resources created and managed by the the Project's Providers.

It is the responsibility of the Provider to assign Project Users the appropriate permissions on its resource(s).  When Users are added, updated, or removed from a Project, each Provider will be notified and provided the correct list of Project Users.
