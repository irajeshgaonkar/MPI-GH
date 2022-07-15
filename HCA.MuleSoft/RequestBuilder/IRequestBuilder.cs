using System;
using HCA.MuleSoft.Models.Request;

namespace HCA.MuleSoft.RequestBuilder;

public interface IRequestBuilder<T, U>
{
    T Build(U request);
}

