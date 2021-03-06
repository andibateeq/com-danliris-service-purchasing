﻿using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentPurchaseRequestModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentDeliveryOrderViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentPurchaseRequestViewModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.GarmentDeliveryOrderControllers;
using Com.Moonlay.NetCore.Lib.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.GarmentDeliveryOrderControllerTests
{
    public class GarmentDeliveryOrderControllerTest
    {
        private GarmentDeliveryOrderViewModel ViewModel
        {
            get
            {
                return new GarmentDeliveryOrderViewModel
                {
                    supplier = new SupplierViewModel(),
                    docurrency = new CurrencyViewModel(),
                    incomeTax = new IncomeTaxViewModel(),
                    items = new List<GarmentDeliveryOrderItemViewModel>
                    {
                        new GarmentDeliveryOrderItemViewModel()
                        {
                            currency = new CurrencyViewModel(),
                            fulfillments = new List<GarmentDeliveryOrderFulfillmentViewModel>
                            {
                                new GarmentDeliveryOrderFulfillmentViewModel()
                                {
                                    unit = new UnitViewModel(),
                                    product = new GarmentProductViewModel(),
                                    purchaseOrderUom = new UomViewModel(),
                                    smallUom = new UomViewModel(),

                                }
                            }
                        }
                    }
                };
            }
        }

        private GarmentDeliveryOrder Model
        {
            get
            {
                return new GarmentDeliveryOrder { };
            }
        }

        private ServiceValidationExeption GetServiceValidationExeption()
        {
            Mock<IServiceProvider> serviceProvider = new Mock<IServiceProvider>();
            List<ValidationResult> validationResults = new List<ValidationResult>();
            System.ComponentModel.DataAnnotations.ValidationContext validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(this.ViewModel, serviceProvider.Object, null);
            return new ServiceValidationExeption(validationContext, validationResults);
        }

        private Mock<IServiceProvider> GetServiceProvider()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            return serviceProvider;
        }

        private GarmentDeliveryOrderController GetController(Mock<IGarmentDeliveryOrderFacade> facadeM, Mock<IValidateService> validateM, Mock<IMapper> mapper)
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            var servicePMock = GetServiceProvider();
            if (validateM != null)
            {
                servicePMock
                    .Setup(x => x.GetService(typeof(IValidateService)))
                    .Returns(validateM.Object);
            }

            GarmentDeliveryOrderController controller = new GarmentDeliveryOrderController(servicePMock.Object, mapper.Object, facadeM.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = user.Object
                    }
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer unittesttoken";
            controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/unit-test");
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "7";

            return controller;
        }

        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        [Fact]
        public void Should_Success_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrder>(It.IsAny<GarmentDeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentDeliveryOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Create_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrder>(It.IsAny<GarmentDeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentDeliveryOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var controller = new GarmentDeliveryOrderController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);

            var response = controller.Post(this.ViewModel).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_All_Data_By_User()
        {
            var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentDeliveryOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentDeliveryOrderViewModel>>(It.IsAny<List<GarmentDeliveryOrder>>()))
                .Returns(new List<GarmentDeliveryOrderViewModel> { ViewModel });

            GarmentDeliveryOrderController controller = GetController(mockFacade, null, mockMapper);
            var response = controller.GetByUser();
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_All_Data_By_User_With_Filter()
        {
            var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentDeliveryOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentDeliveryOrderViewModel>>(It.IsAny<List<GarmentDeliveryOrder>>()))
                .Returns(new List<GarmentDeliveryOrderViewModel> { ViewModel });

            GarmentDeliveryOrderController controller = GetController(mockFacade, null, mockMapper);
            var response = controller.GetByUser(filter: "{ 'IsClosed': false }");
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_All_Data()
        {
            var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentDeliveryOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentDeliveryOrderViewModel>>(It.IsAny<List<GarmentDeliveryOrder>>()))
                .Returns(new List<GarmentDeliveryOrderViewModel> { ViewModel });

            GarmentDeliveryOrderController controller = new GarmentDeliveryOrderController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            var response = controller.Get();
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_All_Data_By_User()
        {
            var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
                .Returns(Tuple.Create(new List<GarmentDeliveryOrder>(), 0, new Dictionary<string, string>()));

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<List<GarmentDeliveryOrderViewModel>>(It.IsAny<List<GarmentDeliveryOrder>>()))
                .Returns(new List<GarmentDeliveryOrderViewModel> { ViewModel });

            GarmentDeliveryOrderController controller = new GarmentDeliveryOrderController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            var response = controller.GetByUser();
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new GarmentDeliveryOrder());

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrderViewModel>(It.IsAny<GarmentDeliveryOrder>()))
                .Returns(ViewModel);

            GarmentDeliveryOrderController controller = new GarmentDeliveryOrderController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Get_Data_By_Id()
        {
            var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            mockFacade.Setup(x => x.ReadById(It.IsAny<int>()))
                .Returns(new GarmentDeliveryOrder());

            var mockMapper = new Mock<IMapper>();

            GarmentDeliveryOrderController controller = new GarmentDeliveryOrderController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);
            var response = controller.Get(It.IsAny<int>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public void Should_Success_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrder>(It.IsAny<GarmentDeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();
            mockFacade.Setup(x => x.Update(It.IsAny<int>(), It.IsAny<GarmentDeliveryOrderViewModel>(), It.IsAny<GarmentDeliveryOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentDeliveryOrderViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.Created, GetStatusCode(response));
        }

        [Fact]
        public void Should_Validate_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderViewModel>())).Throws(GetServiceValidationExeption());

            var mockMapper = new Mock<IMapper>();

            var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();

            var controller = GetController(mockFacade, validateMock, mockMapper);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentDeliveryOrderViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.BadRequest, GetStatusCode(response));
        }

        [Fact]
        public void Should_Error_Update_Data()
        {
            var validateMock = new Mock<IValidateService>();
            validateMock.Setup(s => s.Validate(It.IsAny<GarmentDeliveryOrderViewModel>())).Verifiable();

            var mockMapper = new Mock<IMapper>();
            mockMapper.Setup(x => x.Map<GarmentDeliveryOrder>(It.IsAny<GarmentDeliveryOrderViewModel>()))
                .Returns(Model);

            var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();
            mockFacade.Setup(x => x.Create(It.IsAny<GarmentDeliveryOrder>(), "unittestusername", 7))
               .ReturnsAsync(1);

            var controller = new GarmentDeliveryOrderController(GetServiceProvider().Object, mockMapper.Object, mockFacade.Object);

            var response = controller.Put(It.IsAny<int>(), It.IsAny<GarmentDeliveryOrderViewModel>()).Result;
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

		[Fact]
		public void Should_Error_Get_Data_By_Supplier()
		{
			var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();

			mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
				.Returns(Tuple.Create(new List<GarmentDeliveryOrder>(), 0, new Dictionary<string, string>()));

			var mockMapper = new Mock<IMapper>();
			mockMapper.Setup(x => x.Map<List<GarmentDeliveryOrderViewModel>>(It.IsAny<List<GarmentDeliveryOrder>>()))
				.Returns(new List<GarmentDeliveryOrderViewModel> { ViewModel });

			GarmentDeliveryOrderController controller = GetController(mockFacade, null, mockMapper);
			var response = controller.GetBySupplier();
			Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
		}

		[Fact]
		public void Should_Error_Get_Data_For_Customs()
		{
			var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();

			mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
				.Returns(Tuple.Create(new List<GarmentDeliveryOrder>(), 0, new Dictionary<string, string>()));

			var mockMapper = new Mock<IMapper>();
			mockMapper.Setup(x => x.Map<List<GarmentDeliveryOrderViewModel>>(It.IsAny<List<GarmentDeliveryOrder>>()))
				.Returns(new List<GarmentDeliveryOrderViewModel> { ViewModel });

			GarmentDeliveryOrderController controller = GetController(mockFacade, null, mockMapper);
			var response = controller.GetForCustoms();
			Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
		}
		[Fact]
		public void Should_Error_Get_Data_is_Received()
		{
			var mockFacade = new Mock<IGarmentDeliveryOrderFacade>();

			mockFacade.Setup(x => x.Read(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<string>()))
				.Returns(Tuple.Create(new List<GarmentDeliveryOrder>(), 0, new Dictionary<string, string>()));
			
			var mockMapper = new Mock<IMapper>();
			mockMapper.Setup(x => x.Map<List<GarmentDeliveryOrderViewModel>>(It.IsAny<List<GarmentDeliveryOrder>>()))
				.Returns(new List<GarmentDeliveryOrderViewModel> { ViewModel });
			List<int> listID = new List<int>();
			listID.Add(1);

			GarmentDeliveryOrderController controller = GetController(mockFacade, null, mockMapper);
			var response = controller.GetIsReceived(listID);
			Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
		}
	}
}
