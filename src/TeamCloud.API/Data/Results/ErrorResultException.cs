using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using TeamCloud.Serialization;
using ErrorResultFactory = TeamCloud.API.Data.Results.ErrorResult;

namespace TeamCloud.API.Data.Results
{
    [Serializable]
    public class ErrorResultException : Exception
    {
        private static string GetErrorMessage(IErrorResult errorResult)
            => errorResult is null
            ? string.Empty
            : string.Join(", ", errorResult.Errors ?? new List<ResultError>());

        public ErrorResultException() { }

        public ErrorResultException(IErrorResult errorResult) : base(GetErrorMessage(errorResult))
        {
            ErrorResult = errorResult ?? throw new ArgumentNullException(nameof(errorResult));
        }

        protected ErrorResultException(SerializationInfo info, StreamingContext context) : base(info, context) 
        {
            var errorResultJson = info.GetString(nameof(ErrorResult));

            if (!string.IsNullOrWhiteSpace(errorResultJson))
                ErrorResult = TeamCloudSerialize.DeserializeObject(errorResultJson) as IErrorResult;
        }

        public IErrorResult ErrorResult { get; private set; } = ErrorResultFactory.ServerError();

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(ErrorResult), TeamCloudSerialize.SerializeObject(ErrorResult));
        }

        public IActionResult ToActionResult() => ErrorResult.ToActionResult();

        public Task<IActionResult> ToActionResultAsync() => Task.FromResult(ToActionResult());
    }
}
