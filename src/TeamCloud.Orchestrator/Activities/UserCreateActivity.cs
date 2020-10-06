/**
 *  Copyright (c) Microsoft Corporation.
 *  Licensed under the MIT License.
 */

// using System;
// using System.Threading.Tasks;
// using Microsoft.Azure.WebJobs;
// using Microsoft.Azure.WebJobs.Extensions.DurableTask;
// using TeamCloud.Data;
// using TeamCloud.Model.Data;

// namespace TeamCloud.Orchestrator.Activities
// {
//     public class UserCreateActivity
//     {
//         private readonly IuserRepository userRepository;

//         public UserCreateActivity(IuserRepository userRepository)
//         {
//             this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
//         }

//         [FunctionName(nameof(UserCreateActivity))]
//         public async Task<User> RunActivity(
//             [ActivityTrigger] User user)
//         {
//             if (user is null)
//                 throw new ArgumentNullException(nameof(user));

//             user = await userRepository
//                 .AddAsync(user)
//                 .ConfigureAwait(false);

//             return user;
//         }
//     }
// }
