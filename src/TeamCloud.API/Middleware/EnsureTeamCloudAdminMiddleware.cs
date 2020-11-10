// /**
//  *  Copyright (c) Microsoft Corporation.
//  *  Licensed under the MIT License.
//  */

// using System;
// using System.Linq;
// using System.Net;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Http;
// using Newtonsoft.Json;
// using TeamCloud.API.Data.Results;
// using TeamCloud.Data;

// namespace TeamCloud.API.Middleware
// {
//     public class EnsureTeamCloudAdminMiddleware : IMiddleware
//     {
//         private static bool HasAdmin;

//         private readonly IUserRepository userRepository;

//         public EnsureTeamCloudAdminMiddleware(IUserRepository userRepository)
//         {
//             this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
//         }

//         public async Task InvokeAsync(HttpContext context, RequestDelegate next)
//         {
//             if (context is null)
//                 throw new ArgumentNullException(nameof(context));

//             if (next is null)
//                 throw new ArgumentNullException(nameof(next));

//             // teamcloud needs a at least one admin user to work properly.
//             // to avoid calls that will fail because of a missing user
//             // we will check its existance in this middleware and block
//             // calls until at least one admin user is in place.

//             // as we ensure there is at least one admin user in the delete
//             // and update apis we can keep the HasAdmin state once it is
//             // evaluated to true to avoid unnecessary request to the
//             // teamcloud repository in the future.

//             HasAdmin = HasAdmin || await AdminExistsAsync().ConfigureAwait(false);

//             if (HasAdmin)
//             {
//                 await next(context).ConfigureAwait(false);
//             }
//             else
//             {
//                 context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

//                 var error = ErrorResult.BadRequest("Must POST an Admin user to api/admin/users before calling any other APIs.", ResultErrorCode.ValidationError);
//                 var errorJson = JsonConvert.SerializeObject(error);

//                 await context.Response
//                     .WriteAsync(errorJson)
//                     .ConfigureAwait(false);
//             }

//             async Task<bool> AdminExistsAsync()
//             {
//                 var exists = await userRepository
//                     .ListAdminsAsync()
//                     .AnyAsync()
//                     .ConfigureAwait(false);

//                 return exists;
//             }
//         }
//     }
// }
