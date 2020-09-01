# TeamCloud Web Client

## App registration

In order for the TeamCloud Web application to authenticate users and securely call the API, you must create a new app registration in Azure Active Directory:

### Create the app registration

1. Sign in to the [Azure portal][azure-portal]. If your account has access to multiple tenants, select the **Directory + Subscription** filter in the top menu, and then select the tenant that should contain the app registration you're about to create.
2. Search for and select **Azure Active Directory**.
3. Under **Manage**, select **App registrations**.
4. Select **New registration** at the top:
   - For the application **Name**, enter **TeamCloud.Web**.
   - Under **Supported account types** select **Accounts in this organizational directory only (Default Directory only - Single tenant)**.
   - Do **NOT** enter a **Redirect URI**.
5. Select **Register** to create the app registration.
6. Copy the **Application (client) ID** and **Directory (tenant) ID** at the top of the page, as we'll need them later.

Next, configure the app registration with a Redirect URI (and scope) to specify where the Microsoft identity platform should redirect the client along with any security tokens.

### Add a scope to the app registration

1. _If you're continuing from [**Create the app registration**](#create-the-app-registration) skip to **#5** below._ Otherwise, sign in to the [Azure portal][azure-portal]. If your account has access to multiple tenants, select the **Directory + Subscription** filter in the top menu, and then select the tenant that contains the application named **TeamCloud.Web** that you created in [**Create the app registration**](#create-the-app-registration) above.
2. Search for and select **Azure Active Directory**.
3. Under **Manage**, select **App registrations**.
4. Select application named **TeamCloud.Web** that you created in [**Create the app registration**](#create-the-app-registration) above.
5. If you haven't already, copy the **Application (client) ID** and **Directory (tenant) ID** at the top of the page, as we'll need them later.
6. Under **Manage**, select **Expose an API**.
7. Select **Add a scope**.
8. For the **Application ID URI**, enter **http://TeamCloud.Web**.
9. Select **Save and continue**, and fill in the form as follows:
   - **Scope name**: user_impersonation
   - **Who can consent?**: Admins and users
   - **Admin consent display name**: Access TeamCloud
   - **Admin consent description**: Allow the application to access TeamCloud on behalf of the signed-in user.
   - **User consent display name**: Access TeamCloud
   - **User consent description**: Allow the application to access TeamCloud on your behalf.
   - **State**: Enabled
10. Select **Add scope** to add the scope.

### Add a platform to the app registration

1. _If you're continuing from [**Create the app registration**](#create-the-app-registration) skip to **#5** below._ Otherwise, sign in to the [Azure portal][azure-portal]. If your account has access to multiple tenants, select the **Directory + Subscription** filter in the top menu, and then select the tenant that contains the application named **TeamCloud.Web** that you created in [**Create the app registration**](#create-the-app-registration) above.
2. Search for and select **Azure Active Directory**.
3. Under **Manage**, select **App registrations**.
4. Select application named **TeamCloud.Web** that you created in [**Create the app registration**](#create-the-app-registration) above.
5. If you haven't already, copy the **Application (client) ID** and **Directory (tenant) ID** at the top of the page, as we'll need them later.
6. Under **Manage**, select **Authentication**, and then select **Add a platform**.
7. Under **Web applications**, select **Single-page application** tile.
8. Under **Redirect URIs**, enter a [redirect URI][reply-url].
9. Leave **Logout URL** empty.
10. Under **Implicit grant**, make sure both **Access tokens** and **ID tokens** are **selected**.
11. Select **Configure** to finish adding the redirect URI.

[azure-portal]:https://portal.azure.com/
[reply-url]:https://docs.microsoft.com/en-us/azure/active-directory/develop/reply-url
