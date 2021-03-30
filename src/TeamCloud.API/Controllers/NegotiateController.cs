// /**
//  *  Copyright (c) Microsoft Corporation.
//  *  Licensed under the MIT License.
//  */

// using System;
// using System.Collections.Generic;
// using System.Diagnostics.CodeAnalysis;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Swashbuckle.AspNetCore.Annotations;
// using TeamCloud.API.Auth;
// using TeamCloud.API.Data;
// using TeamCloud.API.Data.Results;
// using TeamCloud.API.Data.Validators;
// using TeamCloud.API.Services;
// using TeamCloud.Data;
// using TeamCloud.Model.Commands;
// using TeamCloud.Model.Data;
// using TeamCloud.Model.Validation.Data;

// namespace TeamCloud.API.Controllers
// {

//     [ApiController]
//     [Route("orgs/{organizationId:organizationId}/projects/{projectId:projectId}/negotiate")]
//     public class NegotiateController : ApiController
//     {

// private readonly IServiceManager _serviceManager;

// public NegotiateController(IConfiguration configuration)
// {
//     var connectionString = configuration["Azure:SignalR:ConnectionString"];
//     _serviceManager = new ServiceManagerBuilder()
//         .WithOptions(o => o.ConnectionString = connectionString)
//         .Build();
// }
//     }
// }
