﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TT.Abp.ShopManagement.Application.Dtos;
using TT.Abp.ShopManagement.Domain;
using TT.Abp.VisitorManagement.Application.Dtos;
using TT.Abp.VisitorManagement.Domain;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace TT.Abp.VisitorManagement.Application
{
    public interface IFormAppService : IApplicationService
    {
        Task<ListResultDto<FormDto>> GetListAsync();

        Task<FormDto> GetAsync(Guid id);

        Task<FormDto> CreateAsync(FormCreateOrEditDto input);


        Task<FormDto> UpdateAsync(Guid id, FormCreateOrEditDto body);

        Task DeleteAsync(Guid id);

        Task<List<ShopDto>> GetShops(Guid id);
    }

    public class FormAppService : ApplicationService, IFormAppService
    {
        private readonly IRepository<Form, Guid> _repository;
        private readonly IRepository<Shop, Guid> _shopRepository;
        private readonly ICurrentTenant _currentTenant;

        public FormAppService(
            IRepository<Form, Guid> formRepository,
            IRepository<Shop, Guid> shopRepository,
            ICurrentTenant currentTenant)
        {
            ObjectMapperContext = typeof(VisitorManagementModule);
            _repository = formRepository;
            _shopRepository = shopRepository;
            _currentTenant = currentTenant;
        }

        [Authorize]
        public async Task<ListResultDto<FormDto>> GetListAsync()
        {
            var result = await _repository.GetListAsync();

            return new ListResultDto<FormDto>(
                ObjectMapper.Map<List<Form>, List<FormDto>>(result));
        }

        public async Task<FormDto> GetAsync(Guid id)
        {
            var find = await _repository.GetAsync(id);

            return ObjectMapper.Map<Form, FormDto>(find);
        }

        public async Task<FormDto> CreateAsync(FormCreateOrEditDto input)
        {
            var newEntity = await _repository.InsertAsync(new Form(GuidGenerator.Create(),
                input.Title,
                input.Description,
                _currentTenant.Id
            ));
            return ObjectMapper.Map<Form, FormDto>(newEntity);
        }

        public async Task<FormDto> UpdateAsync(Guid id, FormCreateOrEditDto body)
        {
            var find = await _repository.GetAsync(id);

            if (find == null)
            {
                throw new EntityNotFoundException(typeof(Form), body.Title);
            }

            find.SetTitle(body.Title);
            find.SetDescription(body.Description);

            return ObjectMapper.Map<Form, FormDto>(find);
        }


        public async Task DeleteAsync(Guid id)
        {
            var find = await _repository.GetAsync(id);

            if (find == null)
            {
                throw new EntityNotFoundException(typeof(Shop));
            }

            await _repository.DeleteAsync(find);
        }

        /// <summary>
        /// 取得使用这个表单的商家
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<List<ShopDto>> GetShops(Guid id)
        {
            var find = await _repository.Include(x => x.ShopForms).FirstOrDefaultAsync(x => x.Id == id);

            var shopids = find.ShopForms.Select(x => x.ShopId);

            var shops = await _shopRepository.Where(x => shopids.Contains(x.Id)).ToListAsync();

            return ObjectMapper.Map<List<Shop>, List<ShopDto>>(shops);
        }
    }
}