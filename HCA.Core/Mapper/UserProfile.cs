using System;
using AutoMapper;
using HCA.Data.Entities;
using HCA.FileProcessor.Models;
using HCA.Models;
using HCA.Models.Request;

namespace HCA.Core.Mapper
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ClientIdentityRequest, ClientIdentityRequestEntity>();
            CreateMap<ClientIdentityRequestEntity, ClientIdentityRequest>();
            CreateMap<FileClientIdentity, ClientIdentityRequestEntity>();
            CreateMap<ClientIdentityRequestEntity, FileClientIdentity>();
        }
    }
}

